# =============================================================================
# Dockerfile — ControlService API
# =============================================================================


# -----------------------------------------------------------------------------
# ESTÁGIO 1 — Builder
# -----------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /repo

# ── Restauração de dependências (camada estável) ───────────────────────────────
COPY backend/ControlService.slnx                                                                       ./backend/
COPY backend/src/ControlService.SharedKernel/ControlService.SharedKernel.csproj                        ./backend/src/ControlService.SharedKernel/
COPY backend/src/ControlService.API/ControlService.API.csproj                                          ./backend/src/ControlService.API/
COPY backend/src/ControlService.Commercial/ControlService.Commercial.Domain/ControlService.Commercial.Domain.csproj             ./backend/src/ControlService.Commercial/ControlService.Commercial.Domain/
COPY backend/src/ControlService.Commercial/ControlService.Commercial.Application/ControlService.Commercial.Application.csproj   ./backend/src/ControlService.Commercial/ControlService.Commercial.Application/
COPY backend/src/ControlService.Commercial/ControlService.Commercial.Infrastructure/ControlService.Commercial.Infrastructure.csproj ./backend/src/ControlService.Commercial/ControlService.Commercial.Infrastructure/
COPY backend/src/ControlService.Management/ControlService.Management.Domain/ControlService.Management.Domain.csproj             ./backend/src/ControlService.Management/ControlService.Management.Domain/
COPY backend/src/ControlService.Management/ControlService.Management.Application/ControlService.Management.Application.csproj   ./backend/src/ControlService.Management/ControlService.Management.Application/
COPY backend/src/ControlService.Management/ControlService.Management.Infrastructure/ControlService.Management.Infrastructure.csproj ./backend/src/ControlService.Management/ControlService.Management.Infrastructure/
COPY backend/src/ControlService.Operational/ControlService.Operational.Domain/ControlService.Operational.Domain.csproj             ./backend/src/ControlService.Operational/ControlService.Operational.Domain/
COPY backend/src/ControlService.Operational/ControlService.Operational.Application/ControlService.Operational.Application.csproj   ./backend/src/ControlService.Operational/ControlService.Operational.Application/
COPY backend/src/ControlService.Operational/ControlService.Operational.Infrastructure/ControlService.Operational.Infrastructure.csproj ./backend/src/ControlService.Operational/ControlService.Operational.Infrastructure/
COPY backend/src/ControlService.Financial/ControlService.Financial.Domain/ControlService.Financial.Domain.csproj             ./backend/src/ControlService.Financial/ControlService.Financial.Domain/
COPY backend/src/ControlService.Financial/ControlService.Financial.Application/ControlService.Financial.Application.csproj   ./backend/src/ControlService.Financial/ControlService.Financial.Application/
COPY backend/src/ControlService.Financial/ControlService.Financial.Infrastructure/ControlService.Financial.Infrastructure.csproj ./backend/src/ControlService.Financial/ControlService.Financial.Infrastructure/
COPY backend/src/ControlService.Reporting/ControlService.Reporting.Domain/ControlService.Reporting.Domain.csproj             ./backend/src/ControlService.Reporting/ControlService.Reporting.Domain/
COPY backend/src/ControlService.Reporting/ControlService.Reporting.Application/ControlService.Reporting.Application.csproj   ./backend/src/ControlService.Reporting/ControlService.Reporting.Application/
COPY backend/src/ControlService.Reporting/ControlService.Reporting.Infrastructure/ControlService.Reporting.Infrastructure.csproj ./backend/src/ControlService.Reporting/ControlService.Reporting.Infrastructure/
COPY backend/tests/ControlService.Domain.Tests/ControlService.Domain.Tests.csproj             ./backend/tests/ControlService.Domain.Tests/
COPY backend/tests/ControlService.Application.Tests/ControlService.Application.Tests.csproj  ./backend/tests/ControlService.Application.Tests/

RUN dotnet restore backend/ControlService.slnx

# ── Cópia do código-fonte (camada instável) ────────────────────────────────────
COPY backend/src/   ./backend/src/
COPY backend/tests/ ./backend/tests/

# ── Testes de Unidade ─────────────────────────────────────────────────────────
RUN dotnet test ./backend/tests/ControlService.Domain.Tests/ControlService.Domain.Tests.csproj \
        --no-restore \
        --configuration Release \
        --logger "console;verbosity=minimal"

RUN dotnet test ./backend/tests/ControlService.Application.Tests/ControlService.Application.Tests.csproj \
        --no-restore \
        --configuration Release \
        --logger "console;verbosity=minimal"

# ── Publicação (Release) ───────────────────────────────────────────────────────
RUN dotnet publish ./backend/src/ControlService.API/ControlService.API.csproj \
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
