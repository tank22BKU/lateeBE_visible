### How to create new service

### Create 4 layer
mkdir -p src/Services/VirtualPatientService
cd src/Services/VirtualPatientService

dotnet new webapi -n VirtualPatientService.API --framework net9.0
dotnet new classlib -n VirtualPatientService.Domain --framework net9.0
dotnet new classlib -n VirtualPatientService.Application --framework net9.0
dotnet new classlib -n VirtualPatientService.Infrastructure --framework net9.0

cd ../../../
dotnet sln add src\Services\VirtualPatientService\VirtualPatientService.API\VirtualPatientService.API.csproj
dotnet sln add src\Services\VirtualPatientService\VirtualPatientService.Domain\VirtualPatientService.Domain.csproj
dotnet sln add src\Services\VirtualPatientService\VirtualPatientService.Application\VirtualPatientService.Application.csproj
dotnet sln add src\Services\VirtualPatientService\VirtualPatientService.Infrastructure\VirtualPatientService.Infrastructure.csproj


### Set dependency injection
dotnet add src\Services\VirtualPatientService\VirtualPatientService.API\VirtualPatientService.API.csproj reference src\Services\VirtualPatientService\VirtualPatientService.Application\VirtualPatientService.Application.csproj
dotnet add src\Services\VirtualPatientService\VirtualPatientService.Application\VirtualPatientService.Application.csproj reference src\Services\VirtualPatientService\VirtualPatientService.Domain\VirtualPatientService.Domain.csproj
dotnet add src\Services\VirtualPatientService\VirtualPatientService.Infrastructure\VirtualPatientService.Infrastructure.csproj reference src\Services\VirtualPatientService\VirtualPatientService.Domain\VirtualPatientService.Domain.csproj
dotnet add src\Services\VirtualPatientService\VirtualPatientService.API\VirtualPatientService.API.csproj reference src\Services\VirtualPatientService\VirtualPatientService.Infrastructure\VirtualPatientService.Infrastructure.csproj


### Include packages
- API
dotnet add src/Services/VirtualPatientService/VirtualPatientService.API package Swashbuckle.AspNetCore


- Application
dotnet add src/Services/VirtualPatientService/VirtualPatientService.Application package AutoMapper --version 15.1.0
dotnet add src/Services/VirtualPatientService/VirtualPatientService.Application package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1
dotnet add src/Services/VirtualPatientService/VirtualPatientService.Application package FluentValidation --version 12.1.0
dotnet add src/Services/VirtualPatientService/VirtualPatientService.Application package FluentValidation.DependencyInjectionExtensions --version 12.1.0
dotnet add src/Services/VirtualPatientService/VirtualPatientService.Application package MediatR --version 13.1.0
dotnet add src/Services/VirtualPatientService/VirtualPatientService.Application package Microsoft.EntityFrameworkCore --version 9.0.0


- Domain

- Infrastructure
dotnet add src/Services/VirtualPatientService/VirtualPatientService.Infrastructure package Pomelo.EntityFrameworkCore.MySql --version 9.0.0
dotnet add src/Services/VirtualPatientService/VirtualPatientService.Infrastructure package Microsoft.EntityFrameworkCore --version 9.0.0
dotnet add src/Services/VirtualPatientService/VirtualPatientService.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 9.0.0
dotnet add src/Services/VirtualPatientService/VirtualPatientService.Infrastructure package Microsoft.EntityFrameworkCore.Tools --version 9.0.0


### add .sln for detail service
cd src/Services/VirtualPatientService
dotnet new sln -n VirtualPatientService


### Build and run the project
- Build khi ở root
+ docker compose -f docker/docker-compose.yml up --build
- Cleam & Rebuild
+ docker compose -f docker/docker-compose.yml down (if u want to migrate db and volume again LET ADD "-v" )
+ docker exec ollama ollama run llama3.1:8b
+ docker compose -f docker/docker-compose.yml up


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

# Nên thấy: llama3.1-with-adapter (merged with adapter)

# Trong terminal khác, kiểm tra model có được tạo không
docker exec ollama ollama list


### Swagger UI
http://localhost:5000/swagger/index.html

### Example gateway
http://localhost:5000/clinical-case/api/clinical-cases?status=active&page=1&pageSize=20


http://localhost:5000/ai-assistant/assistant 

