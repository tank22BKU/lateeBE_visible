# rag_loader.py
from pathlib import Path
from typing import Optional
from langchain_community.document_loaders import PyPDFLoader
from langchain_text_splitters import RecursiveCharacterTextSplitter
from langchain_community.embeddings import HuggingFaceEmbeddings
from langchain_community.vectorstores import FAISS
import os

_EMBED_MODEL = os.getenv(
    "EMBEDDING_MODEL", "sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2"
)
_CHUNK_SIZE = int(os.getenv("CHUNK_SIZE", "2500"))
_CHUNK_OVERLAP = int(os.getenv("CHUNK_OVERLAP", "200"))


class RAGLoader:
    def __init__(self, pdf_dir: str = "./data"):
        self.pdf_dir = Path(pdf_dir)
        self.retriever = None
        self.indexed = False

    def load_and_index(self):
        pdf_files = list(self.pdf_dir.glob("*.pdf"))
        if not pdf_files:
            print("RAG: No PDFs found in", self.pdf_dir)
            return None
        docs = []
        for p in pdf_files:
            try:
                loader = PyPDFLoader(str(p))
                loaded = loader.load()
                for d in loaded:
                    # attach metadata source
                    if not hasattr(d, "metadata") or d.metadata is None:
                        d.metadata = {}
                    d.metadata["source"] = p.name
                docs.extend(loaded)
                print(f"Loaded {len(loaded)} pages from {p.name}")
            except Exception as e:
                print("Failed to load", p, e)

        if not docs:
            print("No docs loaded")
            return None

        splitter = RecursiveCharacterTextSplitter(
            chunk_size=_CHUNK_SIZE, chunk_overlap=_CHUNK_OVERLAP
        )
        chunks = splitter.split_documents(docs)
        # print("Created chunks:", len(chunks))

        embeddings = HuggingFaceEmbeddings(
            model_name=_EMBED_MODEL,
            model_kwargs={"device": "cpu"},
            encode_kwargs={"normalize_embeddings": True},
        )
        vectorstore = FAISS.from_documents(chunks, embeddings)
        self.retriever = vectorstore.as_retriever(search_kwargs={"k": 10})
        self.indexed = True
        print("RAG indexing complete")
        return self.retriever

    def get_retriever(self):
        if self.indexed:
            return self.retriever
        else:
            return self.load_and_index()
