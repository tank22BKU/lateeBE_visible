

```
lateeBE_visible
├─ api_v1.md
├─ docker
│  ├─ docker-compose copy.yml
│  ├─ docker-compose.yml
│  ├─ Dockerfile.AIAssistant
│  ├─ Dockerfile.APIGateway
│  ├─ Dockerfile.Assessment
│  ├─ Dockerfile.ClinicalCase
│  ├─ Dockerfile.Evaluation
│  ├─ Dockerfile.PracticeSession
│  ├─ Dockerfile.Roadmap
│  ├─ Dockerfile.VirtualPatient
│  ├─ Dockerfile.VirtualPatient.Python
│  ├─ Llama-3.1-Virtual-Patient-MimicIV_Ver2.0
│  ├─ Llama-3.1-Virtual-Patient-MimicIV_Ver3.0
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
│  │     ├─ data-sam.txt
│  │     └─ patients.txt
│  └─ vp_model_setup
│     └─ Modelfile.VP
├─ exReadme.md
├─ lateeBE.sln
├─ Readme.md
└─ src
   ├─ Gateway
   │  ├─ ApiGateway
   │  │  ├─ ApiGateway.csproj
   │  │  ├─ ApiGateway.csproj.lscache
   │  │  ├─ ApiGateway.http
   │  │  ├─ appsettings.Development.json
   │  │  ├─ appsettings.json
   │  │  ├─ Auth
   │  │  │  ├─ AuthController.cs
   │  │  │  ├─ AuthDtos.cs
   │  │  │  └─ AuthService.cs
   │  │  ├─ ocelot.auth.json
   │  │  ├─ ocelot.json
   │  │  ├─ ocelot.noauth.json
   │  │  ├─ Program.cs
   │  │  └─ Properties
   │  │     └─ launchSettings.json
   │  └─ s
   └─ Services
      ├─ AIAssistantService
      │  ├─ AIAssistantService.API
      │  │  ├─ AIAssistantService.API.csproj
      │  │  ├─ AIAssistantService.API.csproj.lscache
      │  │  ├─ AIAssistantService.API.http
      │  │  ├─ appsettings.Development.json
      │  │  ├─ appsettings.json
      │  │  ├─ Program.cs
      │  │  └─ Properties
      │  │     └─ launchSettings.json
      │  ├─ AIAssistantService.Application
      │  │  ├─ AIAssistantService.Application.csproj
      │  │  └─ AIAssistantService.Application.csproj.lscache
      │  ├─ AIAssistantService.Domain
      │  │  ├─ AIAssistantService.Domain.csproj
      │  │  ├─ AIAssistantService.Domain.csproj.lscache
      │  │  └─ Class1.cs
      │  ├─ AIAssistantService.Infrastructure
      │  │  ├─ AIAssistantService.Infrastructure.csproj
      │  │  ├─ AIAssistantService.Infrastructure.csproj.lscache
      │  │  └─ Class1.cs
      │  └─ python_api
      │     ├─ app.py
      │     ├─ assistantChat.py
      │     ├─ config.py
      │     ├─ data
      │     │  ├─ abdominal_pain_guideline.md
      │     │  └─ process_guideline_v1.1.docx
      │     ├─ dtos.py
      │     ├─ ragLoader.py
      │     ├─ ragLoaderVer2.py
      │     ├─ reasoning.py
      │     ├─ requirements.txt
      │     └─ validateQuestion.py
      ├─ AssessmentService
      │  ├─ AssessmentService.API
      │  │  ├─ appsettings.Development.json
      │  │  ├─ appsettings.json
      │  │  ├─ AssessmentService.API.csproj
      │  │  ├─ AssessmentService.API.csproj.lscache
      │  │  ├─ AssessmentService.API.http
      │  │  ├─ Controller
      │  │  │  └─ AssessmentController.cs
      │  │  ├─ Program.cs
      │  │  ├─ Properties
      │  │  │  └─ launchSettings.json
      │  │  └─ wwwroot
      │  │     └─ uploads
      │  │        └─ pdfs
      │  │           └─ cardiologyFile.pdf
      │  ├─ AssessmentService.Application
      │  │  ├─ AssessmentService.Application.csproj
      │  │  ├─ AssessmentService.Application.csproj.lscache
      │  │  ├─ Class1.cs
      │  │  ├─ Commands
      │  │  │  ├─ CreateAssessment
      │  │  │  │  └─ CreateAssessmentCommand.cs
      │  │  │  ├─ CreateFullAssessment
      │  │  │  │  └─ CreateFullAssessmentCommand.cs
      │  │  │  ├─ DeleteAssessment
      │  │  │  │  └─ DeleteAssessmentCommand.cs
      │  │  │  ├─ GenerateQuestions
      │  │  │  │  └─ GenerateAssessmentQuestionsCommand.cs
      │  │  │  ├─ Questions
      │  │  │  │  ├─ CreateQuestion
      │  │  │  │  │  └─ CreateQuestionCommand.cs
      │  │  │  │  ├─ DeleteQuestion
      │  │  │  │  │  └─ DeleteQuestionCommand.cs
      │  │  │  │  └─ UpdateQuestion
      │  │  │  │     └─ UpdateQuestionCommand.cs
      │  │  │  ├─ SubmitAssessment
      │  │  │  │  ├─ SubmitAssessmentCommand.cs
      │  │  │  │  └─ SubmitAssessmentHandler.cs
      │  │  │  └─ UpdateAssessment
      │  │  │     └─ UpdateAssessmentCommand.cs
      │  │  ├─ DependencyInjection.cs
      │  │  ├─ Dtos
      │  │  │  ├─ GeneratedQuestionDto.cs
      │  │  │  └─ PageResult.cs
      │  │  └─ Queries
      │  │     ├─ GetAllAssessments
      │  │     │  └─ GetAllAssessmentsHandler.cs
      │  │     ├─ GetAssessmentById
      │  │     │  ├─ AssessmentDetailDto.cs
      │  │     │  └─ GetAssessmentByIdQuery.cs
      │  │     ├─ GetAttemptDetail
      │  │     │  ├─ GetAttemptDetailDto.cs
      │  │     │  └─ GetAttemptDetailHandler.cs
      │  │     └─ GetPagedAssessments
      │  │        ├─ AssessmentSummaryDto.cs
      │  │        └─ GetPagedAssessmentsQuery.cs
      │  ├─ AssessmentService.Domain
      │  │  ├─ AssessmentService.Domain.csproj
      │  │  ├─ AssessmentService.Domain.csproj.lscache
      │  │  ├─ Class1.cs
      │  │  ├─ Entities
      │  │  │  ├─ Assessment.cs
      │  │  │  ├─ AssessmentAnswer.cs
      │  │  │  ├─ AssessmentSession.cs
      │  │  │  ├─ Question.cs
      │  │  │  └─ Users.cs
      │  │  └─ Repositories
      │  │     ├─ IAssessmentRepository.cs
      │  │     └─ IGeminiAiRepository.cs
      │  ├─ AssessmentService.Infrastructure
      │  │  ├─ AssessmentService.Infrastructure.csproj
      │  │  ├─ AssessmentService.Infrastructure.csproj.lscache
      │  │  ├─ Class1.cs
      │  │  ├─ DependencyInjection.cs
      │  │  ├─ Persistance
      │  │  │  └─ AssessmentDbContext.cs
      │  │  └─ Repositories
      │  │     ├─ AssessmentRepository.cs
      │  │     ├─ GeminiAiRepository copy.txt
      │  │     └─ GeminiAiRepository.cs
      │  └─ AssessmentService.sln
      ├─ ClinicalCaseService
      │  ├─ ClinicalCaseService.API
      │  │  ├─ appsettings.Development.json
      │  │  ├─ appsettings.json
      │  │  ├─ ClinicalCaseService.API.csproj
      │  │  ├─ ClinicalCaseService.API.csproj.lscache
      │  │  ├─ ClinicalCaseService.API.http
      │  │  ├─ Controllers
      │  │  │  └─ ClinicalCaseController.cs
      │  │  ├─ Program.cs
      │  │  └─ Properties
      │  │     └─ launchSettings.json
      │  ├─ ClinicalCaseService.Application
      │  │  ├─ Class1.cs
      │  │  ├─ ClinicalCaseService.Application.csproj
      │  │  ├─ ClinicalCaseService.Application.csproj.lscache
      │  │  ├─ Commands
      │  │  │  ├─ CreateClinicalCase
      │  │  │  │  └─ CreateClinicalCaseCommand.cs
      │  │  │  ├─ DeleteClinicalCase
      │  │  │  │  └─ DeleteClinicalCaseCommand.cs
      │  │  │  └─ UpdateClinicalCase
      │  │  │     └─ UpdateClinicalCaseCommand.cs
      │  │  ├─ DependencyInjection.cs
      │  │  ├─ Dtos
      │  │  │  └─ PageResult.cs
      │  │  └─ Queries
      │  │     ├─ GetClinicalCaseById
      │  │     │  ├─ GetClinicalCaseByIdHandler.cs
      │  │     │  └─ GetClinicalCaseByIdQuery.cs
      │  │     └─ GetClinicalCases
      │  │        ├─ ClinicalCaseDto.cs
      │  │        ├─ GetClinicalCasesHandler.cs
      │  │        └─ GetClinicalCasesQuery.cs
      │  ├─ ClinicalCaseService.Domain
      │  │  ├─ Class1.cs
      │  │  ├─ ClinicalCaseService.Domain.csproj
      │  │  ├─ ClinicalCaseService.Domain.csproj.lscache
      │  │  ├─ Entities
      │  │  │  ├─ ClinicalCase.cs
      │  │  │  └─ VirtualPatient.cs
      │  │  └─ Repositories
      │  │     └─ IClinicalCaseRepository.cs
      │  ├─ ClinicalCaseService.Infrastructure
      │  │  ├─ Class1.cs
      │  │  ├─ ClinicalCaseService.Infrastructure.csproj
      │  │  ├─ ClinicalCaseService.Infrastructure.csproj.lscache
      │  │  ├─ DependencyInjection.cs
      │  │  ├─ Persistance
      │  │  │  └─ ClinicalCaseDbContext.cs
      │  │  └─ Repositories
      │  │     └─ ClinicalCaseRepository.cs
      │  └─ ClinicalCaseService.sln
      ├─ EvaluationService
      │  ├─ EvaluationService.API
      │  │  ├─ appsettings.Development.json
      │  │  ├─ appsettings.json
      │  │  ├─ Controllers
      │  │  │  └─ EvaluationController.cs
      │  │  ├─ EvaluationService.API.csproj
      │  │  ├─ EvaluationService.API.csproj.lscache
      │  │  ├─ EvaluationService.API.http
      │  │  ├─ Program.cs
      │  │  └─ Properties
      │  │     └─ launchSettings.json
      │  ├─ EvaluationService.Application
      │  │  ├─ Commands
      │  │  │  ├─ DeleteEvaluation
      │  │  │  │  └─ DeleteEvaluationCommand.cs
      │  │  │  └─ SubmitEvaluation
      │  │  │     └─ SubmitEvaluationCommand.cs
      │  │  ├─ DependencyInjection.cs
      │  │  ├─ Dtos
      │  │  │  └─ WarningDto.cs
      │  │  ├─ EvaluationService.Application.csproj
      │  │  ├─ EvaluationService.Application.csproj.lscache
      │  │  └─ Queries
      │  │     ├─ GetHistory
      │  │     │  └─ GetUserHistoryQuery.cs
      │  │     └─ GetReport
      │  │        └─ GetEvaluationReportQuery.cs
      │  ├─ EvaluationService.Domain
      │  │  ├─ Entities
      │  │  │  ├─ Evaluation.cs
      │  │  │  ├─ PracticeSession.cs
      │  │  │  └─ Warning.cs
      │  │  ├─ EvaluationService.Domain.csproj
      │  │  ├─ EvaluationService.Domain.csproj.lscache
      │  │  └─ Repositories
      │  │     └─ IEvaluationRepository.cs
      │  ├─ EvaluationService.Infrastructure
      │  │  ├─ DependencyInjection.cs
      │  │  ├─ EvaluationService.Infrastructure.csproj
      │  │  ├─ EvaluationService.Infrastructure.csproj.lscache
      │  │  ├─ Persistence
      │  │  │  └─ EvaluationDbContext.cs
      │  │  └─ Repositories
      │  │     └─ EvaluationRepository.cs
      │  └─ EvaluationService.sln
      ├─ PracticeSessionService
      │  ├─ PracticeSessionService.API
      │  │  ├─ appsettings.Development.json
      │  │  ├─ appsettings.json
      │  │  ├─ Controllers
      │  │  │  └─ PracticeSessionController.cs
      │  │  ├─ PracticeSessionService.API.csproj
      │  │  ├─ PracticeSessionService.API.csproj.lscache
      │  │  ├─ PracticeSessionService.API.http
      │  │  ├─ Program.cs
      │  │  └─ Properties
      │  │     └─ launchSettings.json
      │  ├─ PracticeSessionService.Application
      │  │  ├─ Commands
      │  │  │  └─ CreatePracticeSession
      │  │  │     ├─ CreatePracticeSessionCommand.cs
      │  │  │     ├─ CreatePracticeSessionHandler.cs
      │  │  │     └─ CreatePracticeSessionResult.cs
      │  │  ├─ DependencyInjection.cs
      │  │  ├─ Dtos
      │  │  │  ├─ PageResult.cs
      │  │  │  └─ WarningDTO.cs
      │  │  ├─ PracticeSessionService.Application.csproj
      │  │  ├─ PracticeSessionService.Application.csproj.lscache
      │  │  └─ Queries
      │  │     ├─ GetClinicalCases
      │  │     │  ├─ ClinicalCaseDto.cs
      │  │     │  ├─ GetClinicalCasesHandler.cs
      │  │     │  └─ GetClinicalCasesRequest.cs
      │  │     ├─ GetPracticeSessions
      │  │     │  ├─ GetPracticeSessionHandler.cs
      │  │     │  ├─ GetPracticeSessionsRequest.cs
      │  │     │  └─ GetPracticeSessionsResponse.cs
      │  │     └─ SavePracticeSessions
      │  │        ├─ SavePracticeSessionsHandler.cs
      │  │        ├─ SavePracticeSessionsRequest.cs
      │  │        └─ SavePracticeSessionsResponse.cs
      │  ├─ PracticeSessionService.Domain
      │  │  ├─ Entities
      │  │  │  ├─ ClinicalCase.cs
      │  │  │  ├─ PracticeSession.cs
      │  │  │  └─ Warning.cs
      │  │  ├─ PracticeSessionService.Domain.csproj
      │  │  ├─ PracticeSessionService.Domain.csproj.lscache
      │  │  └─ Repositories
      │  │     ├─ IClinicalCaseRepository.cs
      │  │     └─ IPracticeSessionRepository.cs
      │  ├─ PracticeSessionService.Infrastructure
      │  │  ├─ Class1.cs
      │  │  ├─ DependencyInjection.cs
      │  │  ├─ Persistance
      │  │  │  └─ PracticeSessionDbContext.cs
      │  │  ├─ PracticeSessionService.Infrastructure.csproj
      │  │  ├─ PracticeSessionService.Infrastructure.csproj.lscache
      │  │  └─ Repositories
      │  │     ├─ ClinicalCaseRepository.cs
      │  │     └─ PracticeSessionRepository.cs
      │  └─ PracticeSessionService.sln
      ├─ RoadmapService
      │  ├─ RoadmapService.API
      │  │  ├─ appsettings.Development.json
      │  │  ├─ appsettings.json
      │  │  ├─ Controllers
      │  │  │  └─ RoadmapController.cs
      │  │  ├─ Program.cs
      │  │  ├─ Properties
      │  │  │  └─ launchSettings.json
      │  │  ├─ RoadmapService.API.csproj
      │  │  ├─ RoadmapService.API.csproj.lscache
      │  │  └─ RoadmapService.API.http
      │  ├─ RoadmapService.Application
      │  │  ├─ Class1.cs
      │  │  ├─ DependencyInjection.cs
      │  │  ├─ Dtos
      │  │  │  └─ PageResult.cs
      │  │  ├─ Queries
      │  │  │  ├─ GenerateRoadmap
      │  │  │  │  ├─ GenerateRoadmapHandler.cs
      │  │  │  │  ├─ GenerateRoadmapRequest.cs
      │  │  │  │  └─ GenerateRoadmapResponse.cs
      │  │  │  └─ GetClinicalCases
      │  │  │     ├─ ClinicalCaseDto.cs
      │  │  │     ├─ GetClinicalCasesHandler.cs
      │  │  │     └─ GetClinicalCasesQuery.cs
      │  │  ├─ RoadmapService.Application.csproj
      │  │  └─ RoadmapService.Application.csproj.lscache
      │  ├─ RoadmapService.Domain
      │  │  ├─ Class1.cs
      │  │  ├─ Entities
      │  │  │  ├─ ClinicalCase.cs
      │  │  │  └─ VirtualPatient.cs
      │  │  ├─ Repositories
      │  │  │  └─ IClinicalCaseRepository.cs
      │  │  ├─ RoadmapService.Domain.csproj
      │  │  ├─ RoadmapService.Domain.csproj.lscache
      │  │  └─ Services
      │  │     └─ IGeminiService.cs
      │  ├─ RoadmapService.Infrastructure
      │  │  ├─ Class1.cs
      │  │  ├─ DependencyInjection.cs
      │  │  ├─ Persistance
      │  │  │  └─ ClinicalCaseDbContext.cs
      │  │  ├─ Repositories
      │  │  │  └─ ClinicalCaseRepository.cs
      │  │  ├─ RoadmapService.Infrastructure.csproj
      │  │  ├─ RoadmapService.Infrastructure.csproj.lscache
      │  │  └─ Services
      │  │     ├─ GeminiService.cs
      │  │     └─ Prompts.cs
      │  └─ RoadmapService.sln
      └─ VirtualPatientService
         ├─ VirtualPatientService.API
         │  ├─ appsettings.Development.json
         │  ├─ appsettings.json
         │  ├─ Controllers
         │  │  └─ VirtualPatientController.cs
         │  ├─ Program.cs
         │  ├─ Properties
         │  │  └─ launchSettings.json
         │  ├─ VirtualPatientService.API.csproj
         │  ├─ VirtualPatientService.API.csproj.lscache
         │  └─ VirtualPatientService.API.http
         ├─ VirtualPatientService.Application
         │  ├─ DependencyInjection.cs
         │  ├─ Dtos
         │  │  └─ PageResult.cs
         │  ├─ Queries
         │  │  ├─ GetVirtualPatientByID
         │  │  │  ├─ GetVirtualPatientByIdHandler.cs
         │  │  │  └─ GetVirtualPatientByIdQuery.cs
         │  │  └─ GetVirtualPatients
         │  │     ├─ GetVirtualPatientQuery.cs
         │  │     ├─ GetVirtualPatientsHandler.cs
         │  │     └─ VirtualPatientDto.cs
         │  ├─ VirtualPatientService.Application.csproj
         │  └─ VirtualPatientService.Application.csproj.lscache
         ├─ VirtualPatientService.Domain
         │  ├─ Entities
         │  │  ├─ ClinicalCase.cs
         │  │  └─ VirtualPatient.cs
         │  ├─ Repositories
         │  │  ├─ IClinicalCaseRepository.cs
         │  │  └─ IVirtualPatientRepository.cs
         │  ├─ VirtualPatientService.Domain.csproj
         │  └─ VirtualPatientService.Domain.csproj.lscache
         ├─ VirtualPatientService.Infrastructure
         │  ├─ DependencyInjection.cs
         │  ├─ Persistance
         │  │  └─ VirtualPatientDbContext.cs
         │  ├─ Repositories
         │  │  ├─ ClinicalCaseRepository.cs
         │  │  └─ VirtualPatientRepository.cs
         │  ├─ VirtualPatientService.Infrastructure.csproj
         │  └─ VirtualPatientService.Infrastructure.csproj.lscache
         ├─ VirtualPatientService.sln
         └─ vp_api
            ├─ hhs.txt
            ├─ main.py
            ├─ requirements copy.txt
            ├─ requirements.txt
            ├─ sys
            │  ├─ app.py
            │  ├─ ground_truths.json
            │  ├─ prompt_template.json
            │  ├─ system_prompts.json
            │  └─ trainVP.py
            └─ system_prompt.txt

```