import os
import json
import httpx
import asyncio
import torch
from typing import Optional, Dict, Any, List
from fastapi import FastAPI, HTTPException
from fastapi.responses import StreamingResponse
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from cachetools import TTLCache
from transformers import AutoTokenizer, AutoModelForCausalLM, TextIteratorStreamer
from peft import PeftModel
from threading import Thread

# ==========================================
# 1. CONFIG & APP INIT
# ==========================================
BASE_MODEL_ID   = os.getenv("BASE_MODEL_ID",   "meta-llama/Llama-3.1-8B-Instruct")
ADAPTER_PATH    = os.getenv("ADAPTER_PATH",    "/app/adapter")
HF_TOKEN        = os.getenv("HF_TOKEN",        None)   
DEVICE          = os.getenv("DEVICE",          "cuda") 
MAX_NEW_TOKENS  = int(os.getenv("MAX_NEW_TOKENS", "256"))

VIRTUAL_PATIENT_BASE_URL = os.getenv(
    "VIRTUAL_PATIENT_API_URL",
    "http://virtualpatient:8080/api/virtual-patients"
)

PROMPT_CACHE = TTLCache(maxsize=1000, ttl=600)

app = FastAPI(title="LATEE VP AI – HuggingFace + LoRA Adapter")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ==========================================
# 2. MODEL LOADING (once at startup)
# ==========================================
tokenizer: AutoTokenizer = None
model: AutoModelForCausalLM = None

@app.on_event("startup")
async def load_model():
    global tokenizer, model

    print(f"[STARTUP] Loading base model: {BASE_MODEL_ID}")
    tokenizer = AutoTokenizer.from_pretrained(
        BASE_MODEL_ID,
        token=HF_TOKEN,
        trust_remote_code=True,
    )
    if tokenizer.pad_token is None:
        tokenizer.pad_token = tokenizer.eos_token

    base = AutoModelForCausalLM.from_pretrained(
        BASE_MODEL_ID,
        token=HF_TOKEN,
        torch_dtype=torch.float16 if DEVICE == "cuda" else torch.float32,
        device_map="auto" if DEVICE == "cuda" else None,
        trust_remote_code=True,
    )

    if os.path.isdir(ADAPTER_PATH) and os.listdir(ADAPTER_PATH):
        print(f"[STARTUP] Merging LoRA adapter from: {ADAPTER_PATH}")
        base = PeftModel.from_pretrained(base, ADAPTER_PATH)
        base = base.merge_and_unload()          
        print("[STARTUP] Adapter merged successfully.")
    else:
        print(f"[STARTUP] No adapter found at {ADAPTER_PATH}, using base model only.")

    if DEVICE == "cpu":
        base = base.to("cpu")

    base.eval()
    model = base
    print("[STARTUP] Model ready.")


# ==========================================
# 3. MODELS (DTOs)
# ==========================================
class MessageItem(BaseModel):
    role: str      # "doctor" | "patient"
    content: str

class VPRequest(BaseModel):
    doctor_id: str
    patient_id: str
    question: str
    chat_history: List[MessageItem] = []


# ==========================================
# 4. PATIENT DETAIL & SYSTEM PROMPT
# ==========================================
async def get_patient_detail_from_net(patient_id: str) -> Optional[Dict[str, Any]]:
    async with httpx.AsyncClient() as client:
        try:
            resp = await client.get(
                f"{VIRTUAL_PATIENT_BASE_URL}/{patient_id}", timeout=10.0
            )
            if resp.status_code == 200:
                return resp.json()
        except Exception as e:
            print(f"[NET ERROR] {e}")
    return None


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


def _build_system_prompt(data: Dict[str, Any]) -> Dict[str, str]:
    name        = data.get("name", "Patient")
    age         = data.get("age", "Unknown")
    gender      = data.get("gender", "Unknown")
    occupation  = data.get("occupation", "Unknown")
    chief_concern   = data.get("chiefConcern", "")
    medical_history = data.get("medicalHistory", "")
    symptom         = data.get("symptom", "")

    vitals   = _parse_json_or_value(data.get("vitalSigns")) or {}
    persona  = _parse_json_or_value(data.get("persona"))    or {}
    case_rule    = _parse_json_or_value(data.get("caseRule"))
    instructions = _parse_json_or_value(data.get("instructions"))

    emotional_state  = persona.get("emotional_state", "Neutral")
    rules            = persona.get("behavioral_rules") or []
    behavioral_rules = "\n".join(f"- {r}" for r in rules)
    vitals_str       = ", ".join(f"{k}: {v}" for k, v in vitals.items()) if isinstance(vitals, dict) else str(vitals)

    def _fmt(obj):
        if isinstance(obj, dict):  return json.dumps(obj, ensure_ascii=False)
        if isinstance(obj, list):  return "\n".join(f"- {x}" for x in obj)
        return str(obj) if obj else ""

    system_prompt = (
        f"You are {name}, a {age}-year-old {gender} working as a {occupation}.\n"
        "STRICT ROLEPLAY: You are a human patient in a clinical examination. You are NOT an AI assistant.\n\n"
        "*** PERSONALITY & BEHAVIOR ***\n"
        f"- Mood: {emotional_state}\n{behavioral_rules}\n\n"
        "*** YOUR MEDICAL RECORD ***\n"
        f"- Main Symptom: {symptom or chief_concern}\n"
        f"- Current Concern: {chief_concern}\n"
        f"- Current Vitals: {vitals_str}\n"
        f"- Medical History: {medical_history}\n\n"
        "*** CASE RULES ***\n"
        f"- Rules: {_fmt(case_rule)}\n"
        f"- Instructions: {_fmt(instructions)}\n\n"
        "*** CRITICAL INSTRUCTIONS ***\n"
        "1. STAY IN CHARACTER: You are a normal human patient.\n"
        "2. Answer ONLY based on your Medical Record. Do NOT invent symptoms.\n"
        "3. Keep replies ≤ 20 words. Answer only what is asked.\n"
    )
    return {
        "system_prompt": system_prompt,
        "initial_greeting": "Good morning, Doctor...",
    }


async def get_effective_prompt(patient_id: str):
    if patient_id in PROMPT_CACHE:
        return PROMPT_CACHE[patient_id]
    detail = await get_patient_detail_from_net(patient_id)
    if not detail:
        return None
    prompt_data = _build_system_prompt(detail)
    PROMPT_CACHE[patient_id] = prompt_data
    return prompt_data


# ==========================================
# 5. MESSAGE BUILDING (chat template)
# ==========================================
def build_messages(data: Dict, chat_history: List[MessageItem], question: str) -> List[Dict]:
    """
    Build the messages list in the standard chat format accepted by
    tokenizer.apply_chat_template().
    """
    messages = [{"role": "system", "content": data["system_prompt"]}]

    if not chat_history:
        messages.append({"role": "assistant", "content": data["initial_greeting"]})
    else:
        for msg in chat_history:
            role = "user" if msg.role == "doctor" else "assistant"
            messages.append({"role": role, "content": msg.content})

    messages.append({"role": "user", "content": question})
    return messages


def tokenize(messages: List[Dict]):
    prompt = tokenizer.apply_chat_template(
        messages,
        tokenize=False,
        add_generation_prompt=True,
    )
    return tokenizer(prompt, return_tensors="pt").to(model.device)


# ==========================================
# 6. API ENDPOINTS
# ==========================================
@app.post("/chat")
async def chat_with_patient(req: VPRequest):
    data = await get_effective_prompt(req.patient_id)
    if not data:
        raise HTTPException(404, detail="Bệnh án không tồn tại.")

    messages = build_messages(data, req.chat_history, req.question)
    inputs   = tokenize(messages)

    with torch.no_grad():
        output_ids = model.generate(
            **inputs,
            max_new_tokens=MAX_NEW_TOKENS,
            do_sample=False,
            temperature=1.0,           
            pad_token_id=tokenizer.eos_token_id,
        )

    generated = output_ids[0][inputs["input_ids"].shape[-1]:]
    answer    = tokenizer.decode(generated, skip_special_tokens=True).strip()
    return {"answer": answer}


@app.post("/stream")
async def chat_with_patient_stream(req: VPRequest):
    data = await get_effective_prompt(req.patient_id)
    if not data:
        raise HTTPException(404, detail="Bệnh án không tồn tại.")

    messages = build_messages(data, req.chat_history, req.question)
    inputs   = tokenize(messages)

    streamer = TextIteratorStreamer(
        tokenizer,
        skip_prompt=True,
        skip_special_tokens=True,
    )

    generate_kwargs = dict(
        **inputs,
        streamer=streamer,
        max_new_tokens=MAX_NEW_TOKENS,
        do_sample=True,
        temperature=0.1,
        pad_token_id=tokenizer.eos_token_id,
    )

    thread = Thread(target=model.generate, kwargs=generate_kwargs)

    async def generate():
        loop = asyncio.get_event_loop()
        await loop.run_in_executor(None, thread.start)
        try:
            for token_text in streamer:
                if token_text:
                    yield f"data: {json.dumps({'type': 'token', 'content': token_text}, ensure_ascii=False)}\n\n"
            yield f"data: {json.dumps({'type': 'done'})}\n\n"
        except Exception as e:
            yield f"data: {json.dumps({'type': 'error', 'message': str(e)})}\n\n"

    return StreamingResponse(
        generate(),
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "Connection": "keep-alive",
            "X-Accel-Buffering": "no",
        },
    )


@app.get("/health")
async def health():
    return {"status": "ok", "model_loaded": model is not None}