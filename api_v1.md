# API V1

## PracticeSessionService

### POST /api/practice-sessions
Request:
```json
{
  "id": "SESS_1715050000000",
  "learnerId": "USR-LRN-01",
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
  "id": "SESS_1715050000000"
}
```

### POST /api/practice-sessions/submit
Request:
```json
{
  "sessionId": "SESS_1715050000000",
  "learnerId": "USR-LRN-01",
  "finalDiagnosis": "Appendicitis",
  "vpConversationLog": { "messages": [] },
  "aiReasoningLog": { "steps": [] },
  "moduleId": "EPA_STANDARD_V1",
  "discussionType": "Message Type",
  "guidelinesId": "GL-001",
  "warnings": [
    { "warningId": "W-001", "label": "Incomplete HPI", "description": "Missing onset details." }
  ]
}
```
Response:
```json
{
  "sessionId": "SESS_1715050000000"
}
```

### GET /api/practice-sessions/{id}
Response:
```json
{
  "sessionId": "SESS_1715050000000",
  "learnerId": "USR-LRN-01",
  "patientId": "10070247",
  "moduleId": "EPA_STANDARD_V1",
  "discussionType": "Message Type",
  "guidelinesId": "GL-001",
  "vpConversationLog": "{...}",
  "aiReasoningLog": "{...}",
  "finalDiagnosis": "Appendicitis",
  "status": "Completed",
  "startTime": "2026-05-01T09:00:00Z",
  "endTime": "2026-05-01T09:30:00Z",
  "createdAt": "2026-05-01T09:00:00Z",
  "warnings": [
    { "warningId": "W-001", "label": "Incomplete HPI", "description": "Missing onset details." }
  ]
}
```

### GET /api/practice-sessions/clinical-cases?status=active&page=1&pageSize=20
Response:
```json
{
  "items": [
    { "id": "27892518", "title": "Appendicitis", "type": "APPENDICITIS", "status": "active" }
  ],
  "total": 1,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

## EvaluationService

### POST /api/evaluation/submit
Request:
```json
{
  "practiceSessionId": "SESS_1715050000000",
  "learnerId": "USR-LRN-01",
  "epaId": "EPA-001",
  "score": 85.5,
  "duration": 30,
  "feedbackDetail": "Good clinical reasoning.",
  "entrustmentLevel": 4,
  "finalDiagnosis": "Appendicitis",
  "vpConversationLog": "{...}",
  "aiReasoningLog": "{...}",
  "discussionType": "Message Type",
  "moduleId": "EPA_STANDARD_V1",
  "warnings": [
    { "warningId": "W-001", "label": "Incomplete HPI", "description": "Missing onset details." }
  ]
}
```
Response:
```json
{
  "message": "Evaluation saved successfully.",
  "data": {
    "evaluationId": "e3f7b2c1d4a54f9bb8c4e9b7c1a2d3e4",
    "practiceSessionId": "SESS_1715050000000",
    "score": 85.5,
    "entrustmentLevel": 4,
    "feedbackDetail": "Good clinical reasoning.",
    "finalDiagnosis": "Appendicitis",
    "discussionType": "Message Type",
    "duration": 30
  }
}
```

### GET /api/evaluation/{userId}/history
Response:
```json
[
  { "evaluationId": "EVAL-001", "practiceSessionId": "SESS_1715050000000", "score": 85.5, "createdAt": "2026-05-01T09:31:00Z" }
]
```

### GET /api/evaluation/{id}/report
Response:
```json
{
  "evaluationId": "EVAL-001",
  "epaId": "EPA-001",
  "practiceSessionId": "SESS_1715050000000",
  "learnerId": "USR-LRN-01",
  "patientId": "10070247",
  "moduleId": "EPA_STANDARD_V1",
  "discussionType": "Message Type",
  "finalDiagnosis": "Appendicitis",
  "vpConversationLog": "{...}",
  "aiReasoningLog": "{...}",
  "score": 85.5,
  "duration": 30,
  "feedbackDetail": "Good clinical reasoning.",
  "entrustmentLevel": 4,
  "createdAt": "2026-05-01T09:31:00Z",
  "warnings": [
    { "warningId": "W-001", "label": "Incomplete HPI", "description": "Missing onset details." }
  ]
}
```

### DELETE /api/evaluation/{id}
Response: 204 No Content

## AssessmentService

### POST /api/assessments
Request:
```json
{
  "moduleId": "EPA_STANDARD_V1",
  "topic": "Appendicitis",
  "subtopic": "Acute Abdomen",
  "specialty": "General Surgery",
  "difficultyLevel": "Intermediate",
  "title": "Assessment on Appendicitis",
  "descriptions": "Test knowledge on appendicitis.",
  "goal": "Evaluate diagnosis and management.",
  "numQuestions": 10,
  "timeLimitMinutes": 30,
  "passingScorePercentage": 80.0,
  "maxAttempts": 3,
  "allowedQuestionTypes": "[\"MultipleChoice\",\"ShortAnswer\"]"
}
```
Response:
```json
{
  "message": "Assessment created successfully.",
  "assessmentId": "ASM-001"
}
```

### GET /api/assessments?specialty=General%20Surgery&difficultyLevel=Intermediate&page=1&pageSize=20
Response:
```json
{
  "items": [
    {
      "assessmentId": "ASM-001",
      "title": "Assessment on Appendicitis",
      "topic": "Appendicitis",
      "descriptions": "Test knowledge on appendicitis.",
      "difficultyLevel": "Intermediate",
      "numQuestions": 10,
      "isActive": true,
      "createdAt": "2026-05-01T09:00:00Z"
    }
  ],
  "total": 1,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

### GET /api/assessments/all
Response:
```json
[
  {
    "assessmentId": "ASM-001",
    "title": "Assessment on Appendicitis",
    "topic": "Appendicitis",
    "descriptions": "Test knowledge on appendicitis.",
    "difficultyLevel": "Intermediate",
    "numQuestions": 10,
    "isActive": true,
    "createdAt": "2026-05-01T09:00:00Z"
  }
]
```

### GET /api/assessments/{id}
Response:
```json
{
  "assessmentId": "ASM-001",
  "title": "Assessment on Appendicitis",
  "topic": "Appendicitis",
  "descriptions": "Test knowledge on appendicitis.",
  "difficultyLevel": "Intermediate",
  "numQuestions": 10,
  "isActive": true,
  "createdAt": "2026-05-01T09:00:00Z",
  "goal": "Evaluate diagnosis and management.",
  "specialty": "General Surgery",
  "timeLimitMinutes": 30,
  "questions": [
    {
      "id": "Q-001",
      "question": "What is the most common symptom of appendicitis?",
      "questionOption": { "options": ["Fever", "RLQ pain"] },
      "questionType": "MultipleChoice",
      "explanation": "RLQ pain is classic.",
      "points": 1.0
    }
  ]
}
```

### PUT /api/assessments/{id}
Request:
```json
{
  "assessmentId": "ASM-001",
  "title": "Updated title",
  "descriptions": "Updated descriptions",
  "goal": "Updated goal",
  "timeLimitMinutes": 40,
  "isActive": true
}
```
Response: 204 No Content

### DELETE /api/assessments/{id}
Response: 204 No Content

### POST /api/assessments/full-generation
Request:
```json
{
  "title": "Cardiology Assessment",
  "specialty": "Cardiology",
  "topic": "Arrhythmia",
  "difficultyLevel": "Intermediate",
  "goal": "Evaluate diagnosis and management.",
  "descriptions": "Module description here",
  "numQuestions": 5,
  "timeLimitMinutes": 30,
  "passingScorePercentage": 80.0,
  "maxAttempts": 3,
  "language": "English",
  "pdfFileName": "cardiologyFile.pdf"
}
```
Response:
```json
{
  "message": "Generating full assessment successful.",
  "data": {
    "assessmentId": "ASM-XYZ",
    "title": "Cardiology Assessment",
    "questions": [
      {
        "id": "Q-001",
        "assessmentId": "ASM-XYZ",
        "content": "Question text...",
        "questionOption": "[...]",
        "questionType": "MultipleChoice",
        "cognitiveLevel": "Apply",
        "explanation": "Explanation...",
        "points": 1.0,
        "createdAt": "2026-05-01T09:00:00Z",
        "updatedAt": "2026-05-01T09:00:00Z"
      }
    ]
  }
}
```

### POST /api/assessments/{id}/generate-questions
Request:
```json
{ "additionalPrompt": "Focus on ECG interpretation." }
```
Response:
```json
{ "message": "Successfully generated and saved questions to the database." }
```

### POST /api/assessments/{id}/questions
Request:
```json
{
  "assessmentId": "ASM-001",
  "questionType": "MultipleChoice",
  "cognitiveLevel": "Apply",
  "content": "Question text...",
  "options": [{ "id": "A", "content": "Option A", "isCorrect": true }],
  "explanation": "Explanation...",
  "points": 1.0
}
```
Response:
```json
{ "message": "Generated question successfully.", "questionId": "Q-123" }
```

### PUT /api/assessments/questions/{questionId}
Request:
```json
{
  "questionId": "Q-123",
  "questionType": "MultipleChoice",
  "cognitiveLevel": "Apply",
  "content": "Updated content...",
  "options": [{ "id": "A", "content": "Option A", "isCorrect": true }],
  "explanation": "Updated explanation...",
  "points": 1.0
}
```
Response: 204 No Content

### DELETE /api/assessments/questions/{questionId}
Response: 204 No Content

### POST /api/assessments/api/attempts/submit
Request:
```json
{
  "assessmentId": "ASM-001",
  "userId": "USR-LRN-01",
  "durationSeconds": 1200,
  "answers": [
    { "questionId": "Q-001", "selectedOptionId": "A" }
  ]
}
```
Response:
```json
{
  "message": "Submit assessment successful.",
  "data": {
    "attemptId": "ASMT-SES-001",
    "score": 85.5,
    "isPassed": true,
    "correctCount": 8
  }
}
```

### GET /api/assessments/attempts/{attemptId}
Response:
```json
{
  "data": {
    "attemptId": "ASMT-SES-001",
    "score": 85.5,
    "isPassed": true,
    "correctCount": 8,
    "questions": [
      {
        "questionId": "Q-001",
        "content": "Question text...",
        "userAnswerId": "A",
        "correctAnswerId": "A",
        "isCorrect": true,
        "explanation": "Explanation...",
        "options": [
          { "id": "A", "content": "Option A", "isCorrect": true }
        ]
      }
    ]
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
      "occupation": "Worker",
      "chiefConcern": "Abdominal pain",
      "medicalHistory": "History text...",
      "symptom": "RLQ pain",
      "pronouns": "he/him",
      "ethnicity": "Hispanic",
      "persona": { "emotional_state": "Neutral" },
      "vitalSigns": { "bp": "114/91", "hr": 79 },
      "instructions": { "role": "Medical Learner" },
      "behaviors": ["Low pain tolerance"],
      "timeSetting": 30,
      "argumentTime": 15,
      "learningObjectives": ["..."],
      "level": "Intermediate",
      "avatarImage": "/images/...",
      "caseRule": { "rules": ["HPI"] },
      "status": "active",
      "createdAt": "2026-05-01T09:00:00Z",
      "updatedAt": "2026-05-01T09:00:00Z"
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
  "occupation": "Worker",
  "chiefConcern": "Abdominal pain",
  "medicalHistory": "History text...",
  "symptom": "RLQ pain",
  "pronouns": "he/him",
  "ethnicity": "Hispanic",
  "persona": { "emotional_state": "Neutral" },
  "vitalSigns": { "bp": "114/91", "hr": 79 },
  "instructions": { "role": "Medical Learner" },
  "behaviors": ["Low pain tolerance"],
  "timeSetting": 30,
  "argumentTime": 15,
  "learningObjectives": ["..."],
  "level": "Intermediate",
  "avatarImage": "/images/...",
  "caseRule": { "rules": ["HPI"] },
  "status": "active",
  "createdAt": "2026-05-01T09:00:00Z",
  "updatedAt": "2026-05-01T09:00:00Z"
}
```
