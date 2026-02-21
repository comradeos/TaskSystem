FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Migrators.sln ./
COPY PostgreSQL/PostgreSQL.csproj PostgreSQL/

RUN dotnet restore

COPY . .

WORKDIR /src/PostgreSQL
RUN dotnet publish -c Release -o /app/out


FROM mcr.microsoft.com/dotnet/runtime:8.0-bookworm-slim

RUN apt-get update && apt-get install -y \
    libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/out ./

ENTRYPOINT ["dotnet", "PostgreSQL.dll"]