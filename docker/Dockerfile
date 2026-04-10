# =============================================================================
# Dockerfile — ControlService API
# =============================================================================


# -----------------------------------------------------------------------------
# ESTÁGIO 1 — Builder
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /repo

# ── Restauração de dependências (camada estável) ───────────────────────────────
# Copiamos apenas os arquivos de projeto primeiro para maximizar o cache Docker.
# Se o código-fonte mudar mas o .csproj não mudar, o `dotnet restore` é cacheado.
COPY ControlService.slnx                                                              ./
COPY src/ControlService.API/ControlService.API.csproj                                 ./src/ControlService.API/
COPY src/ControlService.Application/ControlService.Application.csproj                 ./src/ControlService.Application/
COPY src/ControlService.Domain/ControlService.Domain.csproj                           ./src/ControlService.Domain/
COPY src/ControlService.Infrastructure/ControlService.Infrastructure.csproj           ./src/ControlService.Infrastructure/
COPY tests/ControlService.Domain.Tests/ControlService.Domain.Tests.csproj             ./tests/ControlService.Domain.Tests/
COPY tests/ControlService.Application.Tests/ControlService.Application.Tests.csproj  ./tests/ControlService.Application.Tests/

RUN dotnet restore

# ── Cópia do código-fonte (camada instável) ────────────────────────────────────
# Copiado após o restore para que mudanças no src não invalidem o cache do NuGet.
COPY src/   ./src/
COPY tests/ ./tests/

# ── Testes de Unidade ─────────────────────────────────────────────────────────
RUN dotnet test ./tests/ControlService.Domain.Tests/ControlService.Domain.Tests.csproj \
        --no-restore \
        --configuration Release \
        --logger "console;verbosity=minimal"

RUN dotnet test ./tests/ControlService.Application.Tests/ControlService.Application.Tests.csproj \
        --no-restore \
        --configuration Release \
        --logger "console;verbosity=minimal"

# ── Publicação (Release) ───────────────────────────────────────────────────────
RUN dotnet publish ./src/ControlService.API/ControlService.API.csproj \
        --no-restore \
        --configuration Release \
        --output /app/publish \
        /p:UseAppHost=false


# -----------------------------------------------------------------------------
# ESTÁGIO 2 — Runtime
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime

# ── Metadados OCI ────────────────────────────────────────────────────────────
LABEL org.opencontainers.image.title="ControlService API" \
      org.opencontainers.image.description="API de Gerenciamento de Controle de Serviços — Projeto de Pós-Graduação" \
      org.opencontainers.image.vendor="MendesMat" \
      org.opencontainers.image.source="https://github.com/MendesMat/ControlService" \
      org.opencontainers.image.licenses="MIT"

WORKDIR /app

# ── Segurança ─────────────────────────────────────────────────────────────────
EXPOSE 8080

# ── Configuração de Inicialização ─────────────────────────────────────────────
ENTRYPOINT ["dotnet", "ControlService.API.dll"]

# ── Cópia dos binários do estágio builder ─────────────────────────────────────
# Apenas o artefato publicado (sem SDK, sem fontes, sem ferramentas de build).
COPY --from=build /app/publish .
