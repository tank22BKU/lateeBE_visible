from typing import List, Optional, Dict, Any, Tuple
from pydantic import BaseModel, Field
import time

HISTORY_MAX_ITEMS = 50  # int(os.getenv("HISTORY_MAX_ITEMS", "50"))
# History config
HISTORY_TTL_SECONDS = (
    3600  # int(os.getenv("HISTORY_TTL_SECONDS", "3600"))  # 1 hour default
)

try:
    from cachetools import TTLCache
except Exception:
    TTLCache = None


# --------------------
# Request/Response models
# --------------------
class MessageItem(BaseModel):
    role: str  # "doctor" or "patient" or "system"
    content: str


class AssistantRequest(BaseModel):
    doctor_id: str
    question: str
    patient_history: List[MessageItem] = Field(default_factory=list)
    use_rag: Optional[bool] = True


class AssistantResponse(BaseModel):
    answer: str
    source_documents: Optional[List[str]] = None


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


class QuestionValidationRequest(BaseModel):
    doctor_id: str
    learner_question: str  # Câu hỏi của learner dành cho bệnh nhân
    conversation_context: List[MessageItem] = Field(
        default_factory=list
    )  # Lịch sử hội thoại để hiểu ngữ cảnh


class QuestionValidationResponse(BaseModel):
    isValid: bool
    reason: str = Field(
        default="",
        max_length=300,
    )
    suggestion: str = Field(
        default="",
        max_length=500,
    )
    severity: str = Field(
        default="medium",
    )
    category: str = Field(
        default="unknown",
    )
    confidence: float = Field(
        default=0.5,
        ge=0.0,
        le=1.0,
    )


class ValidationFlag(BaseModel):
    isValid: bool
    reason: str
    suggestion: Optional[str] = ""


class ClinicalReasoningInteraction(BaseModel):
    dimension: str
    question: str
    answer: str


class ClinicalReasoningRequest(BaseModel):
    patient_case: str
    learner_diagnosis: str
    interaction_history: List[ClinicalReasoningInteraction] = Field(
        default_factory=list
    )


class ClinicalReasoningResponse(BaseModel):
    dimension: str
    question: str
    stop: bool
