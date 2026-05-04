# rag_loader.py

from pathlib import Path
from typing import Optional, List
from functools import lru_cache
import hashlib
import os
import re

from langchain_core.documents import Document
from langchain_community.document_loaders import (
    PyPDFLoader,
    TextLoader
)
from langchain_text_splitters import (
    RecursiveCharacterTextSplitter
)
from langchain_community.vectorstores import FAISS
from langchain_core.embeddings import Embeddings

from fastembed import TextEmbedding

from config import logger

# =========================================================
# CONFIG
# =========================================================

_EMBED_MODEL = os.getenv("EMBEDDING_MODEL", "jinaai/jina-embeddings-v2-small-en")

# Better defaults for medical RAG
_CHUNK_SIZE = int(os.getenv("CHUNK_SIZE", "900"))

_CHUNK_OVERLAP = int(os.getenv("CHUNK_OVERLAP", "180"))

BASE_DIR = Path(__file__).resolve().parent

DATA_DIR = BASE_DIR / "data"

INDEX_PATH = BASE_DIR / "faiss_index"

logger.info(f"FAISS index path: {INDEX_PATH}")


# =========================================================
# FASTEMBED EMBEDDINGS
# =========================================================

class FastEmbedEmbeddings(Embeddings):

    def __init__(self, model_name: str):
        logger.info(f"Loading embedding model: {model_name}")
        self.model = TextEmbedding(model_name=model_name, cache_dir="/tmp/fastembed_cache")

    def embed_documents(self, texts: List[str]) -> List[List[float]]:
        embeddings = list(self.model.embed(texts))
        return [embedding.tolist() for embedding in embeddings]

    def embed_query(self, text: str) -> List[float]:
        embedding = next(self.model.embed([text]))
        return embedding.tolist()


@lru_cache(maxsize=1)
def get_embeddings():
    return FastEmbedEmbeddings(model_name=_EMBED_MODEL)


# =========================================================
# RAG LOADER
# =========================================================

class RAGLoader:

    def __init__(self, data_dir: Optional[str] = None):
        self.data_dir = (Path(data_dir) if data_dir else DATA_DIR)

        self.retriever = None
        self.indexed = False
    # =====================================================
    # PUBLIC METHODS
    # =====================================================

    def load(self):
        if not self._index_exists():
            logger.info("FAISS index not found. Building new index...")
            return self.load_and_index()

        logger.info("Loading FAISS index...")

        vectorstore = FAISS.load_local(
            str(INDEX_PATH),
            get_embeddings(),
            allow_dangerous_deserialization=True
        )

        self.retriever = (self._build_retriever(vectorstore))
        self.indexed = True

        logger.info("FAISS loaded successfully")

        return self.retriever

    def load_and_index(self):
        files = self._get_supported_files()

        if not files:
            logger.error(f"No supported files found in: {self.data_dir}")
            return None

        docs = self._load_documents(files)

        if not docs:
            logger.error("No documents loaded")
            return None

        chunks = self._split_documents(docs)

        logger.info(f"Generating embeddings for " f"{len(chunks)} chunks...")

        vectorstore = FAISS.from_documents(chunks, get_embeddings())
        INDEX_PATH.mkdir(parents=True,exist_ok=True)
        vectorstore.save_local(str(INDEX_PATH))
        self.retriever = (self._build_retriever(vectorstore))
        self.indexed = True

        logger.info("RAG indexing completed")

        return self.retriever

    def get_retriever(self):
        return (self.retriever if self.indexed else self.load())

    # =====================================================
    # INTERNAL METHODS
    # =====================================================

    def _index_exists(self) -> bool:
        return (INDEX_PATH / "index.faiss").exists()

    def _get_supported_files(self) -> List[Path]:
        files = []

        for ext in ["*.pdf", "*.txt", "*.md"]:
            files.extend(self.data_dir.rglob(ext))

        return files

    def _load_documents(self, files: List[Path]):
        docs = []

        for path in files:
            try:
                logger.info(f"Loading: {path.name}")
                loaded_docs = (self._load_single_file(path))
                processed_docs = (self._prepare_documents(loaded_docs, path))
                docs.extend(processed_docs)

            except Exception as e:
                logger.error(f"Failed to load " f"{path.name}: {e}")

        return docs

    def _load_single_file(self, path: Path):

        suffix = (path.suffix.lower())

        if suffix == ".pdf":
            loader = PyPDFLoader(str(path))
            return loader.load()

        if suffix in [".txt", ".md"]:
            loader = TextLoader(str(path), encoding="utf-8")
            return loader.load()

        return []

    def _prepare_documents(self, docs, path: Path):
        processed = []

        for index, doc in enumerate(docs):
            content = (self._clean_text(doc.page_content))

            if len(content.strip()) < 50:
                continue

            metadata = (doc.metadata or {})

            metadata.update({
                # tracing
                "source": path.name,
                "file_type": (path.suffix.lower()),
                
                # classification
                "document_type": self._detect_doc_type(path.name),

                # location
                "page": metadata.get("page", index),

                # stable id
                "doc_id": self._generate_doc_id(content)
            })

            processed.append(Document(page_content=content, metadata=metadata))

        return processed

    def _split_documents(self, docs):

        splitter = (
            RecursiveCharacterTextSplitter(
                chunk_size=_CHUNK_SIZE,
                chunk_overlap=
                _CHUNK_OVERLAP,
                separators=[
                    "\n# ",
                    "\n## ",
                    "\n### ",
                    "\n\n",
                    "\n",
                    ". ",
                    "; ",
                    ", ",
                    " "
                ]
            )
        )

        chunks = (splitter.split_documents(docs))
        enriched_chunks = []

        for idx, chunk in enumerate(chunks):
            chunk.metadata["chunk_id"] = idx
            chunk.metadata["section_title"] = (self._extract_section_title(chunk.page_content))
            enriched_chunks.append(chunk)

        logger.info(f"Split into " f"{len(enriched_chunks)} chunks")

        return enriched_chunks

    def _build_retriever(self, vectorstore):

        return vectorstore.as_retriever(
            search_type="mmr",
            search_kwargs={
                # return top chunks
                "k": 12,
                # candidate pool
                "fetch_k": 40,
                # diversity
                "lambda_mult": 0.65
            }
        )

    # =====================================================
    # TEXT CLEANING
    # =====================================================

    def _clean_text(self, text: str ) -> str:
        # remove repeated spaces
        text = re.sub(r"\s+", " ", text)

        # normalize line breaks
        text = re.sub(r"\n+", "\n", text)

        return text.strip()

    def _extract_section_title(self, text: str) -> str:
        lines = text.split("\n")

        if not lines:
            return "Unknown"

        first_line = (lines[0].strip())

        return first_line[:120]

    def _detect_doc_type(self, filename: str) -> str:
        name = (filename.lower())

        if "guideline" in name:
            return "guideline"

        if "textbook" in name:
            return "textbook"

        if "protocol" in name:
            return "protocol"

        if "case" in name:
            return "case-study"

        return "general-medical"

    def _generate_doc_id(self, text: str) -> str:
        return hashlib.md5(text.encode()).hexdigest()
