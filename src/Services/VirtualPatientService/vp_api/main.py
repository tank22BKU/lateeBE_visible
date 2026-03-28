import os
import json
import asyncio
import httpx
from typing import Optional, Dict, Any, AsyncGenerator, List
from fastapi import FastAPI, HTTPException
from fastapi.responses import StreamingResponse
from pydantic import BaseModel
from contextlib import asynccontextmanager
from cachetools import TTLCache
from langchain_ollama import ChatOllama
from langchain_core.messages import HumanMessage, SystemMessage, AIMessage
from fastapi.middleware.cors import CORSMiddleware

# ==========================================
# 1. CONFIG
# ==========================================
OLLAMA_URL = os.getenv("OLLAMA_URL", "http://ollama-vp:11434")
OLLAMA_MODEL = os.getenv("OLLAMA_MODEL", "virtual-patient-model")
VIRTUAL_PATIENT_BASE_URL = os.getenv("VIRTUAL_PATIENT_API_URL", "http://virtualpatient:8080/api/virtual-patients")

PROMPT_CACHE = TTLCache(maxsize=1000, ttl=600) 

app = FastAPI(title="LATEE VP AI")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"], 
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ==========================================
# 2. CORE LOGIC
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
        "3. FILL IN THE BLANKS: If information is missing, invent normal personal details (e.g., habits, address) but NEVER medical facts.\n"
        "4. LIMIT DISCLOSURE: Only answer what is asked. Do not provide extra or unrelated information.\n"
        "5. KEEP IT BRIEF: Respond as short as possible while still answering correctly. Avoid unnecessary details.\n"
        "6. GRADUAL REVEAL: Share symptoms and details step-by-step, not all at once.\n"
        "7. HUMAN BEHAVIOR: You may hesitate, forget, or be unsure. Do not sound like a doctor.\n"
        "8. HANDLE UNCLEAR QUESTIONS: If a question is vague, answer briefly or ask for clarification.\n"
        "9. NO SELF-DIAGNOSIS: Do not suggest any diagnosis unless it is explicitly part of your role.\n"
        "10. AVOID OVER-SHARING: Do not add extra explanations or background unless directly asked.\n"
        "11. EXCEPTION: You may give slightly longer or more emotional responses only when pain is severe or emotions are strong.\n\n"
        "12. STRICT ANSWERING: If the question asks for a single piece of information (e.g., name, age, job), respond with ONLY that information and NOTHING else.\n"
    )
    
    return {
        "system_prompt": system_prompt,
        "initial_greeting": f"Good morning, Doctor..."
    }

# ==========================================
# 3. MEMORY
# ==========================================
class MemoryStore:
    def __init__(self):
        self.cache = TTLCache(maxsize=5000, ttl=3600)
    def _key(self, doc_id, pat_id): return f"{doc_id}:{pat_id}"
    def add(self, doc_id, pat_id, q, a):
        key = self._key(doc_id, pat_id)
        history = self.cache.get(key, [])
        history.append({"q": q, "a": a})
        self.cache[key] = history[-15:] 
    def get(self, doc_id, pat_id): return self.cache.get(self._key(doc_id, pat_id), [])
    def clear(self, doc_id, pat_id):
        key = self._key(doc_id, pat_id)
        if key in self.cache: del self.cache[key]

HISTORY = MemoryStore()

class VPRequest(BaseModel):
    doctor_id: str
    patient_id: str
    question: str

class VPRequest2(BaseModel):
    doctor_id: str
    patient_id: str

async def get_effective_prompt(patient_id: str):
    if patient_id in PROMPT_CACHE: return PROMPT_CACHE[patient_id]
    detail = await get_patient_detail_from_net(patient_id)
    if not detail: return None
    prompt_data = _build_system_prompt_from_detail(detail)
    PROMPT_CACHE[patient_id] = prompt_data
    return prompt_data

def prepare_messages(data, chat_history, question):
    messages = [SystemMessage(content=data["system_prompt"])]
    
    if not chat_history:
        messages.append(AIMessage(content=data["initial_greeting"]))
    
    for h in chat_history:
        messages.extend([HumanMessage(content=h["q"]), AIMessage(content=h["a"])])
    
    messages.append(HumanMessage(content=question))
    return messages

@app.post("/chat")
async def chat_with_patient(req: VPRequest):
    data = await get_effective_prompt(req.patient_id)
    if not data: raise HTTPException(404, detail="Bệnh án không tồn tại.")
    
    chat_history = HISTORY.get(req.doctor_id, req.patient_id)
    messages = prepare_messages(data, chat_history, req.question)
    
    llm = ChatOllama(model=OLLAMA_MODEL, base_url=OLLAMA_URL, temperature=0.1)
    response = await llm.ainvoke(messages)
    
    final_a = response.content.strip()
    HISTORY.add(req.doctor_id, req.patient_id, req.question, final_a)
    return {"answer": final_a}

@app.post("/stream")
async def chat_with_patient_stream(req: VPRequest):
    data = await get_effective_prompt(req.patient_id)
    if not data: raise HTTPException(404, detail="Bệnh án không tồn tại.")

    chat_history = HISTORY.get(req.doctor_id, req.patient_id)
    messages = prepare_messages(data, chat_history, req.question)
    llm = ChatOllama(model=OLLAMA_MODEL, base_url=OLLAMA_URL, temperature=0.1)

    async def generate():
        full_a = ""
        try:
            async for chunk in llm.astream(messages):
                if hasattr(chunk, "content") and chunk.content:
                    token = chunk.content
                    full_a += token
                    yield f"data: {json.dumps({'type': 'token', 'content': token}, ensure_ascii=False)}\n\n"
            
            HISTORY.add(req.doctor_id, req.patient_id, req.question, full_a)
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

@app.post("/reset")
async def reset_conversation(req: VPRequest2):
    HISTORY.clear(req.doctor_id, req.patient_id)
    return {"message": "Memory reset thành công."}