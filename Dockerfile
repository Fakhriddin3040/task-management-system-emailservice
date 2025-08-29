# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY TaskManagementSystem.EmailService.sln ./
COPY TaskManagementSystem.EmailService/TaskManagementSystem.EmailService.csproj TaskManagementSystem.EmailService/
COPY TaskManagementSystem.Messaging/TaskManagementSystem.Messaging.csproj TaskManagementSystem.Messaging/

RUN dotnet restore TaskManagementSystem.EmailService/TaskManagementSystem.EmailService.csproj

COPY . ./

RUN dotnet publish TaskManagementSystem.EmailService/TaskManagementSystem.EmailService.csproj -c Release -o /app/out --no-restore

# ===== runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080 \
    ASPNETCORE_Kestrel__EndpointDefaults__Protocols=Http1AndHttp2

COPY --from=build /app/out ./

EXPOSE 8080
ENTRYPOINT ["dotnet", "TaskManagementSystem.EmailService.dll"]
