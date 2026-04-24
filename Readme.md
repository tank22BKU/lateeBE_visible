### How to configure
- fill HF_TOKEN in .env
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
mkdir -p src/Services/AssessmentService
cd src/Services/AssessmentService

dotnet new webapi -n AssessmentService.API --framework net9.0
dotnet new classlib -n AssessmentService.Domain --framework net9.0
dotnet new classlib -n AssessmentService.Application --framework net9.0
dotnet new classlib -n AssessmentService.Infrastructure --framework net9.0

cd ../../../
dotnet sln add src\Services\AssessmentService\AssessmentService.API\AssessmentService.API.csproj
dotnet sln add src\Services\AssessmentService\AssessmentService.Domain\AssessmentService.Domain.csproj
dotnet sln add src\Services\AssessmentService\AssessmentService.Application\AssessmentService.Application.csproj
dotnet sln add src\Services\AssessmentService\AssessmentService.Infrastructure\AssessmentService.Infrastructure.csproj


### Set dependency injection
dotnet add src\Services\AssessmentService\AssessmentService.API\AssessmentService.API.csproj reference src\Services\AssessmentService\AssessmentService.Application\AssessmentService.Application.csproj
dotnet add src\Services\AssessmentService\AssessmentService.Application\AssessmentService.Application.csproj reference src\Services\AssessmentService\AssessmentService.Domain\AssessmentService.Domain.csproj
dotnet add src\Services\AssessmentService\AssessmentService.Infrastructure\AssessmentService.Infrastructure.csproj reference src\Services\AssessmentService\AssessmentService.Domain\AssessmentService.Domain.csproj
dotnet add src\Services\AssessmentService\AssessmentService.API\AssessmentService.API.csproj reference src\Services\AssessmentService\AssessmentService.Infrastructure\AssessmentService.Infrastructure.csproj


### Include packages
- API
dotnet add src/Services/AssessmentService/AssessmentService.API package Swashbuckle.AspNetCore --version 6.6.2


- Application
dotnet add src/Services/AssessmentService/AssessmentService.Application package AutoMapper --version 15.1.1
dotnet add src/Services/AssessmentService/AssessmentService.Application package AutoMapper.Extensions.Microsoft.DependencyInjection --version 15.1.1
dotnet add src/Services/AssessmentService/AssessmentService.Application package FluentValidation --version 12.1.0
dotnet add src/Services/AssessmentService/AssessmentService.Application package FluentValidation.DependencyInjectionExtensions --version 12.1.0
dotnet add src/Services/AssessmentService/AssessmentService.Application package MediatR --version 13.1.0
dotnet add src/Services/AssessmentService/AssessmentService.Application package Microsoft.EntityFrameworkCore --version 9.0.0


- Domain

- Infrastructure
dotnet add src/Services/AssessmentService/AssessmentService.Infrastructure package Pomelo.EntityFrameworkCore.MySql --version 9.0.0
dotnet add src/Services/AssessmentService/AssessmentService.Infrastructure package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add src/Services/AssessmentService/AssessmentService.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add src/Services/AssessmentService/AssessmentService.Infrastructure package Microsoft.EntityFrameworkCore.Tools --version 9.0.0


### add .sln for detail service
cd src/Services/AssessmentService
dotnet new sln -n AssessmentService

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

### CONFIGURATION
-Ở RoadmapService.../../appsettings.json thêm gemini api key 

### Khi sửa ocelot.json
docker compose -f docker/docker-compose.yml up -d --build api-gateway
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

### Example gateway
http://localhost:5000/clinical-case/api/clinical-cases?status=active&page=1&pageSize=20


http://localhost:5000/ai-assistant/assistant 

