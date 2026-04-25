# rag_loader.py
from pathlib import Path
from typing import Optional, List
import os
from functools import lru_cache

from langchain_community.document_loaders import PyPDFLoader
from langchain_text_splitters import RecursiveCharacterTextSplitter
from langchain_huggingface import HuggingFaceEmbeddings
from langchain_community.vectorstores import FAISS

from config import logger


# ==============================
# CONFIG
# ==============================

_EMBED_MODEL = os.getenv(
    "EMBEDDING_MODEL", "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2"
)

_CHUNK_SIZE = int(os.getenv("CHUNK_SIZE", "600"))
_CHUNK_OVERLAP = int(os.getenv("CHUNK_OVERLAP", "100"))

BASE_DIR = Path(__file__).resolve().parent
INDEX_PATH = BASE_DIR / "faiss_index"

logger.info(f"Index path: {INDEX_PATH}")


# ==============================
# EMBEDDING
# ==============================

@lru_cache()
def get_embeddings():
    return HuggingFaceEmbeddings(
        model_name=_EMBED_MODEL,
        model_kwargs={"device": "cpu"},
        encode_kwargs={"normalize_embeddings": True},
    )


# ==============================
# RAG LOADER
# ==============================

class RAGLoader:
    def __init__(self, pdf_dir: Optional[str] = None):
        self.pdf_dir = Path(pdf_dir) if pdf_dir else BASE_DIR / "data"
        self.retriever = None
        self.indexed = False

    # --------------------------
    # PUBLIC METHODS
    # --------------------------

    def load(self):
        """Load FAISS index or build if missing"""
        if not self._index_exists():
            return self.load_and_index()

        vectorstore = FAISS.load_local(
            INDEX_PATH,
            get_embeddings(),
            allow_dangerous_deserialization=True
        )

        self.retriever = self._build_retriever(vectorstore)
        self.indexed = True

        logger.info("FAISS loaded")
        return self.retriever

    def load_and_index(self):
        """Load PDFs, split, embed and store FAISS index"""
        pdf_files = self._get_pdf_files()
        if not pdf_files:
            logger.error(f"RAG: No PDFs found in {self.pdf_dir}")
            return None

        docs = self._load_documents(pdf_files)
        if not docs:
            logger.error("No docs loaded")
            return None

        chunks = self._split_documents(docs)

        vectorstore = FAISS.from_documents(chunks, get_embeddings())
        vectorstore.save_local(INDEX_PATH)

        self.retriever = self._build_retriever(vectorstore)
        self.indexed = True

        logger.info("RAG indexing complete")
        return self.retriever

    def get_retriever(self):
        """Return retriever (lazy load)"""
        return self.retriever if self.indexed else self.load()

    # --------------------------
    # INTERNAL METHODS
    # --------------------------

    def _index_exists(self) -> bool:
        return (INDEX_PATH / "index.faiss").exists()

    def _get_pdf_files(self) -> List[Path]:
        return list(self.pdf_dir.glob("*.pdf"))

    def _load_documents(self, pdf_files: List[Path]):
        docs = []

        for path in pdf_files:
            try:
                loader = PyPDFLoader(str(path))
                loaded_docs = loader.load()

                for doc in loaded_docs:
                    doc.metadata = doc.metadata or {}
                    doc.metadata["source"] = path.name

                docs.extend(loaded_docs)

            except Exception as e:
                logger.error(f"Failed to load {path}: {e}")

        return docs

    def _split_documents(self, docs):
        splitter = RecursiveCharacterTextSplitter(
            chunk_size=_CHUNK_SIZE,
            chunk_overlap=_CHUNK_OVERLAP
        )
        return splitter.split_documents(docs)

    def _build_retriever(self, vectorstore):
        return vectorstore.as_retriever(
            search_type="mmr",
            search_kwargs={
                "k": 10,
                "fetch_k": 30,
                "lambda_mult": 0.5
            }
        )