from langchain_core.messages import SystemMessage
from dtos import ClinicalReasoningRequest, ClinicalReasoningResponse
from fastapi import HTTPException
import asyncio
import json
from fastapi.responses import StreamingResponse
from openai import OpenAI
import os
import time
import re
from config import logger

from config import (
    CLINICAL_REASONING_PROMPT,
    DIFY_PROMPT,
    DIFY_PROMPT_VER2,
    DIFY_PROMPT_V3,
    DIFY_PROMPT_V4,
    DIFY_PROMPT_V4_1
)


ALL_DIMENSIONS = [
    {"id": "evidence", "label": "Evidence Base"},
    {"id": "differential", "label": "Differential Diagnosis"},
    {"id": "contradiction", "label": "Contradictory Findings"},
    {"id": "pathophysiology", "label": "Pathophysiology"},
    {"id": "missing_info", "label": "Missing Information"},
    {"id": "danger_priority", "label": "Prioritize Dangerous Diagnosis"},
    {"id": "confidence", "label": "Diagnostic Confidence"},
    {"id": "next_step", "label": "Next Clinical Action"},
]

async def clinical_reasoning_stream_hf(req: ClinicalReasoningRequest):
    #logger.info(f"[START][HF] reasoning : {req.interaction_history}")
    try:
        client = OpenAI(
            base_url="https://router.huggingface.co/v1",
            api_key=os.getenv("HF_TOKEN"),
        )
    except Exception as e:
        raise HTTPException(status_code=503, detail=f"HF LLM not available: {e}")

    # =====================
    # Build prompt (GIỮ NGUYÊN)
    # =====================
    history_items = []
    valid_ids = {d["id"] for d in ALL_DIMENSIONS}
    used_dimension_ids = set()
    if req.interaction_history:
        for q in req.interaction_history:
            dimension = (str(q.dimension or "").strip().lower())

            if dimension in valid_ids:
                used_dimension_ids.add(dimension)
                
            history_items.append({
                "question_dimension": q.dimension,
                "ai_question": q.question,
                "learner_answer": q.answer
            })

    logger.info(f"Used dimensions: {used_dimension_ids}")
    available_dimensions = [d["id"] for d in ALL_DIMENSIONS if d["id"] not in used_dimension_ids]
    
    logger.info(f"Available dimensions: {available_dimensions}")
    
    system_prompt = DIFY_PROMPT_V4_1.format(
        patient_case=req.patient_case,
        learner_diagnosis=req.learner_diagnosis,
        interaction_history=json.dumps(history_items, ensure_ascii=False, indent=2),
        dimensions=json.dumps(available_dimensions, ensure_ascii=False, indent=2),
    )

    messages = [
        {"role": "system", "content": system_prompt}
    ]

    # =====================
    # Streaming
    # =====================
    async def generate():
        if len(available_dimensions) == 0:
            final_data = {
                "type": "done",
                "dimension": "",
                "question": "",
                "stop": True,
            }

            yield f"data: {json.dumps(final_data, ensure_ascii=False)}\n\n"

            return
        
        full_response = ""
        start = time.time()

        try:
            stream = client.chat.completions.create(
                model="meta-llama/Llama-3.1-8B-Instruct:novita",
                messages=messages,
                stream=True,
                temperature=0.1,
                top_p=0.8,
                max_tokens=300,
                response_format={"type": "json_object"},
            )

            # stream token cho UI
            for chunk in stream:
                if chunk.choices and chunk.choices[0].delta:
                    token = chunk.choices[0].delta.content
                    if token:
                        full_response += token

                        yield f"data: {json.dumps({'type': 'token', 'content': token}, ensure_ascii=False)}\n\n"

            duration = time.time() - start
            logger.info(f"[HF] reasoning duration={duration:.2f}s")

            # =====================
            # PARSE JSON (GIỮ LOGIC)
            # =====================
            result = None
            clean_response = full_response.strip()

            # remove markdown nếu có
            if "```json" in clean_response:
                clean_response = clean_response.split("```json")[1].split("```")[0]
            elif "```" in clean_response:
                clean_response = clean_response.split("```")[1].split("```")[0]

            json_matches = re.findall(
                r"\{(?:[^{}]|(?:\{[^{}]*\}))*\}",
                clean_response,
                re.DOTALL
            )
            result = None

            for candidate in json_matches:
                try:
                    parsed = json.loads(candidate)

                    if (
                            isinstance(parsed, dict)
                            and isinstance(parsed.get("dimension"), str)
                            and isinstance(parsed.get("question"), str)
                            and isinstance(parsed.get("stop"), bool)
                    ):
                        result = parsed
                        break
            
                except Exception:
                    continue
                    
            #logger.info(f"[DEBUG] Parsed reasoning result: {result}")

            # fallback nếu fail
            if not result:
                result = {
                    "dimension": "",
                    "question": full_response.strip(),
                    "stop": False,
                }

            # =====================
            # FINAL RESULT
            # =====================
            final_data = {
                "type": "done",
                "dimension": result.get("dimension", "").strip(),
                "question": result.get("question", "").strip(),
                "stop": result.get("stop", True),
                "full_raw": full_response,  # debug optional
            }

            yield f"data: {json.dumps(final_data, ensure_ascii=False)}\n\n"

        except Exception as e:
            error_data = {"type": "error", "message": str(e)}
            yield f"data: {json.dumps(error_data)}\n\n"

    return StreamingResponse(
        generate(),
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "Connection": "keep-alive",
        },
    )
