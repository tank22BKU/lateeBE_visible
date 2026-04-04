from langchain_core.messages import SystemMessage
from dtos import ClinicalReasoningRequest, ClinicalReasoningResponse
from fastapi import HTTPException
import asyncio
import json

from config import (
    CLINICAL_REASONING_PROMPT,
    DIFY_PROMPT,
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
            print(f"[DEBUG] Parsed reasoning result: {result}")
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
