# app.py
from typing import Tuple, List, Optional, Dict, Any
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import os
import time

from contextlib import asynccontextmanager

from langchain_core.messages import HumanMessage, SystemMessage, AIMessage
from langchain_ollama import ChatOllama
from ragLoader import RAGLoader
from dotenv import load_dotenv

from fastapi.responses import StreamingResponse
import json
from typing import AsyncGenerator

import asyncio
from concurrent.futures import ThreadPoolExecutor

from dtos import (
    AssistantRequest,
    AssistantResponse,
    MessageItem,
    QuestionValidationRequest,
    QuestionValidationResponse,
    ValidationFlag,
    HISTORY_MAX_ITEMS,
    HISTORY_TTL_SECONDS,
    RedisHistoryStore,
    MemoryHistoryStore,
    redis,
    TTLCache,
)

load_dotenv()

HF_TOKEN = os.getenv("HF_TOKEN", "")
REPO_ID = os.getenv("HF_REPO_ID", "meta-llama/Llama-3.1-8B-Instruct")
USE_REDIS = bool(os.getenv("REDIS_URL"))


REDIS_HOST = os.getenv("REDIS_HOST", "localhost")
REDIS_PORT = int(os.getenv("REDIS_PORT", 6379))
REDIS_DB = int(os.getenv("REDIS_DB", 0))

app = FastAPI(title="Medical Assistant API")


REDIS_URL = os.getenv("REDIS_URL")

if REDIS_URL and redis is not None:
    HISTORY_STORE = RedisHistoryStore(REDIS_URL)
else:
    if TTLCache is None:
        raise RuntimeError(
            "Install cachetools or configure REDIS_URL for history storage"
        )
    HISTORY_STORE = MemoryHistoryStore()


def init_llm():
    return ChatOllama(
        model="llama3.1:8b",
        base_url="http://ollama:11434",
        temperature=0.1,
        num_predict=1024,
        top_p=0.85,
        repeat_penalty=1.15,
    )


IS_RERUNNING = False

# Lazy init so app can start in dev without HF_TOKEN
LLM = None


def get_llm():
    global LLM, IS_RERUNNING
    if LLM is None:
        LLM = init_llm()
        IS_RERUNNING = True
    return LLM


# --------------------
# Optional retriever placeholder
# --------------------
# If you want RAG, instantiate a retriever (FAISS/Chroma/etc.) and set RETRIEVER var.
RETRIEVER = RAGLoader().get_retriever()
# Example: RETRIEVER = my_vectorstore.as_retriever(search_kwargs={"k":4})


# --------------------
# Utility: build messages for LLM
# --------------------
def build_messages(
    system_prompt: str,
    assistant_hist: List[Dict[str, Any]],
    patient_history: List[MessageItem],
    question: str,
    use_rag: bool = True,
):
    messages = [SystemMessage(content=system_prompt)]
    print(
        "\n#################################################Assistant history:",
        assistant_hist,
    )
    # reconstruct assistant history as interleaved HumanMessage (doctor question) and AIMessage (assistant answer)
    for item in assistant_hist:
        q = item.get("q")
        a = item.get("a")
        if q:
            messages.append(HumanMessage(content=q))
        if a:
            messages.append(AIMessage(content=a))

    # add patient_history as a single context block to avoid role confusion
    if patient_history:
        block = "\nLịch sử hội thoại giữa bác sĩ và bệnh nhân:\n"
        for m in patient_history:
            role = m.role
            block += f"- {role}: {m.content}\n"
        messages.append(HumanMessage(content=block))

    # optionally add retrieved docs context (handled separately)
    # finally current question
    messages.append(HumanMessage(content=question))
    print(
        f'"-----------------------Built messages : {messages}-----------------------\n"'
    )
    return messages


@asynccontextmanager
async def lifespan(app: FastAPI):
    global LLM_POOL
    llm = init_llm()
    # simple warm-up
    try:
        llm.invoke([HumanMessage(content="Test")])
    except:
        pass
    print("✅LLM pool ready")


from config import SYSTEM_PROMPT, VALIDATION_PROMPT


# --------------------
# Main endpoints
# --------------------
@app.post("/assistant", response_model=AssistantResponse)
def assistant_endpoint(req: AssistantRequest):
    """
    Endpoint for asking the assistant WITHOUT sending the patient-doctor chat history.
    The server will still include prior assistant interactions (if stored) for context.
    """
    global IS_RERUNNING

    try:
        llm = get_llm()
        if IS_RERUNNING:
            IS_RERUNNING = False
            HISTORY_STORE.clear(
                req.doctor_id
            )  # clear history on first run to avoid mixing contexts
    except Exception as e:
        raise HTTPException(status_code=503, detail=f"LLM not available: {e}")

    assistant_hist = HISTORY_STORE.get(req.doctor_id) or []
    # build messages
    messages = build_messages(
        SYSTEM_PROMPT, assistant_hist, [], req.question, req.use_rag
    )

    # Optional RAG: if retriever provided and use_rag true, fetch docs and prepend to question.
    source_docs = []
    if RETRIEVER and req.use_rag:
        try:
            docs = RETRIEVER.invoke(
                req.question
            )  # RETRIEVER.get_relevant_documents(req.question)  # or .invoke depending on retriever
            # attach small context
            ctx = "\n\n".join(d.page_content for d in docs[:3])
            # replace last HumanMessage (question) with augmented question
            messages.append(
                HumanMessage(
                    content=f"Dựa vào các tài liệu sau:\n{ctx}\n\nCâu hỏi: {req.question}"
                )
            )
            source_docs = [
                getattr(d, "metadata", {}).get("source", None) or (d.page_content[:200])
                for d in docs[:3]
            ]
        except Exception:
            # ignore retrieval errors but warn in logs if needed
            pass

    try:
        resp = llm.invoke(messages)
        answer = resp.content.strip()
        HISTORY_STORE.append(req.doctor_id, req.question, answer)
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"LLM generation failed: {e}")

    return AssistantResponse(answer=answer, source_documents=source_docs or None)


@app.post("/assistant/stream")
async def assistant_stream(req: AssistantRequest):

    global IS_RERUNNING

    try:
        llm = get_llm()
        if IS_RERUNNING:
            IS_RERUNNING = False
            HISTORY_STORE.clear(
                req.doctor_id
            )  # clear history on first run to avoid mixing contexts
    except Exception as e:
        raise HTTPException(status_code=503, detail=f"LLM not available: {e}")

    # assistant_hist = HISTORY_STORE.get(req.doctor_id) or []
    messages = build_messages(
        SYSTEM_PROMPT,
        [],
        req.patient_history or [],
        req.question,
        req.use_rag,
    )

    source_docs = []
    if RETRIEVER and req.use_rag:
        try:
            docs = RETRIEVER.invoke(req.question)
            ctx = "\n\n".join(d.page_content for d in docs[:3])
            messages.append(
                HumanMessage(
                    content=f"Dựa vào các tài liệu sau:\n{ctx}\n\nCâu hỏi: {req.question}"
                )
            )
            source_docs = [
                getattr(d, "metadata", {}).get("source", None) or (d.page_content[:200])
                for d in docs[:3]
            ]
        except Exception:
            pass

    async def generate() -> AsyncGenerator[str, None]:
        """Stream tokens as they're generated"""
        full_answer = ""

        try:
            # Stream từng chunk
            for chunk in llm.stream(messages):
                if hasattr(chunk, "content") and chunk.content:
                    full_answer += chunk.content

                    # Send chunk to client
                    data = {"type": "token", "content": chunk.content}
                    yield f"data: {json.dumps(data, ensure_ascii=False)}\n\n"

            # Lưu vào history SAU KHI hoàn thành
            # HISTORY_STORE.append(req.doctor_id, req.question, full_answer)

            # Send metadata cuối cùng
            final_data = {
                "type": "done",
                "source_documents": source_docs if source_docs else None,
                "full_answer": full_answer,
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


executor = ThreadPoolExecutor(max_workers=4)


async def retrieve_documents_async(
    question: str, use_rag: bool
) -> Tuple[List, List[str]]:
    """Async wrapper cho RAG retrieval"""
    if not RETRIEVER or not use_rag:
        return [], []

    def _retrieve():
        try:
            docs = RETRIEVER.invoke(question)
            source_docs = [
                getattr(d, "metadata", {}).get("source", None) or (d.page_content[:200])
                for d in docs[:3]
            ]
            return docs[:3], source_docs
        except Exception as e:
            print(f"RAG error: {e}")
            return [], []

    # Chạy trong thread pool để không block event loop
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(executor, _retrieve)


async def prepare_llm_async():
    """Warm-up LLM connection"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(executor, get_llm)



@app.post("/assistant/validate_question")
async def validate_question_endpoint(req: QuestionValidationRequest):
    """
    Endpoint để kiểm tra tính hợp lệ của câu hỏi learner dành cho bệnh nhân.
    Trả về flag trước, sau đó stream explain.
    """
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
                        explain_data = {"isValid": result.get("isValid"), "reason": result.get("reason") , "suggestion": chunk_text}
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
