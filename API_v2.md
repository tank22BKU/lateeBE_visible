# API V2

Tài liệu này cung cấp ví dụ request/response fake data cho các API đang có trong các service bên dưới.

## PracticeSessionService

### POST /api/practice-sessions
Request:
```json
{
  "id": "SESS_20260515090000",
  "learnerId": "USR-LRN-08",
  "patientId": "10070247",
  "moduleId": "EPA_STANDARD_V1",
  "discussionType": "Message Type",
  "guidelinesId": "GL-001",
  "status": "Practicing"
}
```
Response:
```json
{
  "id": "SESS_20260515090000"
}
```

### POST /api/practice-sessions/submit
Request:
```json
{
  "sessionId": "SESS_20260515090000",
  "learnerId": "USR-LRN-08",
  "finalDiagnosis": "Acute appendicitis",
  "vpConversationLog": {
    "messages": [
      { "role": "learner", "content": "When did the pain start?" },
      { "role": "patient", "content": "About 12 hours ago." }
    ]
  },
  "aiReasoningLog": {
    "steps": [
      { "step": 1, "content": "Considered appendicitis due to RLQ pain." },
      { "step": 2, "content": "Checked for fever and rebound tenderness." }
    ]
  },
  "moduleId": "EPA_STANDARD_V1",
  "discussionType": "Message Type",
  "guidelinesId": "GL-001",
  "warnings": [
    { "warningId": "W-001", "label": "Incomplete HPI", "description": "Onset details were missing in early conversation." },
    { "warningId": "W-002", "label": "Limited ROS", "description": "System review was not fully explored." }
  ]
}
```
Response:
```json
{
  "sessionId": "SESS_20260515090000"
}
```

### GET /api/practice-sessions/{id}
Response:
```json
{
  "sessionId": "SESS_20260515090000",
  "learnerId": "USR-LRN-08",
  "patientId": "10070247",
  "moduleId": "EPA_STANDARD_V1",
  "discussionType": "Message Type",
  "guidelinesId": "GL-001",
  "vpConversationLog": {
    "messages": [
      { "role": "learner", "content": "Do you have nausea?" },
      { "role": "patient", "content": "Yes, since this morning." }
    ]
  },
  "aiReasoningLog": {
    "steps": [
      { "step": 1, "content": "Generated differential diagnoses." },
      { "step": 2, "content": "Narrowed toward appendicitis after RLQ pain confirmation." }
    ]
  },
  "finalDiagnosis": "Acute appendicitis",
  "status": "Completed",
  "startTime": "2026-05-15T09:00:00Z",
  "endTime": "2026-05-15T09:27:00Z",
  "createdAt": "2026-05-15T09:00:00Z",
  "warnings": [
    { "warningId": "W-001", "label": "Incomplete HPI", "description": "Onset details were missing in early conversation." }
  ]
}
```

### GET /api/practice-sessions/clinical-cases?status=active&page=1&pageSize=20
Response:
```json
{
  "items": [
    { "id": "27892518", "title": "Acute appendicitis case", "type": "APPENDICITIS", "status": "active" },
    { "id": "27892519", "title": "Right lower quadrant pain case", "type": "ABDOMINAL_PAIN", "status": "active" }
  ],
  "total": 2,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

### GET /api/practice-sessions/attempt-count?learnerId={learnerId}&patientId={patientId}
Response:
```json
{
  "learnerId": "USR-LRN-08",
  "patientId": "10070247",
  "attemptCount": 2,
  "maxAttempts": 3,
  "canAttempt": true
}
```

### GET /api/practice-sessions/active?learnerId={learnerId}&patientId={patientId}
Response:
```json
{
  "sessionId": "SESS_20260515090000",
  "status": "Practicing",
  "startTime": "2026-05-15T09:00:00Z",
  "patientId": "10070247"
}
```

### PATCH /api/practice-sessions/{id}/status
Request:
```json
{
  "status": "VpCompleted"
}
```
Response:
```json
{
  "sessionId": "SESS_20260515090000",
  "status": "VpCompleted"
}
```

## EvaluationService

### POST /api/evaluation/submit
Request:
```json
{
  "practiceSessionId": "SESS_20260515090000",
  "learnerId": "USR-LRN-08",
  "epaId": "EPA-001",
  "score": 88.5,
  "duration": 27,
  "feedbackDetail": "Good clinical reasoning and appropriate follow-up questions.",
  "entrustmentLevel": 4,
  "finalDiagnosis": "Acute appendicitis",
  "vpConversationLog": {
    "messages": [
      { "role": "learner", "content": "Any vomiting?" },
      { "role": "patient", "content": "Yes, once." }
    ]
  },
  "aiReasoningLog": {
    "steps": [
      { "step": 1, "content": "Detected classic appendicitis pattern." },
      { "step": 2, "content": "Prepared final summary." }
    ]
  },
  "discussionType": "Message Type",
  "moduleId": "EPA_STANDARD_V1",
  "warnings": [
    { "warningId": "W-001", "label": "Incomplete HPI", "description": "Onset details were not asked immediately." }
  ]
}
```
Response:
```json
{
  "message": "Evaluation saved successfully.",
  "data": {
    "evaluationId": "EVAL-20260515-001",
    "practiceSessionId": "SESS_20260515090000",
    "score": 88.5,
    "entrustmentLevel": 4,
    "feedbackDetail": "Good clinical reasoning and appropriate follow-up questions.",
    "finalDiagnosis": "Acute appendicitis",
    "discussionType": "Message Type",
    "duration": 27
  }
}
```

### GET /api/evaluation/{userId}/history
Response:
```json
[
  {
    "evaluationId": "EVAL-20260515-001",
    "practiceSessionId": "SESS_20260515090000",
    "score": 88.5,
    "createdAt": "2026-05-15T09:31:00Z"
  },
  {
    "evaluationId": "EVAL-20260510-014",
    "practiceSessionId": "SESS_20260510081500",
    "score": 81.0,
    "createdAt": "2026-05-10T08:45:00Z"
  }
]
```

### GET /api/evaluation/{id}/report
Response:
```json
{
  "evaluationId": "EVAL-20260515-001",
  "epaId": "EPA-001",
  "practiceSessionId": "SESS_20260515090000",
  "learnerId": "USR-LRN-08",
  "patientId": "10070247",
  "moduleId": "EPA_STANDARD_V1",
  "discussionType": "Message Type",
  "finalDiagnosis": "Acute appendicitis",
  "vpConversationLog": {
    "messages": [
      { "role": "learner", "content": "Where exactly is the pain?" },
      { "role": "patient", "content": "It moved to the right lower abdomen." }
    ]
  },
  "aiReasoningLog": {
    "steps": [
      { "step": 1, "content": "Assessed localization and progression of pain." },
      { "step": 2, "content": "Evaluated red flags and surgical urgency." }
    ]
  },
  "score": 88.5,
  "duration": 27,
  "feedbackDetail": "Good clinical reasoning and appropriate follow-up questions.",
  "entrustmentLevel": 4,
  "createdAt": "2026-05-15T09:31:00Z",
  "warnings": [
    { "warningId": "W-001", "label": "Incomplete HPI", "description": "Onset details were not asked immediately." }
  ]
}
```

### POST /api/evaluation/practice-feedback/{practiceSessionId}
Request:
```json
{}
```
Response:
```json
{
  "message": "Feedback generated successfully.",
  "data": {
    "practiceSessionId": "SESS_20260515090000",
    "wasCached": false,
    "feedbackDetail": "The learner asked focused history questions and reached the correct diagnosis.",
    "strengths": [
      "Structured reasoning",
      "Focused abdominal pain history"
    ],
    "improvements": [
      "Ask onset details earlier",
      "Confirm associated symptoms more systematically"
    ]
  }
}
```

### DELETE /api/evaluation/{id}
Response:
```json
{
  "message": "Evaluation deleted successfully."
}
```

### GET /evaluation/api/issues?practiceSessionId={sessionId}&learnerId={learnerId}
Response:
```json
{
  "items": [
    {
      "issueId": "ISS-001",
      "learnerId": "USR-LRN-08",
      "learnerName": "Dr. Smith",
      "createdAt": "2026-03-12T10:00:00Z",
      "label": "Clinical Logic",
      "description": "The recommended dosage seems high given the decreased GFR.",
      "status": "Resolved",
      "expertFeedback": {
        "expertId": "EXP-001",
        "expertName": "Dr. Alexander Pierce",
        "feedback": "Good catch. The case parameters reflect guideline-directed therapy."
      }
    }
  ]
}
```

### POST /evaluation/api/issues
Request:
```json
{
  "practiceSessionId": "SESS_20260515090000",
  "learnerId": "USR-LRN-08",
  "label": "Clinical Logic",
  "description": "The dosage seems high.",
  "itemType": "Practice"
}
```
Response:
```json
{
  "message": "Issue submitted successfully.",
  "issueId": "ISS-002"
}
```

## VirtualPatientService

### GET /api/virtual-patients?gender=MALE&page=1&pageSize=20
Response:
```json
{
  "items": [
    {
      "patientId": "10070247",
      "caseId": "27892518",
      "name": "Richard Anderson",
      "age": 43,
      "gender": "MALE",
      "pronouns": "he/him",
      "ethnicity": "Hispanic",
      "occupation": "Warehouse worker",
      "chiefConcern": "Abdominal pain",
      "medicalHistory": "History of intermittent reflux and seasonal allergies.",
      "symptom": "Right lower quadrant pain",
      "persona": { "emotional_state": "Anxious" },
      "vitalSigns": { "bp": "114/91", "hr": 79, "temp": 37.8, "spo2": "98%", "rr": 18 },
      "instructions": {
        "role": "Medical Learner",
        "task": "Take a focused history from this patient presenting with abdominal pain.",
        "tone": "Provide short answers unless asked directly",
        "procedure": [
          "Introduce yourself and establish rapport",
          "Elicit a focused history using OLDCARTS",
          "Identify red flags and pertinent negatives",
          "Formulate a differential diagnosis"
        ]
      },
      "behaviors": ["Low pain tolerance", "Gives brief answers initially"],
      "timeSetting": 30,
      "argumentTime": 15,
      "learningObjectives": [
        "Take focused abdominal pain history",
        "Identify red flags for surgical abdomen"
      ],
      "level": "Intermediate",
      "avatarImage": "/images/patients/richard-anderson.png",
      "caseRule": {
        "rules": ["Complete HPI", "Perform ROS", "Identify at least 2 differential diagnoses"],
        "totalTime": "45 min",
        "timeBreakdown": [
          "30 minutes for patient interaction",
          "15 minutes for explanation and reasoning"
        ]
      },
      "status": "active",
      "createdAt": "2026-05-15T09:00:00Z",
      "updatedAt": "2026-05-15T09:12:00Z",
      "experts": [
        {
          "expertId": "EXP-001",
          "name": "Dr. Andrew Nguyen",
          "role": "Specialist in Diagnostic Reasoning",
          "avatarUrl": "/images/d22.jpg",
          "bioQuote": "Leading expert in complex clinical case analysis...",
          "educationDetail": "MD Internal Medicine, Johns Hopkins University",
          "expertiseSkill": "Clinical Reasoning, Diagnostic Strategy",
          "phone": "(568) 367-987-237",
          "email": "andrew.nguyen@latee.com",
          "location": "Hudson, Wisconsin"
        }
      ]
    }
  ],
  "total": 1,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

### GET /api/virtual-patients/{id}
Response:
```json
{
  "patientId": "10070247",
  "caseId": "27892518",
  "name": "Richard Anderson",
  "age": 43,
  "gender": "MALE",
  "pronouns": "he/him",
  "ethnicity": "Hispanic",
  "occupation": "Warehouse worker",
  "chiefConcern": "Abdominal pain",
  "medicalHistory": "History of intermittent reflux and seasonal allergies.",
  "symptom": "Right lower quadrant pain",
  "persona": { "emotional_state": "Anxious" },
  "vitalSigns": {
    "bp": "114/91",
    "hr": 79,
    "temp": 37.8,
    "spo2": "98%",
    "rr": 18
  },
  "instructions": {
    "role": "Medical Learner",
    "task": "Take a focused history from this patient presenting with abdominal pain.",
    "tone": "Provide short answers unless asked directly",
    "procedure": [
      "Introduce yourself and establish rapport",
      "Elicit a focused history using OLDCARTS",
      "Identify red flags and pertinent negatives",
      "Formulate a differential diagnosis"
    ]
  },
  "behaviors": ["Low pain tolerance", "Gives brief answers initially"],
  "timeSetting": 30,
  "argumentTime": 15,
  "learningObjectives": [
    "Take focused abdominal pain history",
    "Identify red flags for surgical abdomen"
  ],
  "level": "Intermediate",
  "avatarImage": "/images/patients/richard-anderson.png",
  "caseRule": {
    "rules": ["Complete HPI", "Perform ROS", "Identify at least 2 differential diagnoses"],
    "totalTime": "45 min",
    "timeBreakdown": [
      "30 minutes for patient interaction",
      "15 minutes for explanation and reasoning"
    ]
  },
  "status": "active",
  "createdAt": "2026-05-15T09:00:00Z",
  "updatedAt": "2026-05-15T09:12:00Z",
  "experts": [
    {
      "expertId": "EXP-001",
      "name": "Dr. Andrew Nguyen",
      "role": "Specialist in Diagnostic Reasoning",
      "avatarUrl": "/images/d22.jpg",
      "bioQuote": "Leading expert in complex clinical case analysis...",
      "educationDetail": "MD Internal Medicine, Johns Hopkins University",
      "expertiseSkill": "Clinical Reasoning, Diagnostic Strategy",
      "phone": "(568) 367-987-237",
      "email": "andrew.nguyen@latee.com",
      "location": "Hudson, Wisconsin"
    }
  ]
}
```

## AIAssistantService

### GET /health
Response:
```json
{
  "status": "ok"
}
```

### POST /assistant/stream/hf
Request:
```json
{
  "doctor_id": "DR-001",
  "question": "What is the most likely cause of right lower quadrant pain in this patient?",
  "patient_history": [
    { "role": "doctor", "content": "Can you show me where it hurts?" },
    { "role": "patient", "content": "It starts around the belly button and moved to the right side." }
  ],
  "use_rag": true
}
```
Response:
```json
{
  "type": "done",
  "source_documents": [
    "abdominal_pain_guideline.md",
    "appendicitis_workflow.md"
  ],
  "full_answer": "The most likely cause is acute appendicitis based on migration of pain, localized RLQ tenderness, and associated nausea."
}
```

### POST /assistant/validate_question/hf
Request:
```json
{
  "doctor_id": "DR-001",
  "learner_question": "Does the pain get worse when you move?",
  "conversation_context": [
    { "role": "doctor", "content": "Tell me about the pain." },
    { "role": "patient", "content": "It is sharp and localized on the right side." }
  ]
}
```
Response:
```json
{
  "isValid": true,
  "reason": "Câu hỏi phù hợp với ngữ cảnh chẩn đoán hiện tại.",
  "suggestion": "",
  "severity": "medium",
  "category": "clinical_relevance",
  "confidence": 0.92
}
```

### POST /clinicalreasoning/hf
Request:
```json
{
  "patient_case": "43-year-old male with migrating abdominal pain, nausea, and low-grade fever.",
  "learner_diagnosis": "Acute appendicitis",
  "interaction_history": [
    {
      "dimension": "evidence",
      "question": "What findings support your diagnosis?",
      "answer": "Migrating pain and RLQ tenderness are consistent with appendicitis."
    },
    {
      "dimension": "differential",
      "question": "What other diagnoses should be considered?",
      "answer": "Gastroenteritis, mesenteric adenitis, and renal colic."
    }
  ]
}
```
Response:
```json
{
  "type": "done",
  "dimension": "missing_info",
  "question": "What additional exam finding would increase your confidence?",
  "stop": false,
  "full_raw": "{\"dimension\":\"missing_info\",\"question\":\"What additional exam finding would increase your confidence?\",\"stop\":false}"
}
```
