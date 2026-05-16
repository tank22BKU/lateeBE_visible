import asyncio
import json
import logging
import os
import re
import time
from fastapi import HTTPException
from fastapi.responses import StreamingResponse
from langchain_core.messages import HumanMessage, SystemMessage, AIMessage
from openai import OpenAI
from typing import AsyncGenerator

from config import VALIDATION_PROMPT, VALIDATION_PROMPT_VER2, VALIDATION_PROMPT_V3, VALIDATION_PROMPT_V4, VALIDATION_PROMPT_V5, logger
from config2 import EVALUATION_VALIDATION_PROMPT
from dtos import QuestionValidationRequest, QuestionValidationResponse

async def validate_question_stream_hf(req: QuestionValidationRequest):
    from app import RETRIEVER

    try:
        client = OpenAI(
            base_url="https://router.huggingface.co/v1",
            api_key=os.getenv("HF_TOKEN"),
        )
    except Exception as e:
        raise HTTPException(status_code=503, detail=f"HF LLM not available: {e}")

    context_text = ""
    if req.conversation_context:
        context_text = "\n\nLịch sử hội thoại (để hiểu ngữ cảnh):\n"
        for msg in req.conversation_context[-10:]:
            context_text += f"- {msg.role}: {msg.content}\n"

    process_docs = ""
    if RETRIEVER:
        try:
            docs = RETRIEVER.invoke("quy trình chẩn đoán bệnh lý ổ bụng 6 bước")
            process_docs = "\n\n".join(d.page_content for d in docs[:2])
        except Exception:
            logger.exception("Failed to retrieve documents"
                            f"[WARN][VALIDATION]"
                            f" Retriever failed: {retrieval_error}"
                            )

#     evaluation_prompt = f"""
# TÀI LIỆU QUY TRÌNH:
# {process_docs}

# LỊCH SỬ HỘI THOẠI:
# {context_text}

# CÂU HỎI CẦN ĐÁNH GIÁ:
# "{req.learner_question}"

# BẮT BUỘC: Hãy đánh giá câu hỏi theo đúng hướng dẫn hệ thống.
# """
        evaluation_prompt = f"""
        PROCEDURE DOCUMENTATION:
        {process_docs}

        HISTORY OF CONVERSATION:
        {context_text}

        LEARNER QUESTION:
        "{req.learner_question}"

        REQUIRED: Please evaluate the question according to the system guidelines.
        """
    system_prompt = EVALUATION_VALIDATION_PROMPT

    messages = [
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": evaluation_prompt},
    ]

    # =====================
    # Streaming
    # =====================
    async def generate_validation() -> AsyncGenerator[str, None]:
        try:
            full_response = ""

            stream = client.chat.completions.create(
                model="meta-llama/Llama-3.1-8B-Instruct:novita",
                messages=messages,
                stream=True,
            )

            for chunk in stream:
                if chunk.choices and chunk.choices[0].delta:
                    token = chunk.choices[0].delta.content
                    if token:
                        full_response += token

            logger.info(f"[DEBUG][HF] Raw response: {full_response[:500]}")

            # =====================
            # JSON PARSE (GIỮ NGUYÊN LOGIC)
            # =====================
            result = None
            clean_response = full_response.strip()

            if "```json" in clean_response:
                clean_response = clean_response.split("```json")[1].split("```")[0]
            elif "```" in clean_response:
                clean_response = clean_response.split("```")[1].split("```")[0]

            json_match = re.search(r'\{[^{}]*"isValid"[^{}]*\}', clean_response)
            if json_match:
                clean_response = json_match.group(0)

            try:
                result = json.loads(clean_response.strip())
            except json.JSONDecodeError:
                logger.exception("[DEBUG][HF] JSON parse failed → fallback")

                lower_response = full_response.lower()
                is_valid = True
                reason = "Câu hỏi phù hợp với quy trình chẩn đoán"
                suggestion = ""

                invalid_keywords = [
                    "không hợp lệ",
                    "vi phạm",
                    "sai",
                    "không phù hợp",
                    "không nên",
                    "tránh",
                    "không đúng",
                    'isvalid": false',
                ]

                if any(kw in lower_response for kw in invalid_keywords):
                    is_valid = False
                    reason = "Câu hỏi cần điều chỉnh để phù hợp quy trình"
                    suggestion = "Hãy đặt câu hỏi theo quy trình 6 bước chẩn đoán"

                result = {
                    "isValid": is_valid,
                    "reason": reason,
                    "suggestion": suggestion,
                }

            if not result or "isValid" not in result:
                result = {
                    "isValid": True,
                    "reason": "Câu hỏi có thể chấp nhận",
                    "suggestion": "",
                }

            # =====================
            # STREAM FLAG TRƯỚC
            # =====================
            flag_data = {
                "isValid": result.get("isValid", True),
                "reason": result.get("reason", ""),
                "suggestion": result.get("suggestion", ""),
            }

            yield f"data: {json.dumps(flag_data, ensure_ascii=False)}\n\n"

            # =====================
            # STREAM EXPLAIN
            # =====================
            if not result.get("isValid", True):
                suggestion = result.get("suggestion", "")
                if suggestion:
                    words = suggestion.split()
                    for i, word in enumerate(words):
                        chunk_text = word + (" " if i < len(words) - 1 else "")
                        explain_data = {
                            "isValid": result.get("isValid"),
                            "reason": result.get("reason"),
                            "suggestion": chunk_text,
                        }
                        yield f"data: {json.dumps(explain_data, ensure_ascii=False)}\n\n"
                        await asyncio.sleep(0.03)

            yield f"data: {json.dumps({'type': 'done'})}\n\n"

        except Exception as e:
            logger.exception(f"[ERROR][HF] {str(e)}")

            safe_flag = {
                "type": "flag",
                "isValid": True,
                "reason": "Cant evaluate exactly",
            }
            yield f"data: {json.dumps(safe_flag, ensure_ascii=False)}\n\n"
            yield f"data: {json.dumps({'type': 'done'})}\n\n"

    return StreamingResponse(
        generate_validation(),
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "Connection": "keep-alive",
        },
    )

def safe_json_loads(text: str):
    if not text or not text.strip():
        raise ValueError("Empty JSON response")
    
    text = text.strip()

    # remove trailing commas
    text = re.sub(r",\s*}", "}", text)
    text = re.sub(r",\s*]", "]", text)

    return json.loads(text)

async def validate_question_hf(req: QuestionValidationRequest) -> QuestionValidationResponse:
    from app import RETRIEVER

    try:
        client = OpenAI(
            base_url="https://router.huggingface.co/v1",
            api_key=os.getenv("HF_TOKEN"),
        )
    except Exception as e:
        raise HTTPException(status_code=503, detail=f"HF LLM not available: {e}")

    # =====================
    # Build context
    # =====================
    conversation_lines = []

    for msg in (req.conversation_context or [])[-10:]:
        role = str(msg.role).upper()

        conversation_lines.append(f"{role}: {msg.content}")

    context_text = "\n".join(conversation_lines)

    process_docs = ""

    if RETRIEVER:
        try:
            docs = RETRIEVER.invoke("Abdominal Pain Diagnostic Guideline")
            process_docs = "\n\n".join(d.page_content for d in docs[:3])
        except Exception:
            logger.exception(f"[VALIDATION] RAG retrieval failed: {e}")

    evaluation_prompt = f"""
        ==================================================
        DIAGNOSTIC WORKFLOW REFERENCE
        ==================================================
        
        {process_docs}
        
        ==================================================
        CONVERSATION CONTEXT
        ==================================================
        
        {context_text}
        
        ==================================================
        LEARNER QUESTION
        ==================================================
        
        {req.learner_question}
        
        ==================================================
        TASK
        ==================================================
        
        Evaluate whether the learner question is appropriate in the current clinical interaction context.
        
        Return ONLY valid JSON.
    """

    system_prompt = EVALUATION_VALIDATION_PROMPT

    messages = [
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": evaluation_prompt},
    ]

    try:

        response = client.chat.completions.create(
            model="meta-llama/Llama-3.1-8B-Instruct:novita",
            messages=messages,
            temperature=0.1,
            top_p=0.8,
            max_tokens=300,
            stream=False,
            response_format={"type": "json_object"},
        )

        raw_output = (
            response.choices[0]
            .message.content
            .strip()
        )

        logger.info(
            f"[VALIDATION][RAW] "
            f"{raw_output[:500]}"
        )

    except Exception as e:

        logger.exception(
            f"[VALIDATION] LLM request failed: {e}"
        )

        return QuestionValidationResponse(
            isValid=False,
            reason="Validation service failure.",
            suggestion=(
                "Retry the validation request."
            ),
            severity="high",
            category="system_failure",
            confidence=0.0,
        )

    cleaned = raw_output.strip()

    if "```json" in cleaned:
        cleaned = (cleaned.split("```json")[1].split("```")[0])

    elif "```" in cleaned:
        cleaned = (cleaned.split("```")[1].split("```")[0])

    cleaned = cleaned.strip()
    
    # =====================
    # JSON PARSE
    # =====================
    try:
        json_match = re.search(r"\{.*?\}", cleaned, re.DOTALL,)

        if not json_match:
            raise ValueError("No JSON object found")

        json_text = json_match.group(0)
        parsed_result = json.loads(json_text)

    except Exception:
        logger.exception("[VALIDATION] JSON parse failed")

        parsed_result = {
            "isValid": False,
            "reason": (
                "Invalid response format "
                "returned by model."
            ),
            "suggestion": (
                "Retry the validation process."
            ),
            "severity": "high",
            "category": "system_failure",
            "confidence": 0.0,
        }

    # =====================================================
    # NORMALIZATION
    # =====================================================

    severity = str(parsed_result.get("severity", "medium")).lower()

    if severity not in {"low", "medium", "high",}:
        severity = "medium"

    confidence = float(parsed_result.get("confidence", 0.5,))
    confidence = max(0.0, min(1.0, confidence))

    # =====================================================
    # FINAL RESPONSE
    # =====================================================

    return QuestionValidationResponse(
        isValid=bool(parsed_result.get("isValid", False,)),
        reason=str(parsed_result.get("reason", "No reason provided."))[:300],
        suggestion=str(parsed_result.get("suggestion", ""))[:500],
        severity=severity,
        category=str(parsed_result.get("category", "unknown",)),
        confidence=confidence,
    )