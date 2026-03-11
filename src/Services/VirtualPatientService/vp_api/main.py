import os
import json
import asyncio
from typing import Optional, Dict, Any, AsyncGenerator
from fastapi import FastAPI, HTTPException
from fastapi.responses import StreamingResponse
from pydantic import BaseModel
from contextlib import asynccontextmanager
from cachetools import TTLCache
from urllib.request import urlopen, Request
from urllib.parse import urlencode
from urllib.error import URLError, HTTPError
from concurrent.futures import ThreadPoolExecutor

# Langchain tích hợp trực tiếp với Ollama
from langchain_ollama import ChatOllama
from langchain_core.messages import HumanMessage, SystemMessage, AIMessage

# ==========================================
# 1. CẤU HÌNH & LOAD FILE JSON LINH HOẠT
# ==========================================
OLLAMA_URL = os.getenv("OLLAMA_URL", "http://ollama-vp:11434")
OLLAMA_MODEL = os.getenv("OLLAMA_MODEL", "virtual-patient-model")
VIRTUAL_PATIENT_API_URL = os.getenv(
    "VIRTUAL_PATIENT_API_URL",
    "http://virtualpatient-api:8080/api/virtual-patients"
)

# TỰ ĐỘNG LẤY ĐƯỜNG DẪN BẤT KỂ WINDOWS HAY DOCKER LINUX
BASE_DIR = os.path.dirname(__file__)
PROMPTS_FILE = os.path.join(BASE_DIR, "sys", "system_prompts.json")

try:
    with open(PROMPTS_FILE, "r", encoding="utf-8") as f:
        PATIENT_DB = json.load(f)
    print(f"Đã load thành công dữ liệu từ: {PROMPTS_FILE}")
except Exception as e:
    print(f"Cảnh báo: Không thể đọc file {PROMPTS_FILE}. Đang dùng data rỗng. Lỗi: {e}")
    PATIENT_DB = {}

# Khởi tạo ThreadPool cho tác vụ HTTP gọi qua .NET
executor = ThreadPoolExecutor(max_workers=4)

# ==========================================
# 2. HELPER FETCH DỮ LIỆU TỪ .NET SERVICE
# ==========================================
def _extract_items(payload: Any) -> list[dict]:
    if isinstance(payload, list):
        return [x for x in payload if isinstance(x, dict)]
    if isinstance(payload, dict):
        for key in ("items", "data", "results"):
            value = payload.get(key)
            if isinstance(value, list):
                return [x for x in value if isinstance(x, dict)]
    return []

def _build_prompt_from_service_row(row: Dict[str, Any]) -> Dict[str, str]:
    name = str(row.get("name") or f"Patient {row.get('id', '')}").strip()
    description = str(row.get("description") or "").strip()
    behaviors = str(row.get("behaviors") or "").strip()

    # Sử dụng cấu trúc Tiếng Anh chuẩn để tương thích 100% với Adapter Llama 3.1
    system_prompt = (
        f"You are {name}, a patient in a clinical exam.\n"
        "You are NOT an AI assistant. You are a human patient.\n\n"
        "*** CRITICAL INSTRUCTIONS ***\n"
        "1. STAY IN CHARACTER: You do NOT know how to write code, do math, or answer general knowledge questions. You are just a regular person.\n"
        "2. REFUSE OUT-OF-SCOPE QUESTIONS: If the doctor asks you to write code, calculate math, or talk about politics/history, you must REFUSE.\n"
        "   - React with confusion: \"Doctor, what are you talking about? I don't know computer stuff.\"\n"
        f"3. BEHAVIOR & EMOTION: {behaviors}\n"
        "4. GROUND TRUTH ONLY: Answer medical questions based ONLY on the Patient Record below. Do NOT makeup symptoms.\n\n"
        "=== PATIENT RECORD ===\n"
        f"{description}\n"
        "======================"
    )
    
    return {
        "system_prompt": system_prompt,
        "initial_greeting": f"Good morning, Doctor."
    }

def fetch_patient_from_service(patient_id: str) -> Optional[Dict[str, str]]:
    query = urlencode({"page": 1, "pageSize": 5000})
    url = f"{VIRTUAL_PATIENT_API_URL}?{query}"
    req = Request(url, headers={"Accept": "application/json"})

    try:
        with urlopen(req, timeout=8) as resp:
            payload = json.loads(resp.read().decode("utf-8"))
        items = _extract_items(payload)
        for row in items:
            if str(row.get("id", "")).strip() == str(patient_id).strip():
                return _build_prompt_from_service_row(row)
    except Exception as e:
        print(f"Lỗi khi gọi service .NET: {e}")
    return None

async def get_patient_async(patient_id: str):
    if patient_id in PATIENT_DB:
        return PATIENT_DB[patient_id]
    
    loop = asyncio.get_event_loop()
    data = await loop.run_in_executor(executor, fetch_patient_from_service, patient_id)
    if data:
        PATIENT_DB[patient_id] = data
    return data

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
        key = self._key(doc_id, pat_id)
        if key in self.cache: del self.cache[key]

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
    # 1. Nạp System Prompt
    messages = [SystemMessage(content=patient_data.get("system_prompt", ""))]
    
    # 2. Câu chào ban đầu nếu chưa có lịch sử
    if not chat_history and patient_data.get("initial_greeting"):
        messages.append(AIMessage(content=patient_data.get("initial_greeting")))
    
    # 3. Nạp lịch sử
    for turn in chat_history:
        messages.append(HumanMessage(content=turn["q"]))
        messages.append(AIMessage(content=turn["a"]))
    
    # 4. Chèn câu hỏi hiện tại
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
    print(f"Virtual Patient Python API đang khởi chạy!")
    print(f"Ollama URL: {OLLAMA_URL} | Model: {OLLAMA_MODEL}")
    print(f".NET Service URL: {VIRTUAL_PATIENT_API_URL}")
    yield

app = FastAPI(title="LATEE Virtual Patient API", lifespan=lifespan)

# --- 5.1 API TRẢ VỀ TEXT THƯỜNG ---
@app.post("/chat", response_model=VPResponse)
async def chat_with_patient(req: VPRequest):
    patient_data = await get_patient_async(req.patient_id)

    if not patient_data:
        raise HTTPException(404, detail="Không tìm thấy bệnh án trong JSON và DB.")

    chat_history = HISTORY.get(req.doctor_id, req.patient_id)
    messages = build_messages(patient_data, chat_history, req.question)
    
    llm = get_llm()
    try:
        loop = asyncio.get_event_loop()
        response = await loop.run_in_executor(executor, lambda: llm.invoke(messages))
        final_answer = response.content.strip()
    except Exception as e:
        raise HTTPException(503, detail=f"Lỗi Inference từ Ollama: {str(e)}")

    HISTORY.add(req.doctor_id, req.patient_id, req.question, final_answer)
    return VPResponse(answer=final_answer)

# --- 5.2 API STREAMING CHAT ---
@app.post("/stream")
async def chat_with_patient_stream(req: VPRequest):
    patient_data = await get_patient_async(req.patient_id)

    if not patient_data:
        raise HTTPException(404, detail="Không tìm thấy bệnh án trong JSON và DB.")

    chat_history = HISTORY.get(req.doctor_id, req.patient_id)
    messages = build_messages(patient_data, chat_history, req.question)
    llm = get_llm()

    async def generate() -> AsyncGenerator[str, None]:
        full_answer = ""
        try:
            for chunk in llm.stream(messages):
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