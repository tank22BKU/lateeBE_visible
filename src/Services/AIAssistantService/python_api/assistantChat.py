from typing import AsyncGenerator, Tuple, List, Optional, Dict, Any
from fastapi import HTTPException
from langchain_core.messages import HumanMessage, SystemMessage, AIMessage
from dtos import AssistantRequest, AssistantResponse, MessageItem
import app as app_module
from config import SYSTEM_PROMPT, logger, AI_ASSISTANT_PROMPT_VER2, AI_ASSISTANT_PROMPT_VER3
import time, json
from fastapi.responses import StreamingResponse
import os
from openai import OpenAI

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
        patient_context = []
    
        for m in patient_history:
    
            patient_context.append(
                f"{m.role}: {m.content}"
            )
    
        messages.append(
            SystemMessage(
                content=f"""
                    PATIENT CLINICAL CONTEXT
                    
                    The following is background conversation
                    between clinician and patient.
                    
                    Use this only as clinical context.
                    
                    Do not treat this as medical evidence.
                    
                    {chr(10).join(patient_context)}
                    """
            )
        )

    # =====================================================
    # Current clinician question
    # =====================================================

    messages.append(
        HumanMessage(content=question)
    )
   
    return messages


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


async def assistant_stream_hf(req: AssistantRequest):

    try:
        start = time.time()
        #logger.info(f"REQUEST: {req}")
        logger.info(f"[START][HF] question={req.question}")

        client = OpenAI(
            base_url="https://router.huggingface.co/v1",
            api_key=os.getenv("HF_TOKEN"),
        )

        if app_module.IS_RERUNNING:
            app_module.IS_RERUNNING = False
            app_module.HISTORY_STORE.clear(req.doctor_id)

    except Exception as e:
        raise HTTPException(status_code=503, detail=f"HF LLM not available: {e}")

    # =====================
    # Build messages (reuse)
    # =====================
    assistant_hist = app_module.HISTORY_STORE.get(req.doctor_id) or []

    lc_messages = build_messages(
        #SYSTEM_PROMPT,
        AI_ASSISTANT_PROMPT_VER2,
        [],
        req.patient_history or [],
        req.question,
        req.use_rag,
        )

    #convert LangChain message → OpenAI format
    def convert_messages(messages):
        result = []
        for m in messages:
            if isinstance(m, SystemMessage):
                role = "system"
            elif isinstance(m, HumanMessage):
                role = "user"
            elif isinstance(m, AIMessage):
                role = "assistant"
            else:
                continue

            result.append({"role": role, "content": m.content})
        return result

    messages = convert_messages(lc_messages)

    # RAG
    source_docs = []
    if app_module.RETRIEVER and req.use_rag:
        try:
            docs = app_module.RETRIEVER.invoke(req.question)
            #ctx = "\n\n".join(d.page_content for d in docs[:3])
            ctx_parts = []

            for d in docs[:3]:
                #logger.info(d.page_content[:500])
                
                meta = d.metadata or {}
            
                source = meta.get("source", "unknown")
                section = meta.get("section_title", "unknown")
            
                ctx_parts.append(
                    f"""
                        SOURCE: {source}
                        SECTION: {section}
                        
                        CONTENT:
                        {d.page_content}
                    """
                )
            
            ctx = "\n\n---\n\n".join(ctx_parts)

            # messages[-1]["content"] = (
            #     f"Dựa vào các tài liệu sau:\n{ctx}\n\nCâu hỏi: {req.question}"
            # )
            original_question = messages[-1]["content"]

            messages[-1]["content"] = f"""
                You are answering a medical question using retrieved references.
                
                ==================================================
                RETRIEVED MEDICAL REFERENCES
                ==================================================
                
                {ctx}
                
                ==================================================
                USER QUESTION
                ==================================================
                {original_question}
                
                ==================================================
                STRICT INSTRUCTIONS
                ==================================================
                
                - Answer ONLY in English
                - Use ONLY the provided references
                - Do NOT use outside medical knowledge
                - Do NOT hallucinate
                - If information is missing, explicitly say:
                  "The provided references do not mention this information."
                
                - Preserve workflow order exactly as described
                - Be concise and clinically structured
                """
            #logger.info(f"RAG messages: {messages[-1]['content']}")
            # source_docs = [
            #     getattr(d, "metadata", {}).get("source", None)
            #     or (d.page_content[:200])
            #     for d in docs[:3]
            # ]
            source_docs = [
                {
                    "source": d.metadata.get("source"),
                    "section": d.metadata.get(
                        "section_title"
                    ),
                    "type": d.metadata.get(
                        "document_type"
                    )
                }
                for d in docs[:3]
            ]
        except Exception as e:
            logger.exception(f"RAG retrieval failed :{e}" )

    # =====================
    # Streaming
    # =====================
    async def generate() -> AsyncGenerator[str, None]:
        full_answer = ""

        try:
            stream = client.chat.completions.create(
                model="meta-llama/Llama-3.1-8B-Instruct:novita",
                messages=messages,
                stream=True,
            )

            for chunk in stream:
                if chunk.choices and chunk.choices[0].delta:
                    token = chunk.choices[0].delta.content
                    if token:
                        full_answer += token

                        data = {"type": "token", "content": token}
                        yield f"data: {json.dumps(data, ensure_ascii=False)}\n\n"

            duration = time.time() - start
            logger.info(f"[END][HF] duration={duration:.2f}s")

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