
```
lateeBE_visible
├─ api_v1.md
├─ API_v2.md
├─ docker
│  ├─ docker-compose copy.yml
│  ├─ docker-compose.yml
│  ├─ Dockerfile.AIAssistant
│  ├─ Dockerfile.APIGateway
│  ├─ Dockerfile.Assessment
│  ├─ Dockerfile.ClinicalCase
│  ├─ Dockerfile.Evaluation
│  ├─ Dockerfile.KnowledgeResource
│  ├─ Dockerfile.PracticeSession
│  ├─ Dockerfile.Roadmap
│  ├─ Dockerfile.UserService
│  ├─ Dockerfile.VirtualPatient
│  ├─ Dockerfile.VirtualPatient.Python
│  ├─ Llama-3.1-Virtual-Patient-MimicIV_Ver2.0
│  ├─ Llama-3.1-Virtual-Patient-MimicIV_Ver4.0
│  │  ├─ adapter_config.json
│  │  ├─ README.md
│  │  ├─ special_tokens_map.json
│  │  ├─ tokenizer.json
│  │  └─ tokenizer_config.json
│  ├─ mysql
│  │  ├─ Dockerfile
│  │  └─ init
│  │     ├─ 01_create_tables.sql
│  │     ├─ 02_insert_data.sql
│  │     ├─ 03_insert_additional_data.sql
│  │     ├─ 04_index_database.sql
│  │     ├─ 05_add_practice_session_status.sql
│  │     ├─ data-sam.txt
│  │     ├─ example__insert_data.txt
│  │     └─ patients.txt
│  ├─ ollama_vp_setup
│  │  └─ Modelfile.VP
│  └─ vp_model_setup
│     └─ Modelfile.VP
├─ exReadme.md
├─ lateeBE.sln
├─ Readme.md
├─ src
│  ├─ Gateway
│  │  ├─ ApiGateway
│  │  │  ├─ ApiGateway.csproj
│  │  │  ├─ ApiGateway.csproj.lscache
│  │  │  ├─ ApiGateway.http
│  │  │  ├─ appsettings.Development.json
│  │  │  ├─ appsettings.json
│  │  │  ├─ Auth
│  │  │  │  ├─ AuthController.cs
│  │  │  │  ├─ AuthDtos.cs
│  │  │  │  └─ AuthService.cs
│  │  │  ├─ ocelot.auth.json
│  │  │  ├─ ocelot.json
│  │  │  ├─ ocelot.noauth.json
│  │  │  ├─ Program.cs
│  │  │  └─ Properties
│  │  │     └─ launchSettings.json
│  │  └─ s
│  └─ Services
│     ├─ AIAssistantService
│     │  ├─ AIAssistantService.API
│     │  │  ├─ AIAssistantService.API.csproj
│     │  │  ├─ AIAssistantService.API.csproj.lscache
│     │  │  ├─ AIAssistantService.API.http
│     │  │  ├─ appsettings.Development.json
│     │  │  ├─ appsettings.json
│     │  │  ├─ Program.cs
│     │  │  └─ Properties
│     │  │     └─ launchSettings.json
│     │  ├─ AIAssistantService.Application
│     │  │  ├─ AIAssistantService.Application.csproj
│     │  │  └─ AIAssistantService.Application.csproj.lscache
│     │  ├─ AIAssistantService.Domain
│     │  │  ├─ AIAssistantService.Domain.csproj
│     │  │  ├─ AIAssistantService.Domain.csproj.lscache
│     │  │  └─ Class1.cs
│     │  ├─ AIAssistantService.Infrastructure
│     │  │  ├─ AIAssistantService.Infrastructure.csproj
│     │  │  ├─ AIAssistantService.Infrastructure.csproj.lscache
│     │  │  └─ Class1.cs
│     │  └─ python_api
│     │     ├─ app.py
│     │     ├─ assistantChat.py
│     │     ├─ config.py
│     │     ├─ config2.py
│     │     ├─ data
│     │     │  ├─ abdominal_pain_guideline.md
│     │     │  └─ process_guideline_v1.1.docx
│     │     ├─ dtos.py
│     │     ├─ ragLoader.py
│     │     ├─ ragLoaderVer2.py
│     │     ├─ reasoning.py
│     │     ├─ requirements.txt
│     │     └─ validateQuestion.py
│     ├─ AssessmentService
│     │  ├─ AssessmentService.API
│     │  │  ├─ appsettings.Development.json
│     │  │  ├─ appsettings.json
│     │  │  ├─ AssessmentService.API.csproj
│     │  │  ├─ AssessmentService.API.csproj.lscache
│     │  │  ├─ AssessmentService.API.http
│     │  │  ├─ Controller
│     │  │  │  └─ AssessmentController.cs
│     │  │  ├─ Program.cs
│     │  │  ├─ Properties
│     │  │  │  └─ launchSettings.json
│     │  │  └─ wwwroot
│     │  │     └─ uploads
│     │  │        └─ pdfs
│     │  │           └─ cardiologyFile.pdf
│     │  ├─ AssessmentService.Application
│     │  │  ├─ AssessmentService.Application.csproj
│     │  │  ├─ AssessmentService.Application.csproj.lscache
│     │  │  ├─ Class1.cs
│     │  │  ├─ Commands
│     │  │  │  ├─ CreateAssessment
│     │  │  │  │  └─ CreateAssessmentCommand.cs
│     │  │  │  ├─ CreateFullAssessment
│     │  │  │  │  └─ CreateFullAssessmentCommand.cs
│     │  │  │  ├─ DeleteAssessment
│     │  │  │  │  └─ DeleteAssessmentCommand.cs
│     │  │  │  ├─ GenerateQuestions
│     │  │  │  │  └─ GenerateAssessmentQuestionsCommand.cs
│     │  │  │  ├─ Questions
│     │  │  │  │  ├─ CreateQuestion
│     │  │  │  │  │  └─ CreateQuestionCommand.cs
│     │  │  │  │  ├─ DeleteQuestion
│     │  │  │  │  │  └─ DeleteQuestionCommand.cs
│     │  │  │  │  └─ UpdateQuestion
│     │  │  │  │     └─ UpdateQuestionCommand.cs
│     │  │  │  ├─ SubmitAssessment
│     │  │  │  │  ├─ SubmitAssessmentCommand.cs
│     │  │  │  │  └─ SubmitAssessmentHandler.cs
│     │  │  │  └─ UpdateAssessment
│     │  │  │     └─ UpdateAssessmentCommand.cs
│     │  │  ├─ DependencyInjection.cs
│     │  │  ├─ Dtos
│     │  │  │  ├─ GeneratedQuestionDto.cs
│     │  │  │  └─ PageResult.cs
│     │  │  └─ Queries
│     │  │     ├─ GetAllAssessments
│     │  │     │  └─ GetAllAssessmentsHandler.cs
│     │  │     ├─ GetAllAttempts
│     │  │     │  ├─ AssessmentAttemptOverview.cs
│     │  │     │  ├─ GetAllAttemptsDto.cs
│     │  │     │  └─ GetAllAttemptsHandler.cs
│     │  │     ├─ GetAssessmentById
│     │  │     │  ├─ AssessmentDetailDto.cs
│     │  │     │  └─ GetAssessmentByIdQuery.cs
│     │  │     ├─ GetAttemptDetail
│     │  │     │  ├─ GetAttemptDetailDto.cs
│     │  │     │  └─ GetAttemptDetailHandler.cs
│     │  │     └─ GetPagedAssessments
│     │  │        ├─ AssessmentSummaryDto.cs
│     │  │        └─ GetPagedAssessmentsQuery.cs
│     │  ├─ AssessmentService.Domain
│     │  │  ├─ AssessmentService.Domain.csproj
│     │  │  ├─ AssessmentService.Domain.csproj.lscache
│     │  │  ├─ Class1.cs
│     │  │  ├─ Entities
│     │  │  │  ├─ Assessment.cs
│     │  │  │  ├─ AssessmentAnswer.cs
│     │  │  │  ├─ AssessmentSession.cs
│     │  │  │  ├─ Question.cs
│     │  │  │  └─ Users.cs
│     │  │  └─ Repositories
│     │  │     ├─ IAssessmentRepository.cs
│     │  │     └─ IGeminiAiRepository.cs
│     │  ├─ AssessmentService.Infrastructure
│     │  │  ├─ AssessmentService.Infrastructure.csproj
│     │  │  ├─ AssessmentService.Infrastructure.csproj.lscache
│     │  │  ├─ Class1.cs
│     │  │  ├─ DependencyInjection.cs
│     │  │  ├─ Persistance
│     │  │  │  └─ AssessmentDbContext.cs
│     │  │  └─ Repositories
│     │  │     ├─ AssessmentRepository.cs
│     │  │     ├─ GeminiAiRepository copy.txt
│     │  │     └─ GeminiAiRepository.cs
│     │  └─ AssessmentService.sln
│     ├─ ClinicalCaseService
|     ├─ ├─ ....
│     ├─ EvaluationService
│     │  ├─ EvaluationService.API
│     │  │  ├─ appsettings.Development.json
│     │  │  ├─ appsettings.json
│     │  │  ├─ Controllers
│     │  │  │  └─ EvaluationController.cs
│     │  │  ├─ EvaluationService.API.csproj
│     │  │  ├─ EvaluationService.API.http
│     │  │  ├─ Program.cs
│     │  │  └─ Properties
│     │  │     └─ launchSettings.json
│     │  ├─ EvaluationService.API.Tests
│     │  │  ├─ EvaluationControllerTests.cs
│     │  │  └─ EvaluationService.API.Tests.csproj
│     │  ├─ EvaluationService.Application
│     │  │  ├─ Commands
│     │  │  │  ├─ CreateIssue
│     │  │  │  │  └─ CreateIssueCommand.cs
│     │  │  │  ├─ DeleteEvaluation
│     │  │  │  │  └─ DeleteEvaluationCommand.cs
│     │  │  │  ├─ GeneratePracticeFeedback
│     │  │  │  │  └─ GeneratePracticeFeedbackCommand.cs
│     │  │  │  └─ SubmitEvaluation
│     │  │  │     ├─ OUTPUTTEMPLATE.md
│     │  │  │     └─ SubmitEvaluationCommand.cs
│     │  │  ├─ DependencyInjection.cs
│     │  │  ├─ Dtos
│     │  │  │  ├─ ClinicalCaseDiagnosisDto.cs
│     │  │  │  ├─ EpaScoreDto.cs
│     │  │  │  ├─ EvaluationReportDto.cs
│     │  │  │  ├─ IssueListResponseDto.cs
│     │  │  │  ├─ PracticeFeedbackResponseDto.cs
│     │  │  │  ├─ SubmitEvaluationResultDto.cs
│     │  │  │  └─ WarningDto.cs
│     │  │  ├─ EvaluationService.Application.csproj
│     │  │  ├─ EvaluationService.Application.csproj.lscache
│     │  │  ├─ Orchestrators
│     │  │  │  └─ EvaluationOrchestrator.cs
│     │  │  ├─ Queries
│     │  │  │  ├─ GetHistory
│     │  │  │  │  └─ GetUserHistoryQuery.cs
│     │  │  │  ├─ GetIssues
│     │  │  │  │  └─ GetIssuesQuery.cs
│     │  │  │  └─ GetReport
│     │  │  │     └─ GetEvaluationReportQuery.cs
│     │  │  └─ Services
│     │  │     ├─ EpaScoreAggregator.cs
│     │  │     ├─ EvaluationPersistenceService.cs
│     │  │     ├─ FeedbackComposer.cs
│     │  │     ├─ IEvaluationPersistenceService.cs
│     │  │     └─ IFeedbackComposer.cs
│     │  ├─ EvaluationService.Domain
│     │  │  ├─ Entities
│     │  │  │  ├─ Evaluation.cs
│     │  │  │  ├─ EvaluationEpaScore.cs
│     │  │  │  ├─ Issue.cs
│     │  │  │  ├─ PracticeFeedback.cs
│     │  │  │  ├─ PracticeSession.cs
│     │  │  │  ├─ ResolvedIssue.cs
│     │  │  │  ├─ VirtualPatientRef.cs
│     │  │  │  └─ Warning.cs
│     │  │  ├─ EvaluationService.Domain.csproj
│     │  │  ├─ EvaluationService.Domain.csproj.lscache
│     │  │  ├─ Repositories
│     │  │  │  ├─ IAiEvaluationProvider.cs
│     │  │  │  └─ IEvaluationRepository.cs
│     │  │  ├─ Services
│     │  │  │  ├─ IEpaScoreAggregator.cs
│     │  │  │  ├─ IEvaluationPromptBuilder.cs
│     │  │  │  └─ IRubricProvider.cs
│     │  │  └─ ValueObjects
│     │  │     ├─ DiagnosisMatchResult.cs
│     │  │     ├─ RubricContext.cs
│     │  │     └─ ScoringModifiers.cs
│     │  ├─ EvaluationService.Infrastructure
│     │  │  ├─ DependencyInjection.cs
│     │  │  ├─ EvaluationService.Infrastructure.csproj
│     │  │  ├─ EvaluationService.Infrastructure.csproj.lscache
│     │  │  ├─ Persistence
│     │  │  │  └─ EvaluationDbContext.cs
│     │  │  ├─ Repositories
│     │  │  │  ├─ EvaluationRepository.cs
│     │  │  │  └─ GeminiEvaluationRepository.cs
│     │  │  └─ Rubrics
│     │  │     ├─ EpaRubrics
│     │  │     │  ├─ EPA-1-information-gathering.md
│     │  │     │  ├─ EPA-2-differential-diagnosis.md
│     │  │     │  ├─ EPA-3-clinical-reasoning.md
│     │  │     │  ├─ EPA-4-critical-thinking.md
│     │  │     │  └─ EPA-5-efficiency-professionalism.md
│     │  │     ├─ EvaluationPromptBuilder.cs
│     │  │     └─ RubricProvider.cs
│     │  └─ EvaluationService.sln
│     ├─ PracticeSessionService
│     │  ├─ PracticeSessionService.API
│     │  │  ├─ appsettings.Development.json
│     │  │  ├─ appsettings.json
│     │  │  ├─ Controllers
│     │  │  │  └─ PracticeSessionController.cs
│     │  │  ├─ PracticeSessionService.API.csproj
│     │  │  ├─ PracticeSessionService.API.csproj.lscache
│     │  │  ├─ PracticeSessionService.API.http
│     │  │  ├─ Program.cs
│     │  │  └─ Properties
│     │  │     └─ launchSettings.json
│     │  ├─ PracticeSessionService.Application
│     │  │  ├─ Commands
│     │  │  │  ├─ CreatePracticeSession
│     │  │  │  │  ├─ CreatePracticeSessionCommand.cs
│     │  │  │  │  ├─ CreatePracticeSessionHandler.cs
│     │  │  │  │  └─ CreatePracticeSessionResult.cs
│     │  │  │  └─ UpdatePracticeSessionStatus
│     │  │  │     ├─ UpdatePracticeSessionStatusCommand.cs
│     │  │  │     ├─ UpdatePracticeSessionStatusHandler.cs
│     │  │  │     ├─ UpdatePracticeSessionStatusRequest.cs
│     │  │  │     └─ UpdatePracticeSessionStatusResponse.cs
│     │  │  ├─ DependencyInjection.cs
│     │  │  ├─ Dtos
│     │  │  │  ├─ PageResult.cs
│     │  │  │  └─ WarningDTO.cs
│     │  │  ├─ PracticeSessionService.Application.csproj
│     │  │  ├─ PracticeSessionService.Application.csproj.lscache
│     │  │  └─ Queries
│     │  │     ├─ GetActivePracticeSession
│     │  │     │  ├─ GetActivePracticeSessionHandler.cs
│     │  │     │  ├─ GetActivePracticeSessionRequest.cs
│     │  │     │  └─ GetActivePracticeSessionResponse.cs
│     │  │     ├─ GetAttemptCount
│     │  │     │  ├─ GetAttemptCountHandler.cs
│     │  │     │  ├─ GetAttemptCountRequest.cs
│     │  │     │  └─ GetAttemptCountResponse.cs
│     │  │     ├─ GetClinicalCases
│     │  │     │  ├─ ClinicalCaseDto.cs
│     │  │     │  ├─ GetClinicalCasesHandler.cs
│     │  │     │  └─ GetClinicalCasesRequest.cs
│     │  │     ├─ GetPracticeSessions
│     │  │     │  ├─ GetPracticeSessionHandler.cs
│     │  │     │  ├─ GetPracticeSessionsRequest.cs
│     │  │     │  └─ GetPracticeSessionsResponse.cs
│     │  │     └─ SavePracticeSessions
│     │  │        ├─ SavePracticeSessionsHandler.cs
│     │  │        ├─ SavePracticeSessionsRequest.cs
│     │  │        └─ SavePracticeSessionsResponse.cs
│     │  ├─ PracticeSessionService.Domain
│     │  │  ├─ Entities
│     │  │  │  ├─ ClinicalCase.cs
│     │  │  │  ├─ Constants
│     │  │  │  │  ├─ PracticeSessionRules.cs
│     │  │  │  │  ├─ PracticeSessionStatuses.cs
│     │  │  │  │  └─ WarningLabels.cs
│     │  │  │  ├─ PracticeSession.cs
│     │  │  │  └─ Warning.cs
│     │  │  ├─ PracticeSessionService.Domain.csproj
│     │  │  ├─ PracticeSessionService.Domain.csproj.lscache
│     │  │  └─ Repositories
│     │  │     ├─ IClinicalCaseRepository.cs
│     │  │     └─ IPracticeSessionRepository.cs
│     │  ├─ PracticeSessionService.Infrastructure
│     │  │  ├─ Class1.cs
│     │  │  ├─ DependencyInjection.cs
│     │  │  ├─ Persistance
│     │  │  │  └─ PracticeSessionDbContext.cs
│     │  │  ├─ PracticeSessionService.Infrastructure.csproj
│     │  │  ├─ PracticeSessionService.Infrastructure.csproj.lscache
│     │  │  └─ Repositories
│     │  │     ├─ ClinicalCaseRepository.cs
│     │  │     └─ PracticeSessionRepository.cs
│     │  └─ PracticeSessionService.sln
│     └─ VirtualPatientService
│        ├─ VirtualPatientService.API
│        │  ├─ appsettings.Development.json
│        │  ├─ appsettings.json
│        │  ├─ Controllers
│        │  │  └─ VirtualPatientController.cs
│        │  ├─ Program.cs
│        │  ├─ Properties
│        │  │  └─ launchSettings.json
│        │  ├─ VirtualPatientService.API.csproj
│        │  ├─ VirtualPatientService.API.csproj.lscache
│        │  └─ VirtualPatientService.API.http
│        ├─ VirtualPatientService.Application
│        │  ├─ DependencyInjection.cs
│        │  ├─ Dtos
│        │  │  └─ PageResult.cs
│        │  ├─ Queries
│        │  │  ├─ GetVirtualPatientByID
│        │  │  │  ├─ GetVirtualPatientByIdHandler.cs
│        │  │  │  └─ GetVirtualPatientByIdQuery.cs
│        │  │  └─ GetVirtualPatients
│        │  │     ├─ GetVirtualPatientQuery.cs
│        │  │     ├─ GetVirtualPatientsHandler.cs
│        │  │     └─ VirtualPatientDto.cs
│        │  ├─ VirtualPatientService.Application.csproj
│        │  └─ VirtualPatientService.Application.csproj.lscache
│        ├─ VirtualPatientService.Domain
│        │  ├─ Entities
│        │  │  ├─ ClinicalCase.cs
│        │  │  └─ VirtualPatient.cs
│        │  ├─ Repositories
│        │  │  ├─ IClinicalCaseRepository.cs
│        │  │  └─ IVirtualPatientRepository.cs
│        │  ├─ VirtualPatientService.Domain.csproj
│        │  └─ VirtualPatientService.Domain.csproj.lscache
│        ├─ VirtualPatientService.Infrastructure
│        │  ├─ DependencyInjection.cs
│        │  ├─ Persistance
│        │  │  └─ VirtualPatientDbContext.cs
│        │  ├─ Repositories
│        │  │  ├─ ClinicalCaseRepository.cs
│        │  │  └─ VirtualPatientRepository.cs
│        │  ├─ VirtualPatientService.Infrastructure.csproj
│        │  └─ VirtualPatientService.Infrastructure.csproj.lscache
│        ├─ VirtualPatientService.sln
│        └─ vp_api
│           ├─ FE
│           │  ├─ .next
│           │  │  └─ dev
│           │  │     ├─ build
│           │  │     │  ├─ chunks
│           │  │     │  │  ├─ [turbopack]_runtime.js
│           │  │     │  │  └─ [turbopack]_runtime.js.map
│           │  │     │  ├─ package.json
│           │  │     │  ├─ postcss.js
│           │  │     │  └─ postcss.js.map
│           │  │     ├─ build-manifest.json
│           │  │     ├─ cache
│           │  │     │  ├─ .rscinfo
│           │  │     │  ├─ chrome-devtools-workspace-uuid
│           │  │     │  ├─ images
│           │  │     │  └─ next-devtools-config.json
│           │  │     ├─ fallback-build-manifest.json
│           │  │     ├─ package.json
│           │  │     ├─ prerender-manifest.json
│           │  │     ├─ routes-manifest.json
│           │  │     └─ trace
│           │  ├─ eslint.config.mjs
│           │  ├─ next-env.d.ts
│           │  ├─ next.config.ts
│           │  ├─ package-lock.json
│           │  ├─ package.json
│           │  ├─ postcss.config.mjs
│           │  ├─ PROJECT_STRUCTURE.md
│           │  ├─ README.md
│           │  ├─ StrctureFE.md
│           │  └─ tsconfig.json
│           ├─ hhs.txt
│           ├─ main.py
│           ├─ requirements copy.txt
│           ├─ requirements.txt
│           ├─ sys
│           │  ├─ app.py
│           │  ├─ ground_truths.json
│           │  ├─ prompt_template.json
│           │  ├─ system_prompts.json
│           │  └─ trainVP.py
│           └─ system_prompt.txt
└─ structureBE.md

```