# =============================================================================
# Dockerfile — ControlService API
# Contexto de build: pasta backend/ (self-contained)
# =============================================================================


# -----------------------------------------------------------------------------
# ESTÁGIO 1 — Builder
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /repo

# ── Restauração de dependências (camada estável) ───────────────────────────────
COPY ControlService.slnx                                                                       ./
COPY src/ControlService.SharedKernel/ControlService.SharedKernel.csproj                        ./src/ControlService.SharedKernel/
COPY src/ControlService.API/ControlService.API.csproj                                          ./src/ControlService.API/
COPY src/ControlService.Commercial/ControlService.Commercial.Domain/ControlService.Commercial.Domain.csproj             ./src/ControlService.Commercial/ControlService.Commercial.Domain/
COPY src/ControlService.Commercial/ControlService.Commercial.Application/ControlService.Commercial.Application.csproj   ./src/ControlService.Commercial/ControlService.Commercial.Application/
COPY src/ControlService.Commercial/ControlService.Commercial.Infrastructure/ControlService.Commercial.Infrastructure.csproj ./src/ControlService.Commercial/ControlService.Commercial.Infrastructure/
COPY src/ControlService.Management/ControlService.Management.Domain/ControlService.Management.Domain.csproj             ./src/ControlService.Management/ControlService.Management.Domain/
COPY src/ControlService.Management/ControlService.Management.Application/ControlService.Management.Application.csproj   ./src/ControlService.Management/ControlService.Management.Application/
COPY src/ControlService.Management/ControlService.Management.Infrastructure/ControlService.Management.Infrastructure.csproj ./src/ControlService.Management/ControlService.Management.Infrastructure/
COPY src/ControlService.Operational/ControlService.Operational.Domain/ControlService.Operational.Domain.csproj             ./src/ControlService.Operational/ControlService.Operational.Domain/
COPY src/ControlService.Operational/ControlService.Operational.Application/ControlService.Operational.Application.csproj   ./src/ControlService.Operational/ControlService.Operational.Application/
COPY src/ControlService.Operational/ControlService.Operational.Infrastructure/ControlService.Operational.Infrastructure.csproj ./src/ControlService.Operational/ControlService.Operational.Infrastructure/
COPY src/ControlService.Financial/ControlService.Financial.Domain/ControlService.Financial.Domain.csproj             ./src/ControlService.Financial/ControlService.Financial.Domain/
COPY src/ControlService.Financial/ControlService.Financial.Application/ControlService.Financial.Application.csproj   ./src/ControlService.Financial/ControlService.Financial.Application/
COPY src/ControlService.Financial/ControlService.Financial.Infrastructure/ControlService.Financial.Infrastructure.csproj ./src/ControlService.Financial/ControlService.Financial.Infrastructure/
COPY src/ControlService.Reporting/ControlService.Reporting.Domain/ControlService.Reporting.Domain.csproj             ./src/ControlService.Reporting/ControlService.Reporting.Domain/
COPY src/ControlService.Reporting/ControlService.Reporting.Application/ControlService.Reporting.Application.csproj   ./src/ControlService.Reporting/ControlService.Reporting.Application/
COPY src/ControlService.Reporting/ControlService.Reporting.Infrastructure/ControlService.Reporting.Infrastructure.csproj ./src/ControlService.Reporting/ControlService.Reporting.Infrastructure/
COPY tests/ControlService.Domain.Tests/ControlService.Domain.Tests.csproj             ./tests/ControlService.Domain.Tests/
COPY tests/ControlService.Application.Tests/ControlService.Application.Tests.csproj  ./tests/ControlService.Application.Tests/

RUN dotnet restore ControlService.slnx

# ── Cópia do código-fonte (camada instável) ────────────────────────────────────
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
