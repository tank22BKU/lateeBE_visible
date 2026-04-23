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
    DIFY_PROMPT_VER2
)

ALL_DIMENSIONS = [
    "Cơ sở bằng chứng",
    "Chẩn đoán phân biệt",
    "Dữ kiện mâu thuẫn",
    "Giải thích cơ chế bệnh sinh",
    "Thông tin còn thiếu",
    "Ưu tiên chẩn đoán nguy hiểm",
    "Độ chắc chắn của quyết định",
    "Hành động lâm sàng tiếp theo",
]

async def clinical_reasoning_endpoint(req: ClinicalReasoningRequest):

    try:
        from app import get_llm, executor

        llm = get_llm()
    except Exception as e:
        raise HTTPException(status_code=503, detail=f"LLM not available: {e}")

    history_text = ""
    if req.interaction_history:
        history_text = "\nLịch sử tương tác:"
        for q in req.interaction_history:
            history_text += f"Khía cạnh câu hỏi: {q.dimension}. Câu hỏi: {q.question} + Câu Trả lời của người học: {q.answer}\n"

    # print(f"Interaction history text for reasoning:\n{history_text}")

    system_prompt = DIFY_PROMPT.format(
        patient_case=req.patient_case,
        learner_diagnosis=req.learner_diagnosis,
        interaction_history=history_text,
    )
    messages = [SystemMessage(content=system_prompt)]

    try:
        loop = asyncio.get_event_loop()
        resp = await asyncio.wait_for(
            loop.run_in_executor(executor, lambda: llm.invoke(messages)),
            timeout=90.0,
        )
        content = resp.content.strip()

        import re

        json_match = re.search(r"\{.*\}", content, re.DOTALL)

        if json_match:
            result = json.loads(json_match.group())
            logger.info(f"[DEBUG] Parsed reasoning result: {result}")
        else:
            result = {"dimension": "", "question": content, "stop": False}

    except asyncio.TimeoutError:
        raise HTTPException(status_code=504, detail="Reasoning generation timed out")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Reasoning generation failed: {e}")

    return ClinicalReasoningResponse(
        dimension=result.get("dimension", ""),
        question=result.get("question", ""),
        stop=result.get("stop", False),
    )

async def clinical_reasoning_stream_hf(req: ClinicalReasoningRequest):

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
    history_text = ""
    USED_DIMENSIONS = []
    if req.interaction_history:
        history_text = ""
        for q in req.interaction_history:
            USED_DIMENSIONS.append(q.dimension)
            history_text += (
                f"Khía cạnh câu hỏi: {q.dimension}. " +
                f"Câu hỏi: {q.question}" +
                f"Câu Trả lời của người học: {q.answer}\n"
            )

    # system_prompt = DIFY_PROMPT.format(
    #     patient_case=req.patient_case,
    #     learner_diagnosis=req.learner_diagnosis,
    #     interaction_history=history_text,
    # )
    logger.info(f"Used dimensions: {USED_DIMENSIONS}")
    available_dimensions = [] + [d for d in ALL_DIMENSIONS if d not in USED_DIMENSIONS]
    # if len(available_dimensions) == 0:
    #     final_data = {
    #         "type": "done",
    #         "dimension": "",
    #         "question": "",
    #         "stop": True,
    #         "full_raw": "",  # debug optional
    #     }
    # 
    #     return final_data  
    
    logger.info(f"Available dimensions: {available_dimensions}")
    
    system_prompt = DIFY_PROMPT_VER2.format(
        patient_case=req.patient_case,
        learner_diagnosis=req.learner_diagnosis,
        interaction_history=history_text,
        dimensions=available_dimensions
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

            # json_match = re.search(r"\{.*\}", clean_response, re.DOTALL)
            # 
            # if json_match:
            #     try:
            #         result = json.loads(json_match.group())
            #         logger.info(f"[DEBUG] Parsed reasoning result: {result}")
            #     except:
            #         result = None
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
