import os
import json
import httpx
from typing import Optional, Dict, Any, List
from fastapi import FastAPI, HTTPException
from fastapi.responses import StreamingResponse
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from cachetools import TTLCache
from langchain_ollama import ChatOllama
from langchain_core.messages import HumanMessage, SystemMessage, AIMessage

# ==========================================
# 1. CONFIG & APP INIT
# ==========================================
OLLAMA_URL = os.getenv("OLLAMA_URL", "http://ollama-vp:11434")
OLLAMA_MODEL = os.getenv("OLLAMA_MODEL", "virtual-patient-model")
VIRTUAL_PATIENT_BASE_URL = os.getenv("VIRTUAL_PATIENT_API_URL", "http://virtualpatient:8080/api/virtual-patients")

PROMPT_CACHE = TTLCache(maxsize=1000, ttl=600) 

app = FastAPI(title="LATEE VP AI - Stateless Architecture")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"], 
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ==========================================
# 2. MODELS (DATA TRANSFER OBJECTS)
# ==========================================
class MessageItem(BaseModel):
    role: str
    content: str

class VPRequest(BaseModel):
    doctor_id: str
    patient_id: str
    question: str
    chat_history: List[MessageItem] = [] 

# ==========================================
# 3. CORE LOGIC & SYSTEM PROMPT
# ==========================================
async def get_patient_detail_from_net(patient_id: str) -> Optional[Dict[str, Any]]:
    async with httpx.AsyncClient() as client:
        try:
            url = f"{VIRTUAL_PATIENT_BASE_URL}/{patient_id}"
            resp = await client.get(url, timeout=10.0)
            if resp.status_code == 200:
                return resp.json()
            return None
        except Exception as e:
            print(f"CONNECTION ERROR to .NET: {e}")
    return None

def _build_system_prompt_from_detail(data: Dict[str, Any]) -> Dict[str, str]:
    name = data.get("name", "Patient")
    age = data.get("age", "Unknown")
    gender = data.get("gender", "Unknown")
    occupation = data.get("occupation", "Unknown")
    description = data.get("description", "")
    chief_concern = data.get("chiefConcern", "")
    vitals = data.get("vitalSigns") or {}
    persona = data.get("persona") or {}
    
    emotional_state = persona.get("emotional_state", "Neutral")
    rules = persona.get("behavioral_rules") or []
    behavioral_rules_str = "\n".join([f"- {rule}" for rule in rules])
    vitals_str = ", ".join([f"{k}: {v}" for k, v in vitals.items()])

    system_prompt = (
        f"You are {name}, a {age}-year-old {gender} working as a {occupation}.\n"
        "STRICT ROLEPLAY: You are a human patient in a clinical examination. You are NOT an AI assistant.\n\n"

        "*** PERSONALITY & BEHAVIOR (Persona) ***\n"
        f"- Mood: {emotional_state}\n{behavioral_rules_str}\n\n"

        "*** GROUND TRUTH (YOUR MEDICAL RECORD) ***\n"
        f"- Concern: {chief_concern}\n- Vitals: {vitals_str}\n- History: {description}\n\n"

         "*** CRITICAL INSTRUCTIONS ***\n"
        "1. STAY IN CHARACTER: You are a normal human patient. If asked about medical knowledge, act unsure or confused.\n"
        "2. GROUND TRUTH: Answer ONLY based on your Medical Record. Do NOT invent symptoms or medical facts.\n"
        "3. LIMIT DISCLOSURE: Answer using no more than 20 words. Only answer what is asked. Do not provide extra or unrelated information.\n"
    )
    
    return {
        "system_prompt": system_prompt,
        "initial_greeting": f"Good morning, Doctor..."
    }
    
async def get_effective_prompt(patient_id: str):
    if patient_id in PROMPT_CACHE: return PROMPT_CACHE[patient_id]
    detail = await get_patient_detail_from_net(patient_id)
    if not detail: return None
    prompt_data = _build_system_prompt_from_detail(detail)
    PROMPT_CACHE[patient_id] = prompt_data
    return prompt_data

def prepare_messages(data, chat_history_from_fe, question):
    messages = [SystemMessage(content=data["system_prompt"])]
    
    if not chat_history_from_fe:
        messages.append(AIMessage(content=data["initial_greeting"]))
    else:
        for msg in chat_history_from_fe:
            if msg.role == 'doctor':
                messages.append(HumanMessage(content=msg.content))
            elif msg.role == 'patient':
                messages.append(AIMessage(content=msg.content))
                
    messages.append(HumanMessage(content=question))
    return messages

# ==========================================
# 4. API ENDPOINTS
# ==========================================
@app.post("/chat")
async def chat_with_patient(req: VPRequest):
    data = await get_effective_prompt(req.patient_id)
    if not data: raise HTTPException(404, detail="Bệnh án không tồn tại.")

    messages = prepare_messages(data, req.chat_history, req.question)
    
    llm = ChatOllama(model=OLLAMA_MODEL, base_url=OLLAMA_URL, temperature=0.0)
    response = await llm.ainvoke(messages)
    
    return {"answer": response.content.strip()}

@app.post("/stream")
async def chat_with_patient_stream(req: VPRequest):
    data = await get_effective_prompt(req.patient_id)
    if not data: raise HTTPException(404, detail="Bệnh án không tồn tại.")

    messages = prepare_messages(data, req.chat_history, req.question)

    llm = ChatOllama(model=OLLAMA_MODEL, base_url=OLLAMA_URL, temperature=0.1)

    async def generate():
        try:
            async for chunk in llm.astream(messages):
                if hasattr(chunk, "content") and chunk.content:
                    yield f"data: {json.dumps({'type': 'token', 'content': chunk.content}, ensure_ascii=False)}\n\n"
            
            yield f"data: {json.dumps({'type': 'done'})}\n\n"
        except Exception as e:
            yield f"data: {json.dumps({'type': 'error', 'message': str(e)})}\n\n"

    return StreamingResponse(
        generate(), 
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "Connection": "keep-alive",
            "X-Accel-Buffering": "no" 
        }
    )
