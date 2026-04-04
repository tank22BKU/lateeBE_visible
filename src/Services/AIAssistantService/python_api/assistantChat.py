from typing import AsyncGenerator, Tuple, List, Optional, Dict, Any
from fastapi import HTTPException
from langchain_core.messages import HumanMessage, SystemMessage, AIMessage
from dtos import AssistantRequest, AssistantResponse, MessageItem
import app as app_module
from config import SYSTEM_PROMPT, logger
import time, json
from fastapi.responses import StreamingResponse


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
    # print(
    #     "\n#################################################Assistant history:",
    #     assistant_hist,
    # )
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
    # print(
    #     f'"-----------------------Built messages : {messages}-----------------------\n"'
    # )
    return messages


async def assistant_endpoint(req: AssistantRequest):
    """
    Endpoint for asking the assistant WITHOUT sending the patient-doctor chat history.
    The server will still include prior assistant interactions (if stored) for context.
    """
    try:
        llm = app_module.get_llm()
        if app_module.IS_RERUNNING:
            app_module.IS_RERUNNING = False
            app_module.HISTORY_STORE.clear(
                req.doctor_id
            )  # clear history on first run to avoid mixing contexts
    except Exception as e:
        raise HTTPException(status_code=503, detail=f"LLM not available: {e}")

    assistant_hist = app_module.HISTORY_STORE.get(req.doctor_id) or []
    # build messages
    messages = build_messages(
        SYSTEM_PROMPT, assistant_hist, [], req.question, req.use_rag
    )

    # Optional RAG: if retriever provided and use_rag true, fetch docs and prepend to question.
    source_docs = []
    if app_module.RETRIEVER and req.use_rag:
        try:
            docs = app_module.RETRIEVER.invoke(
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
        # app_module.HISTORY_STORE.append(req.doctor_id, req.question, answer)
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"LLM generation failed: {e}")

    return AssistantResponse(answer=answer, source_documents=source_docs or None)


async def assistant_stream(req: AssistantRequest):

    try:
        start = time.time()

        logger.info(f"[START] question={req.question}")

        llm = app_module.get_llm()
        if app_module.IS_RERUNNING:
            app_module.IS_RERUNNING = False
            app_module.HISTORY_STORE.clear(
                req.doctor_id
            )  # clear history on first run to avoid mixing contexts
    except Exception as e:
        raise HTTPException(status_code=503, detail=f"LLM not available: {e}")

    assistant_hist = app_module.HISTORY_STORE.get(req.doctor_id) or []
    messages = build_messages(
        SYSTEM_PROMPT,
        assistant_hist,
        req.patient_history or [],
        req.question,
        req.use_rag,
    )

    source_docs = []
    if app_module.RETRIEVER and req.use_rag:
        try:
            docs = app_module.RETRIEVER.invoke(req.question)
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

            duration = time.time() - start
            logger.info(f"[END] duration={duration:.2f}s")

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
