### How to create new service

mkdir -p src/Services/ClinicalCaseService
cd src/Services/ClinicalCaseService

dotnet new webapi -n ClinicalCaseService.API --framework net9.0
dotnet new classlib -n ClinicalCaseService.Domain --framework net9.0
dotnet new classlib -n ClinicalCaseService.Application --framework net9.0
dotnet new classlib -n ClinicalCaseService.Infrastructure --framework net9.0

cd ../../../
dotnet sln add src\Services\ClinicalCaseService\ClinicalCaseService.API\ClinicalCaseService.API.csproj
dotnet sln add src\Services\ClinicalCaseService\ClinicalCaseService.Domain\ClinicalCaseService.Domain.csproj
dotnet sln add src\Services\ClinicalCaseService\ClinicalCaseService.Application\ClinicalCaseService.Application.csproj
dotnet sln add src\Services\ClinicalCaseService\ClinicalCaseService.Infrastructure\ClinicalCaseService.Infrastructure.csproj

dotnet add src\Services\ClinicalCaseService\ClinicalCaseService.API\ClinicalCaseService.API.csproj reference src\Services\ClinicalCaseService\ClinicalCaseService.Application\ClinicalCaseService.Application.csproj
dotnet add src\Services\ClinicalCaseService\ClinicalCaseService.Application\ClinicalCaseService.Application.csproj reference src\Services\ClinicalCaseService\ClinicalCaseService.Domain\ClinicalCaseService.Domain.csproj
dotnet add src\Services\ClinicalCaseService\ClinicalCaseService.Infrastructure\ClinicalCaseService.Infrastructure.csproj reference src\Services\ClinicalCaseService\ClinicalCaseService.Domain\ClinicalCaseService.Domain.csproj
dotnet add src\Services\ClinicalCaseService\ClinicalCaseService.API\ClinicalCaseService.API.csproj reference src\Services\ClinicalCaseService\ClinicalCaseService.Infrastructure\ClinicalCaseService.Infrastructure.csproj


dotnet add src/Services/ClinicalCaseService/ClinicalCaseService.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/Services/ClinicalCaseService/ClinicalCaseService.Infrastructure package Pomelo.EntityFrameworkCore.MySql

###

### Build and run the project
- cd docker
- docker compose up --build
- Build khi ở root
+ docker compose -f docker/docker-compose.yml up --build
- Cleam & Rebuild
+ docker compose -f docker/docker-compose.yml down -v
+ docker compose -f docker/docker-compose.yml build --no-cache
+ docker compose -f docker/docker-compose.yml up

