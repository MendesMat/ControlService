#!/usr/bin/env bash
# =============================================================================
# build-and-push.sh — Script para build e publicação da imagem Docker
#
# Uso:
#   ./docker/build-and-push.sh [TAG]
# =============================================================================
set -euo pipefail

# ── Configuração ───────────────────────────────────────────────────────────────
REGISTRY="docker.io"
REPOSITORY="mendesmat/controlservice"
TAG="${1:-latest}"
IMAGE="${REGISTRY}/${REPOSITORY}:${TAG}"
IMAGE_LATEST="${REGISTRY}/${REPOSITORY}:latest"

# Resolve o contexto de build para a raiz do repositório
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
DOCKERFILE="${SCRIPT_DIR}/Dockerfile"

echo "============================================================"
echo "  ControlService — Build & Push"
echo "============================================================"
echo "  Repositório : ${REPOSITORY}"
echo "  Tag         : ${TAG}"
echo "  Imagem      : ${IMAGE}"
echo "  Dockerfile  : ${DOCKERFILE}"
echo "  Contexto    : ${REPO_ROOT}"
echo "============================================================"

# ── Verificação de pré-requisitos ─────────────────────────────────────────────
if ! command -v docker &> /dev/null; then
    echo "[ERRO] Docker não encontrado. Instale o Docker e tente novamente."
    exit 1
fi

# ── Build Multi-Stage ──────────────────────────────────────────────────────────
# Usa BuildKit para builds paralelas, cache avançado e saída limpa.
# O contexto é a raiz do repositório para que o Dockerfile acesse /src e /tests.
echo ""
echo "[1/2] Iniciando build (Multi-Stage)..."
echo ""

DOCKER_BUILDKIT=1 docker build \
    --file  "${DOCKERFILE}" \
    --tag   "${IMAGE}" \
    --tag   "${IMAGE_LATEST}" \
    --label "org.opencontainers.image.created=$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
    --label "org.opencontainers.image.version=${TAG}" \
    --label "org.opencontainers.image.revision=$(git -C "${REPO_ROOT}" rev-parse --short HEAD 2>/dev/null || echo 'unknown')" \
    "${REPO_ROOT}"

echo ""
echo "[1/2] Build concluído com sucesso: ${IMAGE}"

# ── Push para o Docker Hub ─────────────────────────────────────────────────────
# Realiza push da tag semântica E da tag "latest" de forma atômica.
# Camadas já existentes no registry são reutilizadas (sem re-upload).
echo ""
echo "[2/2] Publicando imagem no Docker Hub..."
echo ""

docker push "${IMAGE}"

if [ "${TAG}" != "latest" ]; then
    docker push "${IMAGE_LATEST}"
fi

echo ""
echo "============================================================"
echo "  Publicação concluída com sucesso!"
echo ""
echo "  Para utilizar a imagem:"
echo "    docker pull ${IMAGE}"
echo ""
echo "  Digest da imagem (imutável):"
docker inspect --format='  {{index .RepoDigests 0}}' "${IMAGE}" 2>/dev/null || true
echo "============================================================"
