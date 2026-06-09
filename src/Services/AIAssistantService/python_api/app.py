# app.py
import asyncio
import json
import os
import time
from concurrent.futures import ThreadPoolExecutor
from contextlib import asynccontextmanager
from dotenv import load_dotenv
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import StreamingResponse
from langchain_core.messages import HumanMessage, SystemMessage, AIMessage
from langchain_ollama import ChatOllama
from pydantic import BaseModel
from typing import AsyncGenerator
from typing import Tuple, List, Optional, Dict, Any

from config import (
    logger,
)
from dtos import (
    AssistantRequest,
    QuestionValidationRequest,
    MemoryHistoryStore,
    TTLCache,
    ClinicalReasoningRequest,
    ClinicalReasoningResponse,
)

# from ragLoader import RAGLoader
from ragLoaderVer2 import RAGLoader

load_dotenv()
# --------------------
# Optional retriever placeholder
# --------------------
# If you want RAG, instantiate a retriever (FAISS/Chroma/etc.) and set RETRIEVER var.
ragLoader = RAGLoader()
RETRIEVER = None


@asynccontextmanager
async def lifespan(app: FastAPI):
    logger.info("[STARTING] App starting...")

    # init RAG
    global RETRIEVER
    RETRIEVER = ragLoader.get_retriever()

    try:
        llm = get_llm()
        llm.invoke("Hello")
        logger.info("Ollama warmup done")
    except Exception as e:
        logger.error(f"Ollama warmup failed: {e}")

    yield
    logger.info("[SHUTDOWN] App shutdown")


app = FastAPI(title="Medical Assistant API", lifespan=lifespan)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

export_app = app  # for testing purposes

HF_TOKEN = os.getenv("HF_TOKEN", "")
REPO_ID = os.getenv("HF_REPO_ID", "meta-llama/Llama-3.1-8B-Instruct")

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


if TTLCache is None:
    raise RuntimeError("Install cachetools for in-memory history storage")

HISTORY_STORE = MemoryHistoryStore()


# Middleware to strip trailing spaces from request paths
@app.middleware("http")
async def normalize_path_middleware(request, call_next):
    """Strip trailing spaces from URL path to prevent 404 errors"""
    request.scope["path"] = request.scope["path"].rstrip()
    return await call_next(request)


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


# @app.post("/assistant/stream")
# async def assistant_chat(req: AssistantRequest):
#     from assistantChat import assistant_stream
#     return await assistant_stream(req)


@app.post("/assistant/stream/hf")
async def assistant_hf_chat(req: AssistantRequest):
    from assistantChat import assistant_stream_hf

    return await assistant_stream_hf(req)


"""
API endpoints for question validation and clinical reasoning, implemented in separate modules for clarity.
# Middleware to strip trailing spaces from request paths
@app.middleware("http")
async def normalize_path_middleware(request, call_next):
    
    request.scope["path"] = request.scope["path"].rstrip()
    return await call_next(request)
"""


# @app.post("/assistant/validate_question")
# async def validate_question_endpoint(req: QuestionValidationRequest):
#     from validateQuestion import validate_question
#     return await validate_question(req)


@app.post("/assistant/validate_question/hf")
async def validate_question_endpoint(req: QuestionValidationRequest):
    from validateQuestion import validate_question_hf

    return await validate_question_hf(req)


"""
Clinical reasoning endpoint, also implemented in separate module.
"""


# @app.post("/clinicalreasoning", response_model=ClinicalReasoningResponse)
# async def clinical_reasoning_chat(req: ClinicalReasoningRequest):
#     from reasoning import clinical_reasoning_endpoint
#     return await clinical_reasoning_endpoint(req)


@app.post("/clinicalreasoning/hf")
async def clinical_reasoning_chat_hf(req: ClinicalReasoningRequest):
    from reasoning import clinical_reasoning_stream_hf

    return await clinical_reasoning_stream_hf(req)
