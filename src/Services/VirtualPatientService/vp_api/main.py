import os
import json
import httpx
from typing import Optional, Dict, Any, AsyncGenerator
from fastapi import FastAPI, HTTPException
from fastapi.responses import StreamingResponse
from pydantic import BaseModel
from contextlib import asynccontextmanager
from cachetools import TTLCache

from langchain_ollama import ChatOllama
from langchain_core.messages import HumanMessage, SystemMessage, AIMessage

# ==========================================
# 1. CẤU HÌNH & LOAD TEMPLATE (TÁCH PROMPT)
# ==========================================
OLLAMA_URL = os.getenv("OLLAMA_URL", "http://ollama-vp:11434")
OLLAMA_MODEL = os.getenv("OLLAMA_MODEL", "virtual-patient-model")
VIRTUAL_PATIENT_API_URL = os.getenv(
    "VIRTUAL_PATIENT_API_URL",
    "http://virtualpatient:8080/api/virtual-patients"
)

BASE_DIR = os.path.dirname(__file__)
TEMPLATE_FILE = os.path.join(BASE_DIR, "sys", "prompt_template.json")

try:
    with open(TEMPLATE_FILE, "r", encoding="utf-8") as f:
        PROMPT_CONFIG = json.load(f)
    print(f"Đã load template prompt từ: {TEMPLATE_FILE}")
except Exception as e:
    print(f"Lỗi đọc prompt_template.json. Dùng template mặc định. Lỗi: {e}")
    PROMPT_CONFIG = {
        "template": "You are {name}.\nRecord: {description}\nBehavior: {behaviors}", 
        "initial_greeting": "Good morning, Doctor."
    }

# ==========================================
# 2. FETCH DỮ LIỆU TỪ .NET SERVICE BẰNG HTTPX
# ==========================================
async def fetch_patient_from_service(patient_id: str) -> Optional[Dict[str, str]]:
    async with httpx.AsyncClient(timeout=10.0) as client:
        try:
            response = await client.get(f"{VIRTUAL_PATIENT_API_URL}?page=1&pageSize=5000")
            response.raise_for_status()
            payload = response.json()
            
            items = payload if isinstance(payload, list) else payload.get("items", payload.get("data", []))
            
            for row in items:
                if str(row.get("id", "")).strip() == str(patient_id).strip():
                    name = str(row.get("name") or f"Patient {row.get('id', '')}").strip()
                    description = str(row.get("description") or "").strip()
                    behaviors = str(row.get("behaviors") or "").strip()
                    
                    system_prompt = PROMPT_CONFIG["template"].format(
                        name=name, description=description, behaviors=behaviors
                    )
                    return {
                        "system_prompt": system_prompt,
                        "initial_greeting": PROMPT_CONFIG["initial_greeting"]
                    }
            return None
        except Exception as e:
            print(f"Lỗi kết nối .NET Service: {e}")
            return None

# ==========================================
# 3. BỘ NHỚ HỘI THOẠI
# ==========================================
class MemoryStore:
    def __init__(self):
        self.cache = TTLCache(maxsize=5000, ttl=3600)
    
    def _key(self, doc_id: str, pat_id: str): 
        return f"{doc_id}:{pat_id}"
    
    def add(self, doc_id: str, pat_id: str, q: str, a: str):
        key = self._key(doc_id, pat_id)
        history = self.cache.get(key, [])
        history.append({"q": q, "a": a})
        self.cache[key] = history[-20:] # Nhớ 20 lượt gần nhất
        
    def get(self, doc_id: str, pat_id: str):
        return self.cache.get(self._key(doc_id, pat_id), [])
    
    def clear(self, doc_id: str, pat_id: str):
        self.cache.pop(self._key(doc_id, pat_id), None)

HISTORY = MemoryStore()

# ==========================================
# 4. KẾT NỐI OLLAMA VÀ LẮP RÁP PROMPT
# ==========================================
def get_llm():
    return ChatOllama(
        model=OLLAMA_MODEL,
        base_url=OLLAMA_URL,
        temperature=0.1, 
        top_p=0.9
    )

def build_messages(patient_data: dict, chat_history: list, question: str) -> list:
    messages = [SystemMessage(content=patient_data.get("system_prompt", ""))]
    if not chat_history and patient_data.get("initial_greeting"):
        messages.append(AIMessage(content=patient_data.get("initial_greeting")))
    for turn in chat_history:
        messages.append(HumanMessage(content=turn["q"]))
        messages.append(AIMessage(content=turn["a"]))
    messages.append(HumanMessage(content=question))
    return messages

# ==========================================
# 5. API ROUTING & FASTAPI
# ==========================================
class VPRequest(BaseModel):
    doctor_id: str
    patient_id: str
    question: str

class VPResponse(BaseModel):
    answer: str

@asynccontextmanager
async def lifespan(app: FastAPI):
    print(f"🚀 Virtual Patient Python API đang khởi chạy!")
    print(f"🔗 Ollama URL: {OLLAMA_URL} | Model: {OLLAMA_MODEL}")
    print(f"🔗 .NET Service URL: {VIRTUAL_PATIENT_API_URL}")
    yield

app = FastAPI(title="LATEE Virtual Patient API", lifespan=lifespan)

# --- 5.1 API TRẢ VỀ TEXT THƯỜNG ---
@app.post("/chat", response_model=VPResponse)
async def chat_with_patient(req: VPRequest):
    patient_data = await fetch_patient_from_service(req.patient_id)
    if not patient_data:
        raise HTTPException(404, detail="Không tìm thấy bệnh án trong Database.")

    chat_history = HISTORY.get(req.doctor_id, req.patient_id)
    messages = build_messages(patient_data, chat_history, req.question)
    
    llm = get_llm()
    try:
        # Dùng ainvoke (Async Invoke) thay vì invoke đồng bộ chặn luồng
        response = await llm.ainvoke(messages)
        final_answer = response.content.strip()
    except Exception as e:
        raise HTTPException(503, detail=f"Lỗi Inference từ Ollama: {str(e)}")

    HISTORY.add(req.doctor_id, req.patient_id, req.question, final_answer)
    return VPResponse(answer=final_answer)

# --- 5.2 API STREAMING CHAT ---
@app.post("/stream")
async def chat_with_patient_stream(req: VPRequest):
    patient_data = await fetch_patient_from_service(req.patient_id)
    if not patient_data:
        raise HTTPException(404, detail="Không tìm thấy bệnh án trong Database.")

    chat_history = HISTORY.get(req.doctor_id, req.patient_id)
    messages = build_messages(patient_data, chat_history, req.question)
    llm = get_llm()

    async def generate() -> AsyncGenerator[str, None]:
        full_answer = ""
        try:
            # Dùng astream (Async Stream) thay vì stream
            async for chunk in llm.astream(messages):
                if hasattr(chunk, "content") and chunk.content:
                    full_answer += chunk.content
                    data = {"type": "token", "content": chunk.content}
                    yield f"data: {json.dumps(data, ensure_ascii=False)}\n\n"
            
            HISTORY.add(req.doctor_id, req.patient_id, req.question, full_answer)
            final_data = {"type": "done", "full_answer": full_answer}
            yield f"data: {json.dumps(final_data, ensure_ascii=False)}\n\n"
        except Exception as e:
            error_data = {"type": "error", "message": str(e)}
            yield f"data: {json.dumps(error_data)}\n\n"

    return StreamingResponse(
        generate(),
        media_type="text/event-stream",
        headers={"Cache-Control": "no-cache", "Connection": "keep-alive"}
    )

# --- 5.3 API XÓA LỊCH SỬ ---
@app.post("/reset")
def reset_conversation(req: VPRequest):
    HISTORY.clear(req.doctor_id, req.patient_id)
    return {"message": "Đã xóa trí nhớ bệnh nhân thành công"}