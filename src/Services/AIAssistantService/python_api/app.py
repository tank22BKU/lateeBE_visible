# app.py
from typing import List, Optional, Dict, Any
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
from typing import Tuple, List, Optional

try:
    import redis
except Exception:
    redis = None

try:
    from cachetools import TTLCache
except Exception:
    TTLCache = None

load_dotenv()


SYSTEM_PROMPT = """Bạn là trợ lý AI y khoa chuyên về chẩn đoán bệnh lý ổ bụng.

NHIỆM VỤ CHÍNH:
Hỗ trợ bác sĩ bằng cách trả lời các câu hỏi dựa CHÍNH XÁC trên tài liệu được cung cấp.

NGUYÊN TẮC BẮT BUỘC:
1. **KHI CÓ TÀI LIỆU (context)**:
    - PHẢI dựa 100% vào tài liệu
    - KHÔNG được thêm thông tin không có trong tài liệu
    - Trích dẫn CHÍNH XÁC từng bước như trong tài liệu
    - Nếu tài liệu không đủ thông tin → NÓI THẲNG "Tài liệu không đề cập đến vấn đề này"

2. **KHI KHÔNG CÓ TÀI LIỆU**:
    - Trả lời dựa trên kiến thức cơ bản nhất
    - Luôn cảnh báo: "Thông tin này không có trong tài liệu hướng dẫn"

3. **ĐỊNH DẠNG TRẢ LỜI**:
    - Với câu hỏi về quy trình: Liệt kê từng bước theo đúng thứ tự
    - Khi được hỏi bước tiếp theo phải làm gì ? Phải kiểm tra nếu đã khai thác hết tất cả thông tin của bước trước đó trước khi hướng dẫn đến bước tiếp theo. Khi chưa hoàn thành tất cả các yêu cầu của bước trước đó thì không được hướng dẫn bước tiếp theo, rà soát, đảm bảo hỏi đủ thông tin theo quy trình.
    - Sử dụng bullet points và bold cho các tiêu đề
    - Trả lời ngắn gọn, đúng trọng tâm
4. **NGÔN NGỮ TRẢ LỜI**:
    - Nếu câu hỏi là Tiếng Việt thì trả lời bằng Tiếng Việt
    - Nếu câu hỏi là Tiếng Anh thì trả lời bằng Tiếng Anh

CÁCH TRẢ LỜI MẪU NẾU HỎI VỀ QUY TRÌNH CHẨN ĐOÁN:
"Dựa vào tài liệu, quy trình chẩn đoán bệnh lý ổ bụng gồm 6 bước:

• **Bước 1: Đánh giá ban đầu**
    [Nội dung chính xác từ tài liệu; nếu chỉ yêu cầu tên bước thì bỏ phần chi tiết này]

    • **Bước 2: Tiền sử và khám lâm sàng**
    [Nội dung chính xác từ tài liệu; nếu chỉ yêu cầu tên bước thì bỏ phần chi tiết này]
    
    • **Bước 2: Tiền sử và khám lâm sàng**
    [Nội dung chính xác từ tài liệu; nếu chỉ yêu cầu tên bước thì bỏ phần chi tiết này]
    
    • **Bước 3 : Xét nghiệm cận lâm sàng**
    [Nội dung chính xác từ tài liệu; nếu chỉ yêu cầu tên bước thì bỏ phần chi tiết này]
    
    • **Bước 4 : Chẩn đoán hình ảnh**
    [Nội dung chính xác từ tài liệu; nếu chỉ yêu cầu tên bước thì bỏ phần chi tiết này]
    
    • **Bước 5 : Đánh giá kết quả và chẩn đoán phân biệt**
    [Nội dung chính xác từ tài liệu; nếu chỉ yêu cầu tên bước thì bỏ phần chi tiết này]
    
    • **Bước 6 : Xử trí ban đầu và chuyển tiếp**
    [Nội dung chính xác từ tài liệu; nếu chỉ yêu cầu tên bước thì bỏ phần chi tiết này]  
..."

LƯU Ý: TUYỆT ĐỐI KHÔNG sáng tác hoặc thêm bớt thông tin!
"""

HF_TOKEN = os.getenv(
    "HF_TOKEN", ""
) 
REPO_ID = os.getenv("HF_REPO_ID", "meta-llama/Llama-3.1-8B-Instruct")
USE_REDIS = bool(os.getenv("REDIS_URL"))

# History config
HISTORY_TTL_SECONDS = (
    3600  # int(os.getenv("HISTORY_TTL_SECONDS", "3600"))  # 1 hour default
)
HISTORY_MAX_ITEMS = 50  # int(os.getenv("HISTORY_MAX_ITEMS", "50"))

REDIS_HOST = os.getenv("REDIS_HOST", "localhost")
REDIS_PORT = int(os.getenv("REDIS_PORT", 6379))
REDIS_DB = int(os.getenv("REDIS_DB", 0))

app = FastAPI(title="Medical Assistant API")


# --------------------
# Storage abstraction for assistant-history
# --------------------
class HistoryStore:
    def append(self, doctor_id: str, question: str, answer: str) -> None:
        raise NotImplementedError()

    def get(self, doctor_id: str) -> List[Dict[str, str]]:
        raise NotImplementedError()

    def clear(self, doctor_id: str) -> None:
        raise NotImplementedError()


class RedisHistoryStore(HistoryStore):
    def __init__(self, url: str):
        self.client = redis.from_url(url, decode_responses=True)

    def _key(self, doctor_id: str) -> str:
        return f"assistant_history:{doctor_id}"

    def append(self, doctor_id: str, question: str, answer: str):
        key = self._key(doctor_id)
        # store as simple JSON-like string; use timestamp prefix for ordering
        item = {"q": question, "a": answer, "ts": int(time.time())}
        import json

        self.client.rpush(key, json.dumps(item))
        self.client.expire(key, HISTORY_TTL_SECONDS)
        # trim list to max items
        self.client.ltrim(key, -HISTORY_MAX_ITEMS, -1)

    def get(self, doctor_id: str):
        key = self._key(doctor_id)
        arr = self.client.lrange(key, 0, -1)
        import json

        return [json.loads(x) for x in arr] if arr else []

    def clear(self, doctor_id: str):
        self.client.delete(self._key(doctor_id))


class MemoryHistoryStore(HistoryStore):
    def __init__(self):
        if TTLCache is None:
            raise RuntimeError("cachetools is required for in-memory history store")
        # each doctor_id -> list of (ts, item)
        self.cache = TTLCache(maxsize=10000, ttl=HISTORY_TTL_SECONDS)

    def append(self, doctor_id: str, question: str, answer: str):
        lst = self.cache.get(doctor_id, [])
        lst.append({"q": question, "a": answer, "ts": int(time.time())})
        # trim
        if len(lst) > HISTORY_MAX_ITEMS:
            lst = lst[-HISTORY_MAX_ITEMS:]
        self.cache[doctor_id] = lst

    def get(self, doctor_id: str):
        return self.cache.get(doctor_id, [])

    def clear(self, doctor_id: str):
        if doctor_id in self.cache:
            del self.cache[doctor_id]


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
# Request/Response models
# --------------------
class MessageItem(BaseModel):
    role: str  # "doctor" or "patient" or "system"
    content: str


class AssistantRequest(BaseModel):
    doctor_id: str
    question: str
    patient_history: Optional[List[MessageItem]] = []
    use_rag: Optional[bool] = True


class AssistantResponse(BaseModel):
    answer: str
    source_documents: Optional[List[str]] = None


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
        block = "Lịch sử hội thoại giữa bác sĩ và bệnh nhân:\n"
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


@app.post("/assistant_with_history", response_model=AssistantResponse)
def assistant_with_history(req: AssistantRequest):
    """
    Endpoint for asking the assistant WITH the patient-doctor chat history included in request.
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
    messages = build_messages(
        SYSTEM_PROMPT,
        assistant_hist,
        req.patient_history or [],
        req.question,
        req.use_rag,
    )

    source_docs = []
    if RETRIEVER and req.use_rag:
        try:
            docs = RETRIEVER.get_relevant_documents(req.question)
            ctx = "\n\n".join(d.page_content for d in docs[:3])
            messages[-1] = HumanMessage(
                content=f"Dựa vào các tài liệu sau:\n{ctx}\n\nCâu hỏi: {req.question}"
            )
            source_docs = [
                getattr(d, "metadata", {}).get("source", None) or (d.page_content[:200])
                for d in docs[:3]
            ]
        except Exception:
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


@app.post("/assistant/multiplethread", response_model=AssistantResponse)
async def assistant_endpoint_optimized(req: AssistantRequest):
    """
    Optimized endpoint với parallel processing
    KHÔNG thay đổi logic, CHỈ tối ưu thứ tự thực thi
    """

    # === BƯỚC 1: Chạy song song RAG + LLM warm-up ===
    rag_task = retrieve_documents_async(req.question, req.use_rag)
    llm_task = prepare_llm_async()

    # Đợi cả 2 hoàn thành
    (docs, source_docs), llm = await asyncio.gather(rag_task, llm_task)

    # === BƯỚC 2: Build messages (nhanh) ===
    assistant_hist = HISTORY_STORE.get(req.doctor_id) or []
    messages = build_messages(
        SYSTEM_PROMPT,
        assistant_hist,
        req.patient_history or [],
        req.question,
        req.use_rag,
    )

    # === BƯỚC 3: Augment với RAG context ===
    if docs:
        ctx = "\n\n".join(d.page_content for d in docs)
        # Replace last message với augmented version
        messages[-1] = HumanMessage(
            content=f"Dựa vào các tài liệu sau:\n{ctx}\n\nCâu hỏi: {req.question}"
        )

    # === BƯỚC 4: Generate (blocking operation) ===
    try:
        loop = asyncio.get_event_loop()
        resp = await loop.run_in_executor(executor, lambda: llm.invoke(messages))
        answer = resp.content.strip()
        HISTORY_STORE.append(req.doctor_id, req.question, answer)
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"LLM generation failed: {e}")

    return AssistantResponse(answer=answer, source_documents=source_docs or None)
