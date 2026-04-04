from fastapi.responses import StreamingResponse
import json
from typing import AsyncGenerator
import time
import logging
import asyncio
from fastapi import HTTPException
from langchain_core.messages import HumanMessage, SystemMessage, AIMessage
from dtos import QuestionValidationRequest, QuestionValidationResponse
from config import VALIDATION_PROMPT


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

            print(f"[DEBUG] Raw LLM response: {full_response[:500]}")  # Debug log

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
                print(f"[DEBUG] JSON parse failed, trying manual parse")

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
                print(f"[DEBUG] Manual parse result: {result}")

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
            print(f"[ERROR] Validation error: {str(e)}")
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
