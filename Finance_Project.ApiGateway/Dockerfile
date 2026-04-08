# === Build stage ===
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia csproj separadamente para aproveitar cache de layers do Docker
COPY src/ApiGateway/ApiGateway.csproj ApiGateway/

# Restore (cached se o csproj não mudou)
RUN dotnet restore ApiGateway/ApiGateway.csproj

# Copia todo o código e faz o build
COPY src/ .
RUN dotnet publish ApiGateway/ApiGateway.csproj \
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

# Health check nativo do Docker
HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
