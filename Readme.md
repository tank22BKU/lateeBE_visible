### How to configure
- fill HUGGINGFACE_TOKEN in .env
- fill RoadmapService.API/appsettings.json with Google Gemini API key
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Gemini": {
    "ApiKey": "Google Gemini API key here",
    "Model": "gemini-2.5-flash"
  }
}

### How to create new service

### Create 4 layer
mkdir -p src/Services/EvaluationService
cd src/Services/EvaluationService
mkdir -p src/Services/EvaluationService
cd src/Services/EvaluationService

dotnet new webapi -n EvaluationService.API --framework net9.0
dotnet new classlib -n EvaluationService.Domain --framework net9.0
dotnet new classlib -n EvaluationService.Application --framework net9.0
dotnet new classlib -n EvaluationService.Infrastructure --framework net9.0
dotnet new webapi -n EvaluationService.API --framework net9.0
dotnet new classlib -n EvaluationService.Domain --framework net9.0
dotnet new classlib -n EvaluationService.Application --framework net9.0
dotnet new classlib -n EvaluationService.Infrastructure --framework net9.0

cd ../../../
dotnet sln add src\Services\EvaluationService\EvaluationService.API\EvaluationService.API.csproj
dotnet sln add src\Services\EvaluationService\EvaluationService.Domain\EvaluationService.Domain.csproj
dotnet sln add src\Services\EvaluationService\EvaluationService.Application\EvaluationService.Application.csproj
dotnet sln add src\Services\EvaluationService\EvaluationService.Infrastructure\EvaluationService.Infrastructure.csproj
dotnet sln add src\Services\EvaluationService\EvaluationService.API\EvaluationService.API.csproj
dotnet sln add src\Services\EvaluationService\EvaluationService.Domain\EvaluationService.Domain.csproj
dotnet sln add src\Services\EvaluationService\EvaluationService.Application\EvaluationService.Application.csproj
dotnet sln add src\Services\EvaluationService\EvaluationService.Infrastructure\EvaluationService.Infrastructure.csproj


### Set dependency injection
dotnet add src\Services\EvaluationService\EvaluationService.API\EvaluationService.API.csproj reference src\Services\EvaluationService\EvaluationService.Application\EvaluationService.Application.csproj
dotnet add src\Services\EvaluationService\EvaluationService.Application\EvaluationService.Application.csproj reference src\Services\EvaluationService\EvaluationService.Domain\EvaluationService.Domain.csproj
dotnet add src\Services\EvaluationService\EvaluationService.Infrastructure\EvaluationService.Infrastructure.csproj reference src\Services\EvaluationService\EvaluationService.Domain\EvaluationService.Domain.csproj
dotnet add src\Services\EvaluationService\EvaluationService.API\EvaluationService.API.csproj reference src\Services\EvaluationService\EvaluationService.Infrastructure\EvaluationService.Infrastructure.csproj
dotnet add src\Services\EvaluationService\EvaluationService.API\EvaluationService.API.csproj reference src\Services\EvaluationService\EvaluationService.Application\EvaluationService.Application.csproj
dotnet add src\Services\EvaluationService\EvaluationService.Application\EvaluationService.Application.csproj reference src\Services\EvaluationService\EvaluationService.Domain\EvaluationService.Domain.csproj
dotnet add src\Services\EvaluationService\EvaluationService.Infrastructure\EvaluationService.Infrastructure.csproj reference src\Services\EvaluationService\EvaluationService.Domain\EvaluationService.Domain.csproj
dotnet add src\Services\EvaluationService\EvaluationService.API\EvaluationService.API.csproj reference src\Services\EvaluationService\EvaluationService.Infrastructure\EvaluationService.Infrastructure.csproj


### Include packages
- API
dotnet add src/Services/EvaluationService/EvaluationService.API package Swashbuckle.AspNetCore
dotnet add src/Services/EvaluationService/EvaluationService.API package Swashbuckle.AspNetCore --version 6.6.2


- Application
dotnet add src/Services/EvaluationService/EvaluationService.Application package AutoMapper --version 12.1.0
dotnet add src/Services/EvaluationService/EvaluationService.Application package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
dotnet add src/Services/EvaluationService/EvaluationService.Application package FluentValidation --version 12.1.0
dotnet add src/Services/EvaluationService/EvaluationService.Application package FluentValidation.DependencyInjectionExtensions --version 12.1.0
dotnet add src/Services/EvaluationService/EvaluationService.Application package MediatR --version 13.1.0
dotnet add src/Services/EvaluationService/EvaluationService.Application package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add src/Services/EvaluationService/EvaluationService.Application package AutoMapper --version 15.1.1
dotnet add src/Services/EvaluationService/EvaluationService.Application package AutoMapper.Extensions.Microsoft.DependencyInjection --version 15.1.1
dotnet add src/Services/EvaluationService/EvaluationService.Application package FluentValidation --version 12.1.0
dotnet add src/Services/EvaluationService/EvaluationService.Application package FluentValidation.DependencyInjectionExtensions --version 12.1.0
dotnet add src/Services/EvaluationService/EvaluationService.Application package MediatR --version 13.1.0
dotnet add src/Services/EvaluationService/EvaluationService.Application package Microsoft.EntityFrameworkCore --version 9.0.0


- Domain

- Infrastructure
dotnet add src/Services/EvaluationService/EvaluationService.Infrastructure package Pomelo.EntityFrameworkCore.MySql --version 9.0.0
dotnet add src/Services/EvaluationService/EvaluationService.Infrastructure package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add src/Services/EvaluationService/EvaluationService.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add src/Services/EvaluationService/EvaluationService.Infrastructure package Microsoft.EntityFrameworkCore.Tools --version 9.0.0
dotnet add src/Services/EvaluationService/EvaluationService.Infrastructure package Pomelo.EntityFrameworkCore.MySql --version 9.0.0
dotnet add src/Services/EvaluationService/EvaluationService.Infrastructure package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add src/Services/EvaluationService/EvaluationService.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add src/Services/EvaluationService/EvaluationService.Infrastructure package Microsoft.EntityFrameworkCore.Tools --version 9.0.0


### add .sln for detail service
cd src/Services/EvaluationService
dotnet new sln -n EvaluationService
cd src/Services/EvaluationService
dotnet new sln -n EvaluationService

### Gemini API 
dotnet add package Google.GenAI


### Build and run the project
- Build khi ở root
+ docker compose -f docker/docker-compose.yml up --build
- Cleam & Rebuild
+ docker compose -f docker/docker-compose.yml down -v
+ docker compose -f docker/do*cker-compose.yml up -d --build*
+ docker exec ollama ollama pull llama3.1:8b
+ docker compose -f docker/docker-compose.yml up
+ docker compose --env-file .env -f docker/docker-compose.yml up

### CONFIGURATION
-Ở RoadmapService.../../appsettings.json thêm gemini api key 

### Khi sửa ocelot.json
Build lại Gateway và các deps của nó
docker compose -f docker/docker-compose.yml up -d --build api-gateway
Build lại Gateway mà không kéo theo các deps
docker compose -f docker/docker-compose.yml build api-gateway
docker compose -f docker/docker-compose.yml up -d --no-deps api-gateway

docker compose -f docker/docker-compose.yml up -d --build ai-assistant

- Tương tự khi sửa 1 service nào thì build lại service đó


# Clean up
docker compose -f docker/docker-compose.yml down -v

# Build lại
docker compose -f docker/docker-compose.yml up --build ollama

# Chờ cho đến khi thấy:
# Creating model with adapter...
# Model created successfully

# Kiểm tra model có được tạo không
docker exec ollama ollama list

# Trong terminal khác, kiểm tra model có được tạo không
docker exec ollama ollama list


### Swagger UI
http://localhost:5000/swagger/index.html

-AUTH Swagger UI
http://localhost:5000/swagger/auth/index.html

### Test Account
LEARNER: tan.dang@latee.edu.vn ------ hashed_pass_1 
EXPERT: tu.nguyen@latee.edu.vn ------ hashed_pass_2
ADMIN:  admin@latee.edu.vn     ------ hashed_pass_3

### Example gateway
http://localhost:5000/clinical-case/api/clinical-cases?status=active&page=1&pageSize=20


http://localhost:5000/ai-assistant/assistant 

### Bổ sung pakage cho Evaluation
dotnet add src/Services/EvaluationService/EvaluationService.Application/EvaluationService.Application.csproj package Microsoft.Extensions.Configuration.Abstractions
dotnet add src/Services/EvaluationService/EvaluationService.Application/EvaluationService.Application.csproj package Microsoft.Extensions.Http


