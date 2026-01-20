FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY . .
RUN ls -R src
RUN dotnet restore src/Services/ClinicalCaseService/ClinicalCaseService.API/ClinicalCaseService.API.csproj
RUN dotnet publish src/Services/ClinicalCaseService/ClinicalCaseService.API/ClinicalCaseService.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ClinicalCaseService.API.dll"]
