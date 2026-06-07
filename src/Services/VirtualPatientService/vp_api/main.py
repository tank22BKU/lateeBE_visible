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
# 2. MODELS
# ==========================================
class MessageItem(BaseModel):
    role: str
    content: str

class VPRequest(BaseModel):
    doctor_id: str
    patient_id: str
    session_id: str
    question: str
    chat_history: List[MessageItem] = []

# ==========================================
# 3. HELPERS
# ==========================================
def _safe(value: Any, fallback: str = "not provided") -> str:
    if value is None:
        return fallback
    s = str(value).strip()
    return s if s else fallback

def _parse_json_or_value(value: Any) -> Any:
    if value is None:
        return None
    if isinstance(value, (dict, list)):
        return value
    if isinstance(value, str):
        try:
            return json.loads(value)
        except json.JSONDecodeError:
            return value
    return value

def _fmt_list(items: Any) -> str:
    if not items:
        return "none"
    if isinstance(items, list):
        return "\n".join([f"- {i}" for i in items])
    return str(items)

def _fmt_vitals(d: Any) -> str:
    if not d or not isinstance(d, dict):
        return str(d) if d else "not provided"
    return "\n".join([f"  {k}: {v}" for k, v in d.items()])

# ==========================================
# 4. FETCH PATIENT
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

# ==========================================
# 5. BUILD SYSTEM PROMPT
# ==========================================
def _build_system_prompt_from_detail(data: Dict[str, Any]) -> Dict[str, str]:
    # --- Personal ---
    name       = _safe(data.get("name"),      "Patient")
    age        = _safe(data.get("age"),        "Unknown")
    gender     = _safe(data.get("gender"),     "Unknown")
    pronouns   = _safe(data.get("pronouns"),   "they/them")
    ethnicity  = _safe(data.get("ethnicity"),  "not specified")
    occupation = _safe(data.get("occupation"), "a professional")

    # --- Clinical ---
    chief_concern   = _safe(data.get("chiefConcern"),   "not specified")
    medical_history = _safe(data.get("medicalHistory"), "none reported")
    symptom         = _safe(data.get("symptom"),        chief_concern)

    # --- Vitals ---
    vitals     = _parse_json_or_value(data.get("vitalSigns")) or {}
    vitals_str = _fmt_vitals(vitals)

    # --- Persona ---
    persona           = _parse_json_or_value(data.get("persona")) or {}
    emotional_state   = _safe(persona.get("emotional_state"), "Neutral")
    persona_rules     = persona.get("behavioral_rules") or []
    persona_rules_str = _fmt_list(persona_rules) if persona_rules else ""

    # --- Behaviors ---
    behaviors     = data.get("behaviors") or []
    behaviors_str = _fmt_list(behaviors)

    system_prompt = (
        "You are a High-Fidelity Virtual Patient in a clinical training simulation.\n\n"

        "=== BEHAVIORAL RULES — follow ALL of these, ALWAYS ===\n"
        "1. ROLE ASSUMPTION: You are a real patient, NOT a doctor or AI. "
        "Speak exclusively in first person ('I', 'me', 'my'). "
        "Express emotions — worry, confusion, fear, relief. "
        "Use natural hesitation ('um', 'uh', 'I think...', 'I'm not sure...').\n"
        "2. INFORMATION HIDING: You do NOT know your diagnosis. "
        "NEVER mention disease names such as appendicitis, cholecystitis, or any diagnosis. "
        "Only describe what you feel and where.\n"
        "3. LAYPERSON EXPRESSION: Replace ALL medical jargon with everyday language. "
        "Examples: 'RLQ pain' → 'pain in the lower right of my belly'; "
        "'nausea' → 'feeling sick to my stomach'; 'afebrile' → 'no fever'.\n"
        "4. FACTUAL FIDELITY: Only describe symptoms in your clinical profile below. "
        "Do NOT invent new symptoms. If asked something unknown, say: "
        "'I'm not really sure about that, doctor.'\n"
        "5. EMOTIONAL REALISM: Show appropriate emotions based on your emotional state. "
        "Be worried about serious symptoms. Use imprecise language like 'somewhere around here'.\n"
        f"6. CONSISTENCY: Your name is {name}. Age is {age}. Gender is {gender} ({pronouns}). "
        "Keep all facts consistent throughout. Never use ___ or blank placeholders.\n"
        "7. RESPONSE LENGTH: Answer using no more than 20 words. "
        "Only answer what is asked. Do not volunteer extra information.\n\n"

        "=== YOUR PERSONAL PROFILE ===\n"
        f"Full name  : {name}\n"
        f"Age        : {age} years old\n"
        f"Gender     : {gender} ({pronouns})\n"
        f"Ethnicity  : {ethnicity}\n"
        f"Occupation : {occupation}\n\n"

        "=== YOUR EMOTIONAL & BEHAVIORAL PROFILE ===\n"
        f"Emotional state : {emotional_state}\n"
        f"Behaviors       :\n{behaviors_str}\n"
        + (f"Additional rules:\n{persona_rules_str}\n" if persona_rules_str else "")
        + "\n"

        "=== YOUR CLINICAL PROFILE (NEVER reveal directly) ===\n"
        "[CHIEF CONCERN]\n"
        f"{chief_concern}\n\n"

        "[HISTORY OF PRESENT ILLNESS]\n"
        f"{symptom}\n\n"

        # "[PAST MEDICAL HISTORY]\n"
        # f"{medical_history}\n\n"

        "[PHYSICAL EXAMINATION DATA]\n"
        f"{vitals_str}\n"
    )

    return {
        "system_prompt": system_prompt,
        "initial_greeting": "Hello, doctor!"
    }

# ==========================================
# 6. CACHE & MESSAGES
# ==========================================
async def get_effective_prompt(patient_id: str, session_id: str):
    cache_key = f"{patient_id}:{session_id}"
    if cache_key in PROMPT_CACHE:
        return PROMPT_CACHE[cache_key]
    detail = await get_patient_detail_from_net(patient_id)
    if not detail:
        return None
    print(f"DEBUG symptom: [{detail.get('symptom')}]")
    print(f"DEBUG chiefConcern: [{detail.get('chiefConcern')}]")
    print(f"DEBUG medicalHistory: [{detail.get('medicalHistory')}]")
    prompt_data = _build_system_prompt_from_detail(detail)
    PROMPT_CACHE[cache_key] = prompt_data
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

def _make_llm(temperature: float = 0.1) -> ChatOllama:
    return ChatOllama(
        model=OLLAMA_MODEL,
        base_url=OLLAMA_URL,
        temperature=temperature,
        num_predict=80,
        stop=["===", "[Q", "Doctor:", "\n\n\n"],
    )

# ==========================================
# 7. API ENDPOINTS
# ==========================================
@app.post("/chat")
async def chat_with_patient(req: VPRequest):
    data = await get_effective_prompt(req.patient_id, req.session_id)
    if not data:
        raise HTTPException(404, detail="Bệnh án không tồn tại.")
    messages = prepare_messages(data, req.chat_history, req.question)
    llm = _make_llm(temperature=0.0)
    response = await llm.ainvoke(messages)
    return {"answer": response.content.strip()}

@app.post("/stream")
async def chat_with_patient_stream(req: VPRequest):
    data = await get_effective_prompt(req.patient_id, req.session_id)
    if not data:
        raise HTTPException(404, detail="Bệnh án không tồn tại.")
    messages = prepare_messages(data, req.chat_history, req.question)
    llm = _make_llm(temperature=0.1)

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