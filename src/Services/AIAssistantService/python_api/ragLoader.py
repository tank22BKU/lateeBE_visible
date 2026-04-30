# rag_loader.py

from pathlib import Path
from typing import Optional, List
from functools import lru_cache
import os

from langchain_community.document_loaders import PyPDFLoader
from langchain_text_splitters import RecursiveCharacterTextSplitter
from langchain_community.vectorstores import FAISS
from langchain_core.embeddings import Embeddings

from fastembed import TextEmbedding

from config import logger


# =========================================================
# CONFIG
# =========================================================

_EMBED_MODEL = os.getenv(
    "EMBEDDING_MODEL",
    "jinaai/jina-embeddings-v2-small-en"
)

_CHUNK_SIZE = int(os.getenv("CHUNK_SIZE", "600"))
_CHUNK_OVERLAP = int(os.getenv("CHUNK_OVERLAP", "100"))

BASE_DIR = Path(__file__).resolve().parent

INDEX_PATH = BASE_DIR / "faiss_index"

logger.info(f"FAISS index path: {INDEX_PATH}")


# =========================================================
# FASTEMBED EMBEDDINGS
# =========================================================

class FastEmbedEmbeddings(Embeddings):

    def __init__(self, model_name: str):

        logger.info(
            f"Loading embedding model: {model_name}"
        )

        self.model = TextEmbedding(
            model_name=model_name,
            cache_dir="/tmp/fastembed_cache"
        )

    def embed_documents(
            self,
            texts: List[str]
    ) -> List[List[float]]:

        embeddings = list(
            self.model.embed(texts)
        )

        return [
            embedding.tolist()
            for embedding in embeddings
        ]

    def embed_query(
            self,
            text: str
    ) -> List[float]:

        embedding = next(
            self.model.embed([text])
        )

        return embedding.tolist()


@lru_cache(maxsize=1)
def get_embeddings():

    return FastEmbedEmbeddings(
        model_name=_EMBED_MODEL
    )


# =========================================================
# RAG LOADER
# =========================================================

class RAGLoader:

    def __init__(
            self,
            pdf_dir: Optional[str] = None
    ):

        self.pdf_dir = (
            Path(pdf_dir)
            if pdf_dir
            else BASE_DIR / "data"
        )

        self.retriever = None
        self.indexed = False

    # =====================================================
    # PUBLIC METHODS
    # =====================================================

    def load(self):

        if not self._index_exists():

            logger.info(
                "FAISS index not found. Building new index..."
            )

            return self.load_and_index()

        logger.info("Loading FAISS index...")

        vectorstore = FAISS.load_local(
            str(INDEX_PATH),
            get_embeddings(),
            allow_dangerous_deserialization=True
        )

        self.retriever = self._build_retriever(
            vectorstore
        )

        self.indexed = True

        logger.info(
            "FAISS loaded successfully"
        )

        return self.retriever

    def load_and_index(self):

        pdf_files = self._get_pdf_files()

        if not pdf_files:

            logger.error(
                f"No PDFs found in: {self.pdf_dir}"
            )

            return None

        docs = self._load_documents(
            pdf_files
        )

        if not docs:

            logger.error(
                "No documents loaded"
            )

            return None

        chunks = self._split_documents(
            docs
        )

        logger.info(
            f"Generating embeddings for {len(chunks)} chunks..."
        )

        vectorstore = FAISS.from_documents(
            chunks,
            get_embeddings()
        )

        INDEX_PATH.mkdir(
            parents=True,
            exist_ok=True
        )

        vectorstore.save_local(
            str(INDEX_PATH)
        )

        self.retriever = self._build_retriever(
            vectorstore
        )

        self.indexed = True

        logger.info(
            "RAG indexing completed"
        )

        return self.retriever

    def get_retriever(self):

        return (
            self.retriever
            if self.indexed
            else self.load()
        )

    # =====================================================
    # INTERNAL METHODS
    # =====================================================

    def _index_exists(self) -> bool:

        return (
                INDEX_PATH / "index.faiss"
        ).exists()

    def _get_pdf_files(self) -> List[Path]:

        return list(
            self.pdf_dir.glob("*.pdf")
        )

    def _load_documents(
            self,
            pdf_files: List[Path]
    ):

        docs = []

        for path in pdf_files:

            try:

                logger.info(
                    f"Loading PDF: {path.name}"
                )

                loader = PyPDFLoader(
                    str(path)
                )

                loaded_docs = loader.load()

                for doc in loaded_docs:

                    doc.metadata = (
                            doc.metadata or {}
                    )

                    doc.metadata[
                        "source"
                    ] = path.name

                docs.extend(
                    loaded_docs
                )

            except Exception as e:

                logger.error(
                    f"Failed to load {path.name}: {e}"
                )

        return docs

    def _split_documents(
            self,
            docs
    ):

        splitter = (
            RecursiveCharacterTextSplitter(
                chunk_size=_CHUNK_SIZE,
                chunk_overlap=_CHUNK_OVERLAP
            )
        )

        chunks = splitter.split_documents(
            docs
        )

        logger.info(
            f"Split into {len(chunks)} chunks"
        )

        return chunks

    def _build_retriever(
            self,
            vectorstore
    ):

        return vectorstore.as_retriever(
            search_type="mmr",
            search_kwargs={
                "k": 10,
                "fetch_k": 30,
                "lambda_mult": 0.5
            }
        )