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

from config import VALIDATION_PROMPT, VALIDATION_PROMPT_VER2, logger
from dtos import QuestionValidationRequest, QuestionValidationResponse


async def validate_question(req: QuestionValidationRequest):
    """
    Endpoint để kiểm tra tính hợp lệ của câu hỏi learner dành cho bệnh nhân.
    Trả về flag trước, sau đó stream explain.
    """
    from app import get_llm, RETRIEVER

    try:
        llm = get_llm()
    except Exception as e:
        raise HTTPException(status_code=503, detail=f"LLM not available: {e}")

    # Xây dựng context từ conversation history
    context_text = ""
    if req.conversation_context:
        context_text = "\n\nLịch sử hội thoại (để hiểu ngữ cảnh):\n"
        for msg in req.conversation_context[-10:]:
            context_text += f"- {msg.role}: {msg.content}\n"

    # Lấy tài liệu quy trình từ RAG
    process_docs = ""
    if RETRIEVER:
        try:
            docs = RETRIEVER.invoke("quy trình chẩn đoán bệnh lý ổ bụng 6 bước")
            process_docs = "\n\n".join(d.page_content for d in docs[:2])
        except Exception:
            pass

    # Prompt được cải thiện để bắt buộc JSON format
    evaluation_prompt = f"""Tài liệu quy trình chẩn đoán:
{process_docs}
{context_text}

Câu hỏi của học viên cần đánh giá: "{req.learner_question}"

BẮT BUỘC: Chỉ trả về JSON thuần túy, không có text giải thích thêm, không có markdown, không có preamble.

Format JSON bắt buộc:
{{"isValid": true, "reason": "...", "suggestion": "..."}}
hoặc
{{"isValid": false, "reason": "...", "suggestion": "..."}}"""

    messages = [
        SystemMessage(content=VALIDATION_PROMPT),
        HumanMessage(content=evaluation_prompt),
    ]

    async def generate_validation() -> AsyncGenerator[str, None]:
        """Stream response với flag trước, explain sau"""
        try:
            # Thu thập toàn bộ response
            full_response = ""
            for chunk in llm.stream(messages):
                if hasattr(chunk, "content") and chunk.content:
                    full_response += chunk.content

            logger.info(f"[DEBUG] Raw LLM response: {full_response[:500]}")  # Debug log

            # Parse JSON với xử lý lỗi tốt hơn
            result = None
            clean_response = full_response.strip()

            # Thử nhiều cách clean JSON
            # 1. Loại bỏ markdown code blocks
            if "```json" in clean_response:
                clean_response = clean_response.split("```json")[1].split("```")[0]
            elif "```" in clean_response:
                clean_response = clean_response.split("```")[1].split("```")[0]

            # 2. Tìm JSON object đầu tiên trong response
            import re

            json_match = re.search(r'\{[^{}]*"isValid"[^{}]*\}', clean_response)
            if json_match:
                clean_response = json_match.group(0)

            try:
                result = json.loads(clean_response.strip())
            except json.JSONDecodeError:
                # Fallback: Phân tích thủ công nếu JSON lỗi
                logger.exception(f"[DEBUG] JSON parse failed, trying manual parse")

                # Kiểm tra keywords để quyết định isValid
                lower_response = full_response.lower()
                is_valid = True
                reason = "Câu hỏi phù hợp với quy trình chẩn đoán"
                suggestion = ""

                # Các từ khóa cho invalid
                invalid_keywords = [
                    "không hợp lệ",
                    "vi phạm",
                    "sai",
                    "không phù hợp",
                    "không nên",
                    "tránh",
                    "không đúng",
                    'isvalid": false',
                    'isvalid":false',
                    '"isvalid": false',
                ]

                if any(kw in lower_response for kw in invalid_keywords):
                    is_valid = False
                    # Trích xuất reason từ response
                    if "reason" in lower_response:
                        try:
                            reason_part = full_response.split("reason")[1].split(",")[0]
                            reason = reason_part.strip(" :\"'{}\n")[:200]
                        except:
                            reason = "Câu hỏi cần điều chỉnh để phù hợp với quy trình chẩn đoán"

                    # Trích xuất suggestion
                    if "suggestion" in lower_response:
                        try:
                            sugg_part = full_response.split("suggestion")[1]
                            suggestion = sugg_part.strip(" :\"'{}\n")[:500]
                        except:
                            suggestion = "Hãy đặt câu hỏi theo quy trình 6 bước chẩn đoán bệnh lý ổ bụng"

                result = {
                    "isValid": is_valid,
                    "reason": reason,
                    "suggestion": suggestion,
                }
                logger.info(f"[DEBUG] Manual parse result: {result}")

            # Đảm bảo result có đủ keys
            if not result or "isValid" not in result:
                result = {
                    "isValid": True,  # Default cho safe
                    "reason": "Câu hỏi có thể chấp nhận được",
                    "suggestion": "",
                }

            # Gửi flag trước
            flag_data = {
                "isValid": result.get("isValid", True),
                "reason": result.get("reason", ""),
                "suggestion": result.get("suggestion", ""),
            }
            yield f"data: {json.dumps(flag_data, ensure_ascii=False)}\n\n"

            # Stream explanation nếu không hợp lệ
            if not result.get("isValid", True):
                suggestion = result.get("suggestion", "")
                if suggestion:
                    # Stream từng từ
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

            # Gửi signal kết thúc
            done_data = {"type": "done"}
            yield f"data: {json.dumps(done_data)}\n\n"

        except Exception as e:
            logger.exception(f"[ERROR] Validation error: {str(e)}")
            # Trả về flag mặc định an toàn
            safe_flag = {
                "type": "flag",
                "isValid": True,
                "reason": "Không thể đánh giá chính xác, vui lòng kiểm tra thủ công",
            }
            yield f"data: {json.dumps(safe_flag, ensure_ascii=False)}\n\n"

            done_data = {"type": "done"}
            yield f"data: {json.dumps(done_data)}\n\n"

    return StreamingResponse(
        generate_validation(),
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "Connection": "keep-alive",
        },
    )


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

    evaluation_prompt = f"""
TÀI LIỆU QUY TRÌNH:
{process_docs}

LỊCH SỬ HỘI THOẠI:
{context_text}

CÂU HỎI CẦN ĐÁNH GIÁ:
"{req.learner_question}"

BẮT BUỘC: Hãy đánh giá câu hỏi theo đúng hướng dẫn hệ thống.
"""

    messages = [
        {"role": "system", "content": VALIDATION_PROMPT},
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
                "reason": "Không thể đánh giá chính xác",
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
            pass

    evaluation_prompt = f"""
TÀI LIỆU QUY TRÌNH:
{process_docs}

LỊCH SỬ HỘI THOẠI:
{context_text}

CÂU HỎI CẦN ĐÁNH GIÁ:
"{req.learner_question}"

BẮT BUỘC: Hãy đánh giá câu hỏi theo đúng hướng dẫn hệ thống.
"""

    messages = [
        {"role": "system", "content": VALIDATION_PROMPT_VER2},
        {"role": "user", "content": evaluation_prompt},
    ]

    try:
        response = client.chat.completions.create(
            model="meta-llama/Llama-3.1-8B-Instruct:novita",
            messages=messages,
            stream=False,
        )

        raw_output = (response.choices[0].message.content or "").strip()

        logger.info(f"[DEBUG][HF] Raw response: {raw_output[:500]}")

        cleaned = raw_output

        if "```json" in cleaned:

            cleaned = (
                cleaned
                .split("```json")[1]
                .split("```")[0]
            )

        elif "```" in cleaned:

            cleaned = (
                cleaned
                .split("```")[1]
                .split("```")[0]
            )

        cleaned = cleaned.strip()
        # =====================
        # JSON PARSE
        # =====================
        try:
            #cleaned = (cleaned.replace("{{", "{").replace("}}", "}"))
            #parsed_result = json.loads(cleaned)

            json_match = re.search(
                r"\{.*\}",
                cleaned,
                re.DOTALL,
            )

            if not json_match:
                raise ValueError("No JSON found")
            
            json_text = json_match.group(0)
            
            json_text = (
                json_text
                .replace("{{", "{")
                .replace("}}", "}")
            )
            
            parsed_result = json.loads(json_text)

        except Exception:
            logger.exception("[WARN][VALIDATION]"
                " JSON parse failed")

            parsed_result = {
                "isValid": False,

                "reason": (
                    "Model trả về sai định dạng"
                ),

                "suggestion": (
                    "Hãy kiểm tra lại "
                    "quy trình chẩn đoán"
                ),

                "severity": "high",

                "category": "system_failure",

                "confidence": 0.0,
            }

        # ==========================================
        # NORMALIZE RESPONSE
        # ==========================================

        severity = str(
            parsed_result.get(
                "severity",
                "medium",
            )
        )

        if severity not in {
            "low",
            "medium",
            "high",
        }:
            severity = "medium"

        confidence = float(
            parsed_result.get(
                "confidence",
                0.5,
            )
        )

        confidence = max(
            0.0,
            min(1.0, confidence),
        )

        return QuestionValidationResponse(

            isValid=bool(
                parsed_result.get(
                    "isValid",
                    False,
                )
            ),

            reason=str(
                parsed_result.get(
                    "reason",
                    "No reason provided",
                )
            )[:300],

            suggestion=str(
                parsed_result.get(
                    "suggestion",
                    "",
                )
            )[:500],

            severity=severity,

            category=str(
                parsed_result.get(
                    "category",
                    "unknown",
                )
            ),

            confidence=confidence,
        )

    except Exception as e:

        logger.exception(
            f"[VALIDATION] Unexpected error: {str(e)}"
        )

        return QuestionValidationResponse(

            isValid=False,

            reason=(
                "Không thể xác thực câu hỏi "
                "một cách an toàn"
            ),

            suggestion=(
                "Hãy kiểm tra lại câu hỏi "
                "theo quy trình chẩn đoán"
            ),

            severity="high",

            category="system_failure",

            confidence=0.0,
        )