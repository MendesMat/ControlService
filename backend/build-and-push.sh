#!/usr/bin/env bash
# =============================================================================
# build-and-push.sh — Script para build e publicação da imagem Docker
#
# Deve ser executado a partir da RAIZ do repositório ou de dentro de backend/.
# O contexto de build é sempre a pasta backend/ (self-contained).
#
# Uso:
#   ./backend/build-and-push.sh [TAG]
# =============================================================================
set -euo pipefail

# ── Configuração ───────────────────────────────────────────────────────────────
REGISTRY="docker.io"
REPOSITORY="mendesmat/controlservice"
TAG="${1:-latest}"
IMAGE="${REGISTRY}/${REPOSITORY}:${TAG}"
IMAGE_LATEST="${REGISTRY}/${REPOSITORY}:latest"

# Resolve o diretório backend/ independente de onde o script é chamado
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DOCKERFILE="${SCRIPT_DIR}/Dockerfile"
# Contexto de build: a pasta backend/ é auto-contida
BUILD_CONTEXT="${SCRIPT_DIR}"

echo "============================================================"
echo "  ControlService — Build & Push"
echo "============================================================"
echo "  Repositório : ${REPOSITORY}"
echo "  Tag         : ${TAG}"
echo "  Imagem      : ${IMAGE}"
echo "  Dockerfile  : ${DOCKERFILE}"
echo "  Contexto    : ${BUILD_CONTEXT}"
echo "============================================================"

# ── Verificação de pré-requisitos ─────────────────────────────────────────────
if ! command -v docker &> /dev/null; then
    echo "[ERRO] Docker não encontrado. Instale o Docker e tente novamente."
    exit 1
fi

# ── Build Multi-Stage ──────────────────────────────────────────────────────────
# Contexto é a pasta backend/ — o Dockerfile acessa src/ e tests/ relativos a ela.
echo ""
echo "[1/2] Iniciando build (Multi-Stage)..."
echo ""

DOCKER_BUILDKIT=1 docker build \
    --file  "${DOCKERFILE}" \
    --tag   "${IMAGE}" \
    --tag   "${IMAGE_LATEST}" \
    --label "org.opencontainers.image.created=$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
    --label "org.opencontainers.image.version=${TAG}" \
    --label "org.opencontainers.image.revision=$(git -C "${SCRIPT_DIR}" rev-parse --short HEAD 2>/dev/null || echo 'unknown')" \
    "${BUILD_CONTEXT}"

echo ""
echo "[1/2] Build concluído com sucesso: ${IMAGE}"

# ── Push para o Docker Hub ─────────────────────────────────────────────────────
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
