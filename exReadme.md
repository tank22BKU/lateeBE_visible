### How to create new service

### Create 4 layer
mkdir -p src/Services/RoadmapService
cd src/Services/RoadmapService

dotnet new webapi -n RoadmapService.API --framework net9.0
dotnet new classlib -n RoadmapService.Domain --framework net9.0
dotnet new classlib -n RoadmapService.Application --framework net9.0
dotnet new classlib -n RoadmapService.Infrastructure --framework net9.0

cd ../../../
dotnet sln add src\Services\RoadmapService\RoadmapService.API\RoadmapService.API.csproj
dotnet sln add src\Services\RoadmapService\RoadmapService.Domain\RoadmapService.Domain.csproj
dotnet sln add src\Services\RoadmapService\RoadmapService.Application\RoadmapService.Application.csproj
dotnet sln add src\Services\RoadmapService\RoadmapService.Infrastructure\RoadmapService.Infrastructure.csproj


### Set dependency injection
dotnet add src\Services\RoadmapService\RoadmapService.API\RoadmapService.API.csproj reference src\Services\RoadmapService\RoadmapService.Application\RoadmapService.Application.csproj
dotnet add src\Services\RoadmapService\RoadmapService.Application\RoadmapService.Application.csproj reference src\Services\RoadmapService\RoadmapService.Domain\RoadmapService.Domain.csproj
dotnet add src\Services\RoadmapService\RoadmapService.Infrastructure\RoadmapService.Infrastructure.csproj reference src\Services\RoadmapService\RoadmapService.Domain\RoadmapService.Domain.csproj
dotnet add src\Services\RoadmapService\RoadmapService.API\RoadmapService.API.csproj reference src\Services\RoadmapService\RoadmapService.Infrastructure\RoadmapService.Infrastructure.csproj


### Include packages
- API
dotnet add src/Services/RoadmapService/RoadmapService.API package Swashbuckle.AspNetCore


- Application
dotnet add src/Services/RoadmapService/RoadmapService.Application package AutoMapper --version 15.1.0
dotnet add src/Services/RoadmapService/RoadmapService.Application package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
dotnet add src/Services/RoadmapService/RoadmapService.Application package FluentValidation --version 12.1.0
dotnet add src/Services/RoadmapService/RoadmapService.Application package FluentValidation.DependencyInjectionExtensions --version 12.1.0
dotnet add src/Services/RoadmapService/RoadmapService.Application package MediatR --version 13.1.0
dotnet add src/Services/RoadmapService/RoadmapService.Application package Microsoft.EntityFrameworkCore --version 9.0.0


- Domain

- Infrastructure
dotnet add src/Services/RoadmapService/RoadmapService.Infrastructure package Pomelo.EntityFrameworkCore.MySql --version 9.0.0
dotnet add src/Services/RoadmapService/RoadmapService.Infrastructure package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add src/Services/RoadmapService/RoadmapService.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add src/Services/RoadmapService/RoadmapService.Infrastructure package Microsoft.EntityFrameworkCore.Tools --version 9.0.0


### add .sln for detail service
cd src/Services/RoadmapService
dotnet new sln -n RoadmapService


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

