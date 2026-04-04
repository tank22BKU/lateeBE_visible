# app.py
from typing import Tuple, List, Optional, Dict, Any
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
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
    ClinicalReasoningRequest,
    ClinicalReasoningResponse,
)

from config import (
    SYSTEM_PROMPT,
    VALIDATION_PROMPT,
    CLINICAL_REASONING_PROMPT,
    DIFY_PROMPT,
    logger,
)


load_dotenv()

HF_TOKEN = os.getenv("HF_TOKEN", "")
REPO_ID = os.getenv("HF_REPO_ID", "meta-llama/Llama-3.1-8B-Instruct")
USE_REDIS = bool(os.getenv("REDIS_URL"))


REDIS_HOST = os.getenv("REDIS_HOST", "localhost")
REDIS_PORT = int(os.getenv("REDIS_PORT", 6379))
REDIS_DB = int(os.getenv("REDIS_DB", 0))

# --------------------
# Optional retriever placeholder
# --------------------
# If you want RAG, instantiate a retriever (FAISS/Chroma/etc.) and set RETRIEVER var.
RETRIEVER = RAGLoader().get_retriever()


@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("[STARTING WARM-UP] Starting app... warming up LLM")
    llm = init_llm()
    start = time.time()

    try:
        llm.invoke([HumanMessage(content="Hello")])
        logger.info(f"[DONE WARM-UP] LLM warm-up done in {time.time() - start:.2f}s")
    except Exception as e:
        logger.error(f"[ERROR] Warm-up failed: {e}")

    yield
    logger.info("[SHUT DOWN] App shutdown")


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


app = FastAPI(title="Medical Assistant API", lifespan=lifespan)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

export_app = app  # for testing purposes

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


@app.get("/health")
def health_check():
    return {"status": "ok"}


# --------------------
# Main endpoints
# --------------------
# Lazy imports moved to end to avoid circular import issues
# These modules import from app.py, so we import them after all app-level
# variables (RETRIEVER, executor, export_app) are fully initialized


@app.post("/assistant/stream")
async def assistant_chat(req: AssistantRequest):
    from assistantChat import assistant_stream
    return await assistant_stream(req)


"""
API endpoints for question validation and clinical reasoning, implemented in separate modules for clarity.
"""


@app.post("/assistant/validate_question")
async def validate_question_endpoint(req: QuestionValidationRequest):
    from validateQuestion import validate_question
    return await validate_question(req)


"""
Clinical reasoning endpoint, also implemented in separate module.
"""


@app.post("/clinicalreasoning", response_model=ClinicalReasoningResponse)
async def clinical_reasoning_chat(req: ClinicalReasoningRequest):
    from reasoning import clinical_reasoning_endpoint
    return await clinical_reasoning_endpoint(req)
