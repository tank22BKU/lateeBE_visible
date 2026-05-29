# API V2

Tài liệu này cung cấp ví dụ request/response fake data cho các API đang có trong các service bên dưới.

## UserService

### GET /api/experts
Trả về danh sách expert tối giản để FE populate dropdown / autocomplete.
Query params:
- keyword: string|null
Response:
```json
[
  { "expertId": "EXP-001", "name": "Dr. Anna Nguyen" },
  { "expertId": "EXP-002", "name": "Dr. Minh Tran" }
]
```

### GET /api/experts/search?keyword={keyword}
Tìm expert theo `expertId` hoặc `name`.
Response:
```json
[
  { "expertId": "EXP-001", "name": "Dr. Anna Nguyen" }
]
```

## PracticeSessionService

### POST /api/practice-sessions
Request:
Request fields (types):
- id: string
- learnerId: string
- patientId: string
- moduleId: string
- discussionType: string
- guidelinesId: string
- status: string
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
Request fields (types):
- sessionId: string
- learnerId: string
- finalDiagnosis: string|null
- vpConversationLog: object|null
- aiReasoningLog: object|null
- moduleId: string|null
- discussionType: string|null
- guidelinesId: string|null
- warnings: array of { warningId: string, label: string, description: string }
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
```

### GET /api/practice-sessions/active?learnerId={learnerId}&patientId={patientId}
```json
{
  "sessionId": "SESS_20260515090000",
  "status": "Practicing",
  "startTime": "2026-05-15T09:00:00Z",
  "patientId": "10070247"
}
```

### PATCH /api/practice-sessions/{id}/status
Supported values: Practicing, VpCompleted, ReasoningStarted, Submitted, Completed, Abandoned
Request:
Request fields (types):
- status: string
```json
{
  "status": "Submitted"
}
```
Response:
```json
{
  "sessionId": "SESS_20260515090000",
  "status": "Submitted"
}
```

## EvaluationService

### POST /api/evaluation/submit
Request:
Request fields (types):
- practiceSessionId: string
- learnerId: string
- finalDiagnosis: string|null
- vpConversationLog: string|null (JSON string payload)
- aiReasoningLog: string|null (JSON string payload)
- discussionType: string|null
- moduleId: string|null
- warnings: array of { warningId: string, practiceSessionId: string, learnerId: string, label: string, description: string, createdAt: string (date-time) }
```json
{
  "practiceSessionId": "SESS_20260515090000",
  "learnerId": "USR-LRN-08",
  "finalDiagnosis": "Acute appendicitis",
  "vpConversationLog": "{\"messages\":[{\"role\":\"learner\",\"content\":\"Any vomiting?\"},{\"role\":\"patient\",\"content\":\"Yes, once.\"}]}",
  "aiReasoningLog": "{\"steps\":[{\"step\":1,\"content\":\"Detected classic appendicitis pattern.\"},{\"step\":2,\"content\":\"Prepared final summary.\"}]}",
  "discussionType": "Message Type",
  "moduleId": "EPA_STANDARD_V1",
  "warnings": [
    {
      "warningId": "W-001",
      "practiceSessionId": "SESS_20260515090000",
      "learnerId": "USR-LRN-08",
      "label": "Incomplete HPI",
      "description": "Onset details were not asked immediately.",
      "createdAt": "2026-05-15T09:20:00Z"
    }
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
    "pureEpaScore": 86,
    "positiveAdjustmentTotal": 10,
    "negativeAdjustmentTotal": 8,
    "adjustmentTotal": 2,
    "diagnosisMatch": {
      "matchType": "EXACT_MATCH",
      "matchTypeLabel": "Exact match",
      "isAcceptable": true,
      "isDangerous": false,
      "requiresSafetyReview": false
    },
    "diagnosisMatchType": "EXACT_MATCH",
    "diagnosisModifier": 10,
    "timeModifier": -1,
    "warningPenalty": 7,
    "warningCount": 1,
    "safetyEscalationRequired": false,
    "cognitiveAlerts": [],
    "epaScores": [
      {
        "epaId": "EPA-001",
        "numericalScore": 16,
        "maxScore": 20,
        "entrustmentLevel": 4,
        "feedbackDetail": "Strong data gathering with a focused ROS.",
        "evidenceCited": ["Clear onset timeline", "Asked about migration of pain"],
        "failurePatterns": [],
        "safetyFlags": []
      }
    ],
    "adjustments": {
      "positive": [
        {
          "code": "DIAGNOSIS_EXACT_MATCH",
          "title": "Exact diagnosis match",
          "score": 10,
          "reason": "Diagnosis matches the canonical condition.",
          "source": "diagnosis",
          "severity": "positive"
        }
      ],
      "negative": [
        {
          "code": "TIME_OVER_SLIGHT",
          "title": "Slightly over time",
          "score": -1,
          "reason": "Session exceeded allotted time by up to 20%.",
          "source": "time",
          "severity": "low"
        }
      ],
      "validation": {
        "hasEthicsViolation": false,
        "hasUnsafeQuestion": false,
        "hasWorkflowViolation": false,
        "safetyEscalationRequired": false,
        "totalWarnings": 1
      }
    },
    "discussionType": "Message Type",
    "duration": 27,
    "practiceFeedbackAvailable": false
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
    "entrustmentLevel": 4,
    "rubricVersion": "v2",
    "createdAt": "2026-05-15T09:31:00Z"
  },
  {
    "evaluationId": "EVAL-20260510-014",
    "practiceSessionId": "SESS_20260510081500",
    "score": 81.0,
    "entrustmentLevel": 3,
    "rubricVersion": "v2",
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
  "vpConversationLog": "{\"messages\":[{\"role\":\"learner\",\"content\":\"Where exactly is the pain?\"},{\"role\":\"patient\",\"content\":\"It moved to the right lower abdomen.\"}]}",
  "aiReasoningLog": "{\"steps\":[{\"step\":1,\"content\":\"Assessed localization and progression of pain.\"},{\"step\":2,\"content\":\"Evaluated red flags and surgical urgency.\"}]}",
  "score": 88.5,
  "duration": 27,
  "evaluationTrace": null,
  "entrustmentLevel": 4,
  "rubricVersion": "v2",
  "pureEpaScore": 86,
  "positiveAdjustmentTotal": 10,
  "negativeAdjustmentTotal": 8,
  "adjustmentTotal": 2,
  "diagnosisMatch": {
    "matchType": "EXACT_MATCH",
    "matchTypeLabel": "Exact match",
    "isAcceptable": true,
    "isDangerous": false,
    "requiresSafetyReview": false
  },
  "diagnosisMatchType": "EXACT_MATCH",
  "diagnosisModifier": 10,
  "timeModifier": -1,
  "warningPenalty": 7,
  "safetyEscalationRequired": false,
  "cognitiveAlerts": [],
  "epaScores": [
    {
      "epaId": "EPA-001",
      "numericalScore": 16,
      "maxScore": 20,
      "entrustmentLevel": 4,
      "feedbackDetail": "Strong data gathering with a focused ROS.",
      "evidenceCited": ["Clear onset timeline", "Asked about migration of pain"],
      "failurePatterns": [],
      "safetyFlags": []
    }
  ],
  "adjustments": {
    "positive": [
      {
        "code": "DIAGNOSIS_EXACT_MATCH",
        "title": "Exact diagnosis match",
        "score": 10,
        "reason": "Diagnosis matches the canonical condition.",
        "source": "diagnosis",
        "severity": "positive"
      }
    ],
    "negative": [
      {
        "code": "TIME_OVER_SLIGHT",
        "title": "Slightly over time",
        "score": -1,
        "reason": "Session exceeded allotted time by up to 20%.",
        "source": "time",
        "severity": "low"
      }
    ],
    "validation": {
      "hasEthicsViolation": false,
      "hasUnsafeQuestion": false,
      "hasWorkflowViolation": false,
      "safetyEscalationRequired": false,
      "totalWarnings": 1
    }
  },
  "createdAt": "2026-05-15T09:31:00Z",
  "warnings": [
    {
      "warningId": "W-001",
      "practiceSessionId": "SESS_20260515090000",
      "learnerId": "USR-LRN-08",
      "label": "Incomplete HPI",
      "description": "Onset details were not asked immediately.",
      "createdAt": "2026-05-15T09:20:00Z"
    }
  ],
  "practiceFeedback": {
    "id": "FB-20260515-001",
    "overallAttempt": "Good effort",
    "overallLabel": "GOOD",
    "strength": "Focused abdominal pain history",
    "improvement": "EPA 1: Information Gathering — The student did not conduct a thorough history-taking process. They should have asked more detailed questions about the patient's respiratory and gastrointestinal symptoms, including onset, duration, and any exacerbating or alleviating factors.\nEPA 2: Diagnosis Reasoning — There was no evidence of considering a differential diagnosis. The student should have linked the patient's symptoms and medical history to potential conditions such as CVID, bronchiectasis, or recurrent infections.\nEPA 3: Diagnosis Testing — The tests ordered were not appropriate for the symptoms presented. The student should have considered ordering immunoglobulin levels or a CT scan of the chest to evaluate for CVID or bronchiectasis.\nEPA 4: Management Plan — There was no management plan formulated. The student should have discussed potential modifications to the patient's current treatment regimen or additional interventions.\nOverall: The learner should prioritize systematic history-taking and differential generation, then align investigations and a clear management plan based on the highest-probability diagnoses.",
    "createdAt": "2026-05-15T09:31:00Z"
  }
}
```

## ClinicalCaseService (Expert APIs)

Below are the expert-facing endpoints for managing clinical cases.

### API-1: List Clinical Cases (Paginated + Filtered)
GET /api/expert/clinical-cases
Query Params:
- page : number (default: 1)
- pageSize : number (default: 12)
- search : string (filter by title, caseId)
- status : string (active | draft | archived | published)
- type : string (APPENDICITIS | ABDOMINAL_PAIN | ...)
- eccid : string (evaluation criteria ID)
- sortBy : string (createdAt | updatedAt | title)
- sortDir : string (asc | desc)
Response:
```json
{
  "items": [
    {
      "caseId": "27892518",
      "title": "Acute Appendicitis Presentation",
      "description": "A patient came to the hospital...",
      "type": "APPENDICITIS",
      "status": "active",
      "eccid": "CRIT-001",
      "createdBy": "USR-EXP-001",
      "createdByName": "Dr. Andrew Nguyen",
      "createdAt": "2026-05-15T09:00:00Z",
      "updatedAt": "2026-05-15T09:12:00Z",
      "virtualPatientCount": 2,
      "attemptCount": 14,
      "avgScore": 84.5
    }
  ],
  "total": 42,
  "page": 1,
  "pageSize": 12,
  "totalPages": 4,
  "filters": {
    "availableStatuses": ["active", "draft", "archived", "published"],
    "availableTypes": ["APPENDICITIS", "ABDOMINAL_PAIN", "CHEST_PAIN"],
    "availableEccids": ["CRIT-001", "CRIT-002"]
  }
}
```

### FE Quick Contract: Search Cases For VP Create Modal
GET /api/expert/clinical-cases?search={q}&pageSize=10

Example request:
GET /api/expert/clinical-cases?search=appendicitis&pageSize=10

Example response:
```json
{
  "items": [
    {
      "caseId": "27892518",
      "title": "Acute Appendicitis Presentation",
      "type": "APPENDICITIS",
      "status": "active",
      "createdByName": "Dr. Andrew Nguyen"
    }
  ],
  "total": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```


### API-2: Get Case Detail
GET /api/expert/clinical-cases/{id}
Response:
```json
{
  "caseId": "27892518",
  "title": "Acute Appendicitis Presentation",
  "description": "...",
  "type": "APPENDICITIS",
  "status": "active",
  "pe": "Admission Vitals: Temp: 98 ...",
  "symptom": "Patient presents with...",
  "medicalhistory": "Past Medical History: Asthma...",
  "createdBy": "USR-EXP-001",
  "createdByName": "Dr. Andrew Nguyen",
  "eccid": "CRIT-001",
  "createdAt": "2026-05-15T09:00:00Z",
  "updatedAt": "2026-05-15T09:12:00Z",
  "labs": [
    {
      "id": 1,
      "itemId": 51301,
      "label": "White Blood Cells",
      "fluid": "Blood",
      "category": "Hematology",
      "value": "19.2 K/uL",
      "rangeLower": "4.0",
      "rangeUpper": "11.0"
    }
  ],
  "radiology": [
    {
      "id": 1,
      "noteId": "10070247-RR",
      "modality": "CT",
      "region": "Abdomen",
      "examName": "CT ABD & PELVIS WITH CONTRAST",
      "text": "Enlarged and fluid-filled appendix measuring up to 2.1 cm..."
    }
  ],
  "virtualPatients": [
    {
      "patientId": "10070247",
      "name": "Richard Anderson",
      "age": 43,
      "gender": "MALE",
      "level": "Intermediate",
      "status": "active"
    }
  ],
  "stats": {
    "totalAttempts": 14,
    "avgScore": 84.5,
    "completionRate": 0.78
  }
}
```

GET /api/expert/clinical-cases/{id}

### API-3: Create Clinical Case
POST /api/expert/clinical-cases
Request:
```json
{
  "title": "New Clinical Case",
  "description": "Case description",
  "type": "APPENDICITIS",
  "status": "draft",
  "pe": "Admission Vitals: ...",
  "symptom": "Patient presents with...",
  "medicalhistory": "Past Medical History: ...",
  "eccid": "CRIT-001"
}
```
Response:
```json
{
  "caseId": "28000001",
  "title": "New Clinical Case",
  "status": "draft",
  "createdAt": "2026-05-24T10:00:00Z"
}
```

### API-4: Update Clinical Case
PUT /api/expert/clinical-cases/{id}
Request: (same shape as POST, all fields required by current implementation)
```json
{
  "caseId": "27892518",
  "title": "Acute Appendicitis Presentation (Updated)",
  "description": "Updated case description",
  "type": "APPENDICITIS",
  "status": "active",
  "pe": "Admission Vitals: Temp: 38.1 ...",
  "symptom": "RLQ pain with nausea",
  "medicalhistory": "Past Medical History: Asthma...",
  "createdBy": "USR-EXP-001",
  "eccid": "CRIT-001"
}
```
Response:
```json
{
  "caseId": "27892518",
  "updatedAt": "2026-05-24T11:00:00Z"
}
```

### API-5: Update Case Status
PATCH /api/expert/clinical-cases/{id}/status
Request:
```json
{
  "status": "published"
}
```
Supported values: active | draft | archived | published
Response:
```json
{
  "caseId": "27892518",
  "status": "published",
  "updatedAt": "2026-05-24T11:00:00Z"
}
```

### API-6: Delete Clinical Case
DELETE /api/expert/clinical-cases/{id}
Response:
```json
{
  "success": true,
  "caseId": "27892518"
}
```

### API-7: Duplicate Clinical Case
POST /api/expert/clinical-cases/{id}/duplicate
Response:
```json
{
  "caseId": "28000099",
  "title": "Acute Appendicitis Presentation (Copy)",
  "status": "draft",
  "createdAt": "2026-05-24T11:30:00Z"
}
```

### API-8: Update Lab Test Value
PATCH /api/expert/clinical-cases/{id}/labs/{labId}
Request:
```json
{
  "value": "21.0 K/uL"
}
```
Response:
```json
{
  "id": 1,
  "value": "21.0 K/uL",
  "updatedAt": "2026-05-24T11:00:00Z"
}
```

### API-9: Update Radiology Text
PATCH /api/expert/clinical-cases/{id}/radiology/{radId}
Request:
```json
{
  "text": "Updated radiology finding text..."
}
```
Response:
```json
{
  "id": 1,
  "text": "Updated radiology finding text...",
  "updatedAt": "2026-05-24T11:00:00Z"
}
```


EpaScore response fields (types):
- epaId: string
- numericalScore: number
- maxScore: number
- entrustmentLevel: number
- feedbackDetail: string
- evidenceCited: array of string
- failurePatterns: array of string
- safetyFlags: array of string

### POST /api/evaluation/practice-feedback/{practiceSessionId}
Request:
Request fields (types):
- (empty body)
```json
{}
```
Response:
```json
{
  "message": "Feedback generated successfully.",
  "data": {
    "id": "FB-20260515-001",
    "overallAttempt": "Good effort",
    "overallLabel": "GOOD",
    "strength": "Focused abdominal pain history",
    "improvement": "Ask onset details earlier",
    "createdAt": "2026-05-15T09:31:00Z",
    "wasCached": false
  }
}
```

### GET /api/evaluation/practice-history?learnerId={learnerId}&patientId={patientId}
Response:
```json
{
  "learnerId": "USR-LRN-08",
  "patientId": "10070247",
  "items": [
    {
      "practiceSessionId": "SESS_20260515090000",
      "attemptNo": 1,
      "evaluationId": "EVAL-20260515-001",
      "score": 88.5,
      "pureEpaScore": 86,
      "entrustmentLevel": 4,
      "finalDiagnosis": "Acute appendicitis",
      "duration": 27,
      "diagnosisMatch": null,
      "rubricVersion": "v2",
      "createdAt": "2026-05-15T09:31:00Z",
      "status": "Completed",
      "feedbackId": "FB-20260515-001"
    },
    {
      "practiceSessionId": "SESS_20260420083000",

  Note: `feedbackId` is the `practice_feedback.id` returned by `POST /api/evaluation/practice-feedback/{practiceSessionId}`.
      "attemptNo": 2,
      "evaluationId": null,
      "score": null,
      "pureEpaScore": null,
      "entrustmentLevel": null,
      "finalDiagnosis": "",
      "duration": 18,
      "diagnosisMatch": null,
      "rubricVersion": null,
      "createdAt": "2026-04-20T08:48:00Z",
      "status": "Submitted",
      "feedbackId": null
    }
  ]
}
```

### DELETE /api/evaluation/{id}
Response: 204 No Content

### GET /api/evaluation/issues?practiceSessionId={sessionId}&learnerId={learnerId}
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

### POST /api/evaluation/issues
Request:
Request fields (types):
- practiceSessionId: string
- learnerId: string
- label: string
- description: string
- itemType: string (Practice|Assessment)
```json
{
  "practiceSessionId": "SESS_20260515090000",
  "learnerId": "USR-LRN-08",
  "label": "Clinical Logic",
  "description": "The dosage seems high.",
  "itemType": "Practice"
}
```


## VirtualPatientService

### GET /api/virtual-patients/discovery?learnerId={learnerId}&sortBy=newest&pageSize=200
Response:
```json
{
  "items": [
    {
      "patientId": "VP-10070247",
      "caseId": "CASE-APP-001",
      "name": "Mia Tran",
      "age": 22,
      "gender": "FEMALE",
      "occupation": "University student",
      "chiefConcern": "Right lower quadrant abdominal pain",
      "symptom": "Pain started around the umbilicus and migrated to the RLQ.",
      "level": "Intermediate",
      "avatarImage": "https://cdn.example.com/vp/mia-tran.png",
      "timeSetting": 18,
      "argumentTime": 20,
      "createdAt": "2026-05-15T09:00:00Z",
      "feedbackCount": 2,
      "attemptSummary": {
        "attempted": true,
        "attemptCount": 2,
        "maxAttempts": 3,
        "bestScore": 88.5,
        "latestScore": 84.0
      },
      "experts": [
        {
          "expertId": "EXP-001",
          "name": "Dr. Alexander Pierce",
          "role": "Emergency Medicine",
          "avatarUrl": "https://cdn.example.com/experts/alexander-pierce.png"
        }
      ]
    },
    {
      "patientId": "VP-10070248",
      "caseId": "CASE-RESP-004",
      "name": "Hao Nguyen",
      "age": 31,
      "gender": "MALE",
      "occupation": "Delivery driver",
      "chiefConcern": "Shortness of breath and cough",
      "symptom": "Wheezing for three days, worse at night.",
      "level": "Beginner",
      "avatarImage": "https://cdn.example.com/vp/hao-nguyen.png",
      "timeSetting": 15,
      "argumentTime": 18,
      "createdAt": "2026-05-14T07:30:00Z",
      "feedbackCount": 0,
      "attemptSummary": {
        "attempted": false,
        "attemptCount": 0,
        "maxAttempts": 3,
        "bestScore": null,
        "latestScore": null
      },
      "experts": []
    }
  ],
  "total": 14,
  "page": 1,
  "pageSize": 200,
  "filters": {
    "availableLevels": ["Beginner", "Intermediate"],
    "availableGenders": ["MALE", "FEMALE"],
    "availableSpecialties": [],
    "availableCaseTypes": []
  }
}
```

### POST /api/virtual-patients/discovery/fetch-cases
Request:
Request fields (types):
- learnerId: string
- level: string|null
- gender: string|null
- fetchCount: number
```json
{
  "learnerId": "USR-LRN-08",
  "level": "Intermediate",
  "gender": "MALE",
  "fetchCount": 5
}
```
Note: Each returned item in `fetchedItems` includes a `status` field (string). The server sets `status` to "published" for newly fetched cases.
Response:
```json
{
  "success": true,
  "message": "Successfully fetched 5 new virtual patient cases.",
  "data": {
    "learnerId": "USR-LRN-08",
    "fetchedCount": 5,
    "currentPoolTotal": 14,
    "fetchedItems": [
      {
        "patientId": "VP-10080111",
        "caseId": "CASE-REN-002",
        "name": "Bao Le",
        "level": "Intermediate",
        "status": "published"
      },
      {
        "patientId": "VP-10080122",
        "caseId": "CASE-CARD-006",
        "name": "Duc Pham",
        "level": "Intermediate",
        "status": "published"
      }
    ]
  }
}
```

Partial load:
```json
{
  "success": true,
  "message": "Only 3 new cases were available matching your criteria.",
  "data": {
    "learnerId": "USR-LRN-08",
    "fetchedCount": 3,
    "currentPoolTotal": 12,
    "fetchedItems": [
      {
        "patientId": "VP-10080201",
        "caseId": "CASE-GI-003",
        "name": "Linh Tran",
        "level": "Intermediate",
        "status": "published"
      }
    ]
  }
}
```

No more cases available:
```json
{
  "success": false,
  "errorCode": "NO_MORE_CASES_AVAILABLE",
  "message": "No new patient cases match your criteria. Try changing filters."
}
```

## VirtualPatientExpertService (Expert APIs)

Fake/example payloads for the expert-facing Virtual Patient management APIs (versioned under `api`).

### GET /api/expert/virtual-patients
Query params: `page`, `pageSize`, `search`, `sortBy`, `sortDir`, `status`, `level`, `gender`, `caseId`
Response (200):
```json
{
  "items": [
    {
      "patientId": "VP-abc123",
      "caseId": "CASE-001",
      "name": "John Doe",
      "age": 45,
      "gender": "male",
      "occupation": "Farmer",
      "chiefConcern": "Abdominal pain",
      "level": "intermediate",
      "status": "draft",
      "avatarImage": null,
      "timeSetting": 15,
      "argumentTime": 3,
      "createdAt": "2026-05-20T10:00:00Z",
      "updatedAt": "2026-05-20T10:00:00Z",
      "attemptCount": 12,
      "avgScore": 78.5,
      "expertCount": 2
    }
  ],
  "total": 1,
  "page": 1,
  "pageSize": 15,
  "totalPages": 1,
  "filters": {
    "availableStatuses": ["active","draft","archived","published"],
    "availableLevels": ["beginner","intermediate","advanced"],
    "availableGenders": ["male","female"],
    "availableCaseIds": ["CASE-001","CASE-002"]
  }
}
```

### GET /api/expert/virtual-patients/{id}
Response (200):
```json
{
  "patientId": "VP-abc123",
  "ownerExpertId": "EXP-01",
  "caseId": "CASE-001",
  "name": "John Doe",
  "age": 45,
  "gender": "male",
  "pronouns": "he/him",
  "ethnicity": null,
  "occupation": "Farmer",
  "chiefConcern": "Abdominal pain",
  "medicalHistory": "Background medical history text",
  "symptom": "Right lower quadrant pain",
  "persona": {"brief":"Cooperative, anxious"},
  "vitalSigns": {"bp":"120/80","hr":78},
  "instructions": null,
  "behaviors": null,
  "timeSetting": 15,
  "argumentTime": 3,
  "learningObjectives": {"objective1":"Collect focused history"},
  "level": "intermediate",
  "avatarImage": null,
  "caseRule": null,
  "status": "draft",
  "createdAt": "2026-05-20T10:00:00Z",
  "updatedAt": "2026-05-20T10:00:00Z",
  "experts": [
    {
      "expertId": "EXP-01",
      "name": "Dr. Alice",
      "role": "Consultant",
      "avatarUrl": null,
      "bioQuote": "General surgeon",
      "educationDetail": "MD, Surgery",
      "expertiseSkill": "General surgery",
      "phone": "+123456789",
      "email": "alice@example.com",
      "location": "Bangkok"
    }
  ],
  "stats": {
    "totalAttempts": 12,
    "avgScore": 78.5,
    "completionRate": 0.75
  }
}
```

### POST /api/expert/virtual-patients
Request (201 Created) example body:
```json
{
  "caseId": "CASE-001",
  "name": "New Patient",
  "age": 30,
  "gender": "female",
  "pronouns": "she/her",
  "occupation": "Teacher",
  "chiefConcern": "Headache",
  "medicalHistory": "No significant medical history",
  "symptom": "Intermittent headache",
  "persona": { "notes": "calm" },
  "vitalSigns": { "bp": "110/70", "hr": 72, "temp": 36.6, "spo2": "98%", "rr": 16 },
  "timeSetting": 10,
  "argumentTime": 2,
  "learningObjectives": ["Practice history taking"],
  "level": "beginner",
  "avatarImage": null,
  "caseRule": null,
  "expertIds": ["EXP-01"]
}
```
Response (201):
```json
{
  "patientId": "VP-unique-123",
  "ownerExpertId": "EXP-01",
  "name": "New Patient",
  "status": "draft",
  "createdAt": "2026-05-24T12:00:00Z",
  "expertIds": ["EXP-01"],
  "experts": [
    {
      "expertId": "EXP-01",
      "name": "Dr. Alice",
      "role": "Consultant",
      "avatarUrl": null
    }
  ],
  "stats": {
    "totalAttempts": 0,
    "avgScore": null,
    "completionRate": 0,
    "expertCount": 1
  }
}
```

Note: backend may include additional expert fields in each `experts` item (`bioQuote`, `educationDetail`, `expertiseSkill`, `phone`, `email`, `location`).

### PUT /api/expert/virtual-patients/{id}
Request body: same as POST, but `expertIds` is optional. If omitted, existing expert links are preserved.
Response (200):
```json
{ "patientId": "VP-abc123", "updatedAt": "2026-05-24T12:10:00Z" }
```

### PUT /api/expert/virtual-patients/{id}/experts
Replace the full expert list for a virtual patient.
Request body:
```json
{ "expertIds": ["EXP-01", "EXP-02"] }
```
Response (200):
```json
{
  "patientId": "VP-abc123",
  "expertIds": ["EXP-01", "EXP-02"],
  "updatedAt": "2026-05-24T12:10:00Z"
}
```

### POST /api/expert/virtual-patients/{id}/experts
Append experts to the existing list for a virtual patient.
Request body:
```json
{ "expertIds": ["EXP-03"] }
```
Response (200):
```json
{
  "patientId": "VP-abc123",
  "expertIds": ["EXP-01", "EXP-02", "EXP-03"],
  "updatedAt": "2026-05-24T12:10:00Z"
}
```

### DELETE /api/expert/virtual-patients/{id}/experts/{expertId}
Remove a single expert from the virtual patient.
Response (200):
```json
{
  "patientId": "VP-abc123",
  "expertIds": ["EXP-01", "EXP-02"],
  "updatedAt": "2026-05-24T12:10:00Z"
}
```

### PATCH /api/expert/virtual-patients/{id}/status
Request body:
```json
{ "status": "published" }
```
Response (200):
```json
{ "patientId": "VP-abc123", "status": "published", "updatedAt": "2026-05-24T12:11:00Z" }
```

### PATCH /api/expert/virtual-patients/{id}/publish
Request body:
```json
{ "publish": true }
```
Response (200):
```json
{ "patientId": "VP-abc123", "status": "published", "updatedAt": "2026-05-24T12:11:00Z" }
```

### DELETE /api/expert/virtual-patients/{id}
Query params:
- confirm: boolean (required, must be true)
Response (200):
```json
{ "success": true, "patientId": "VP-abc123" }
```

### POST /api/expert/virtual-patients/{id}
Delete alias for clients that cannot send `DELETE` with query confirmation.
Request: no body.
Response (200): same as `DELETE /api/expert/virtual-patients/{id}`.

### POST /api/expert/virtual-patients/{id}/delete
Delete alias for clients that cannot send `DELETE` with query confirmation.
Request: no body.
Response (200): same as `DELETE /api/expert/virtual-patients/{id}`.

### POST /api/expert/virtual-patients/{id}/duplicate
Response (201):
```json
{ "patientId": "VP-duplicate-456", "name": "John Doe", "status": "draft", "createdAt": "2026-05-24T12:12:00Z" }
```

---

Note: These are fake/example payloads for documentation and testing; adapt to real DTOs in `VirtualPatientExpertController` when implementing.
Response:
```json
{
  "message": "Issue created successfully.",
  "data": {
    "issueId": "ISS-002",
    "createdAt": "2026-05-15T09:45:00Z",
    "status": "Open"
  }
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

### GET /api/virtual-patients/discovery?learnerId={learnerId}&page=1&pageSize=9&sortBy=newest
Implemented `sortBy` values: `newest` (default), `oldest`, `level_asc`, `level_desc`. Other accepted values currently fall back to `newest`.
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
      "occupation": "Warehouse worker",
      "chiefConcern": "Abdominal pain",
      "symptom": "Right lower quadrant pain",
      "level": "Intermediate",
      "avatarImage": "/images/patients/richard-anderson.png",
      "timeSetting": 30,
      "argumentTime": 15,
      "createdAt": "2026-05-15T09:00:00Z",
      "feedbackCount": 2,
      "attemptSummary": {
        "attempted": true,
        "attemptCount": 2,
        "maxAttempts": 3,
        "bestScore": 88.5,
        "latestScore": 84.0
      },
      "experts": [
        {
          "expertId": "EXP-001",
          "name": "Dr. Andrew Nguyen",
          "role": "Specialist in Diagnostic Reasoning",
          "avatarUrl": "/images/d22.jpg"
        }
      ]
    }
  ],
  "total": 1,
  "page": 1,
  "pageSize": 9,
  "filters": {
    "availableLevels": ["Beginner", "Intermediate"],
    "availableGenders": ["FEMALE", "MALE"],
    "availableSpecialties": ["APPENDICITIS", "ABDOMINAL_PAIN"],
    "availableCaseTypes": ["APPENDICITIS", "ABDOMINAL_PAIN"]
  }
}
```

### POST /api/virtual-patients/discovery/fetch-cases
Request:
Request fields (types):
- learnerId: string
- level: string|null
- gender: string|null
- fetchCount: number
```json
{
  "learnerId": "USR-LRN-08",
  "level": "Intermediate",
  "gender": "MALE",
  "fetchCount": 5
}
```
Note: Each returned item in `fetchedItems` includes a `status` field (string). The server sets `status` to "active" for newly fetched cases.
Response:
```json
{
  "success": true,
  "message": "Successfully fetched 5 new virtual patient cases from the system database.",
  "data": {
    "learnerId": "USR-LRN-08",
    "fetchedCount": 5,
    "currentPoolTotal": 14,
    "fetchedItems": [
      { "patientId": "10070247", "caseId": "27892518", "name": "Richard Anderson", "level": "Intermediate", "status": "published" },
      { "patientId": "10070248", "caseId": "27892520", "name": "John Doe", "level": "Intermediate", "status": "published" },
      { "patientId": "10070249", "caseId": "27892521", "name": "Robert Smith", "level": "Intermediate", "status": "published" },
      { "patientId": "10070250", "caseId": "27892522", "name": "Michael Johnson", "level": "Intermediate", "status": "published" },
      { "patientId": "10070251", "caseId": "27892523", "name": "William David", "level": "Intermediate", "status": "published" }
    ]
  }
}
```
Error 400:
```json
{
  "success": false,
  "errorCode": "INVALID_FETCH_COUNT",
  "message": "The fetchCount parameter must be an integer between 1 and 20."
}
```
Error 404:
```json
{
  "success": false,
  "errorCode": "NO_MORE_CASES_AVAILABLE",
  "message": "No new patient cases match your criteria in the system database. Try changing the difficulty level or gender filters."
}
```
Error 401:
```json
{
  "success": false,
  "errorCode": "LEARNER_NOT_FOUND",
  "message": "The provided learnerId does not exist or session has expired."
}
```

### GET /api/virtual-patients/learner-last-discovery?learnerId={learnerId}
Response:
```json
{
  "learnerId": "USR-LRN-08",
  "filterJson": "{\"level\":\"Intermediate\",\"gender\":\"MALE\",\"sortBy\":\"newest\"}",
  "lastAccessed": "2026-05-18T10:20:00Z"
}
```

### POST /api/virtual-patients/learner-last-discovery
Request:
Request fields (types):
- learnerId: string
- filterJson: string|null
- lastAccessed: string|null (date-time)
```json
{
  "learnerId": "USR-LRN-08",
  "filterJson": "{\"level\":\"Intermediate\",\"gender\":\"MALE\",\"sortBy\":\"newest\"}",
  "lastAccessed": "2026-05-18T10:20:00Z"
}
```
Response:
```json
{
  "success": true,
  "learnerId": "USR-LRN-08",
  "lastAccessed": "2026-05-18T10:20:00Z"
}
```

## VirtualPatientService (VP AI)

### POST /chat
Request:
```json
{
  "doctor_id": "DR-001",
  "patient_id": "10070247",
  "question": "When did the pain start?",
  "chat_history": [
    { "role": "doctor", "content": "Hello, how are you feeling?" },
    { "role": "patient", "content": "I feel pain in my lower right abdomen." }
  ]
}
```
Response:
```json
{
  "answer": "About 12 hours ago."
}
```

### POST /stream
Request:
```json
{
  "doctor_id": "DR-001",
  "patient_id": "10070247",
  "question": "Do you have nausea?",
  "chat_history": [
    { "role": "doctor", "content": "Tell me about the pain." },
    { "role": "patient", "content": "It is sharp on the right side." }
  ]
}
```
Response (text/event-stream):
```text
data: {"type":"token","content":"Yes"}

data: {"type":"token","content":", since this morning."}

data: {"type":"done"}

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

## UserService

Gọi qua API Gateway: `/user/api/{everything}`. Controller nội bộ đang map ở `/api/users`.

### GET /api/users
Response:
```json
[
  {
    "userId": "USR-001",
    "name": "Dr. Andrew Nguyen",
    "email": "andrew.nguyen@latee.com",
    "phone": "+1-555-0101",
    "birthday": "1985-04-12T00:00:00Z",
    "password": null,
    "gender": "male",
    "address": "Hudson, Wisconsin",
    "role": "expert",
    "status": "active",
    "avatarUrl": "/images/users/andrew.png",
    "isDeleted": false,
    "createdAt": "2026-05-20T10:00:00Z",
    "updatedAt": "2026-05-24T12:10:00Z"
  }
]
```

### GET /api/users/{id}
Response:
```json
{
  "userId": "USR-001",
  "name": "Dr. Andrew Nguyen",
  "email": "andrew.nguyen@latee.com",
  "phone": "+1-555-0101",
  "birthday": "1985-04-12T00:00:00Z",
  "gender": "male",
  "address": "Hudson, Wisconsin",
  "status": "active",
  "role": "expert",
  "avatarUrl": "/images/users/andrew.png",
  "createdAt": "2026-05-20T10:00:00Z",
  "updatedAt": "2026-05-24T12:10:00Z",
  "profile": {
    "id": "EXP-001",
    "ssn": "123-45-6789",
    "bioQoute": "Leading expert in complex clinical case analysis...",
    "educationDetail": "MD Internal Medicine, Johns Hopkins University",
    "titlePosition": "Specialist in Diagnostic Reasoning",
    "expertiseSkill": "Clinical Reasoning, Diagnostic Strategy",
    "socialLink": "https://linkedin.com/in/andrew-nguyen"
  }
}
```

### GET /api/users/dashboard-stats
Response:
```json
{
  "increaseUser": 12,
  "totalLearners": 842,
  "increaseLearners": 8,
  "totalExperts": 24,
  "totalAdmins": 3,
  "totalActiveUsers": 615
}
```

### POST /api/users
Request:
```json
{
  "userId": "USR-002",
  "name": "Jane Doe",
  "email": "jane.doe@example.com",
  "password": "P@ssw0rd!",
  "phone": "+1-555-0102",
  "birthday": "1996-08-15T00:00:00Z",
  "gender": "female",
  "address": "Bangkok",
  "status": "active",
  "role": "learner",
  "avatarUrl": "/images/users/jane.png"
}
```
Response:
```json
{
  "userId": "USR-002",
  "name": "Jane Doe",
  "email": "jane.doe@example.com",
  "role": "learner",
  "status": "active",
  "createdAt": "2026-05-25T10:00:00Z",
  "updatedAt": "2026-05-25T10:00:00Z"
}
```

### PUT /api/users/{id}
Request: same shape as POST /api/users
Response: 204 No Content

### DELETE /api/users/{id}
Response: 204 No Content
