# === Build stage ===
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia csproj separadamente para aproveitar cache de layers do Docker
COPY src/CardsService.Domain/CardsService.Domain.csproj CardsService.Domain/
COPY src/CardsService.Application/CardsService.Application.csproj CardsService.Application/
COPY src/CardsService.Infrastructure/CardsService.Infrastructure.csproj CardsService.Infrastructure/
COPY src/CardsService.API/CardsService.API.csproj CardsService.API/

# Restore (cached se os csproj não mudaram)
RUN dotnet restore CardsService.API/CardsService.API.csproj

# Copia todo o código e faz o build
COPY src/ .
RUN dotnet publish CardsService.API/CardsService.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# === Runtime stage (imagem mínima, sem SDK) ===
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Usuário não-root para segurança em produção
RUN groupadd -r appuser && useradd -r -g appuser appuser
USER appuser

COPY --from=build /app/publish .

# Porta configurável via variável de ambiente
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Health check nativo do Docker (complementa o do Kubernetes)
HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "CardsService.API.dll"]
