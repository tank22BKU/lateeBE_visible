### How to create new service

### Create 4 layer
mkdir -p src/Services/AIAssistantService
cd src/Services/AIAssistantService

dotnet new webapi -n AIAssistantService.API --framework net9.0
dotnet new classlib -n AIAssistantService.Domain --framework net9.0
dotnet new classlib -n AIAssistantService.Application --framework net9.0
dotnet new classlib -n AIAssistantService.Infrastructure --framework net9.0

cd ../../../
dotnet sln add src\Services\AIAssistantService\AIAssistantService.API\AIAssistantService.API.csproj
dotnet sln add src\Services\AIAssistantService\AIAssistantService.Domain\AIAssistantService.Domain.csproj
dotnet sln add src\Services\AIAssistantService\AIAssistantService.Application\AIAssistantService.Application.csproj
dotnet sln add src\Services\AIAssistantService\AIAssistantService.Infrastructure\AIAssistantService.Infrastructure.csproj


### Set dependency injection
dotnet add src\Services\AIAssistantService\AIAssistantService.API\AIAssistantService.API.csproj reference src\Services\AIAssistantService\AIAssistantService.Application\AIAssistantService.Application.csproj
dotnet add src\Services\AIAssistantService\AIAssistantService.Application\AIAssistantService.Application.csproj reference src\Services\AIAssistantService\AIAssistantService.Domain\AIAssistantService.Domain.csproj
dotnet add src\Services\AIAssistantService\AIAssistantService.Infrastructure\AIAssistantService.Infrastructure.csproj reference src\Services\AIAssistantService\AIAssistantService.Domain\AIAssistantService.Domain.csproj
dotnet add src\Services\AIAssistantService\AIAssistantService.API\AIAssistantService.API.csproj reference src\Services\AIAssistantService\AIAssistantService.Infrastructure\AIAssistantService.Infrastructure.csproj


### Include packages
- API
dotnet add src/Services/AIAssistantService/AIAssistantService.API package Swashbuckle.AspNetCore


- Application
dotnet add src/Services/AIAssistantService/AIAssistantService.Application package AutoMapper --version 15.1.0
dotnet add src/Services/AIAssistantService/AIAssistantService.Application package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
dotnet add src/Services/AIAssistantService/AIAssistantService.Application package FluentValidation --version 12.1.0
dotnet add src/Services/AIAssistantService/AIAssistantService.Application package FluentValidation.DependencyInjectionExtensions --version 12.1.0
dotnet add src/Services/AIAssistantService/AIAssistantService.Application package MediatR --version 13.1.0
dotnet add src/Services/AIAssistantService/AIAssistantService.Application package Microsoft.EntityFrameworkCore --version 9.0.0


- Domain

- Infrastructure
dotnet add src/Services/AIAssistantService/AIAssistantService.Infrastructure package Pomelo.EntityFrameworkCore.MySql --version 9.0.0
dotnet add src/Services/AIAssistantService/AIAssistantService.Infrastructure package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add src/Services/AIAssistantService/AIAssistantService.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add src/Services/AIAssistantService/AIAssistantService.Infrastructure package Microsoft.EntityFrameworkCore.Tools --version 9.0.0


### add .sln for detail service
cd src/Services/AIAssistantService
dotnet new sln -n AIAssistantService


### Build and run the project
- Build khi ở root
+ docker compose -f docker/docker-compose.yml up --build
- Cleam & Rebuild
+ docker compose -f docker/docker-compose.yml down -v
+ docker compose -f docker/docker-compose.yml up -d --build
+ docker exec ollama ollama pull llama3.1:8b
+ docker compose -f docker/docker-compose.yml up


### Khi sửa ocelot.json
docker compose -f docker/docker-compose.yml up -d --build api-gateway
docker compose -f docker/docker-compose.yml up -d --build ai-assistant

- Tương tự khi sửa 1 service nào thì build lại service đó

### Swagger UI
http://localhost:5000/swagger/index.html

### Example gateway
http://localhost:5000/clinical-case/api/clinical-cases?status=active&page=1&pageSize=20


http://localhost:5000/ai-assistant/assistant 

