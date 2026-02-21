FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Api.sln ./

COPY TaskSystem.Api/TaskSystem.Api.csproj TaskSystem.Api/
COPY TaskSystem.Domain/TaskSystem.Domain.csproj TaskSystem.Domain/
COPY TaskSystem.Infrastructure/TaskSystem.Infrastructure.csproj TaskSystem.Infrastructure/
COPY TaskSystem.IntegrationTests/TaskSystem.IntegrationTests.csproj TaskSystem.IntegrationTests/

RUN dotnet restore Api.sln

COPY . .

RUN dotnet publish TaskSystem.Api/TaskSystem.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TaskSystem.Api.dll"]