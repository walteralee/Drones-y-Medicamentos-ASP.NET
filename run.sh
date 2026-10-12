#!/usr/bin/env bash
# ================================================
# Drones y Medicamentos - arranque desde el codigo en Linux y macOS
# Tambien se puede usar con Docker: docker compose up -d (ver README)
# ================================================

set -euo pipefail

cd "$(dirname "$0")"

PUERTO=5212

error() {
  echo
  echo "[ERROR] $*" >&2
  exit 1
}

echo "================================================"
echo "DRONES Y MEDICAMENTOS"
echo "================================================"

# ================================================
# REQUISITOS
# ================================================

command -v dotnet >/dev/null 2>&1 ||
  error "No se encuentra .NET. Instala el SDK 8 o superior desde https://dotnet.microsoft.com/download (o usa Docker)"

dotnet --list-sdks | grep -Eq '^([89]|[1-9][0-9])\.' ||
  error "Hace falta el SDK de .NET 8 o superior. Descargalo de https://dotnet.microsoft.com/download"

# ================================================
# PUERTO
# ================================================

if (exec 3<>"/dev/tcp/127.0.0.1/$PUERTO") 2>/dev/null; then
  error "El puerto $PUERTO esta ocupado. Puede que la aplicacion ya este abierta: entra en http://localhost:$PUERTO"
fi

# ================================================
# SERVIDOR
# ================================================

echo
echo "Compilando y arrancando (la primera vez tarda un poco)..."
echo "Se abrira el navegador en http://localhost:$PUERTO"
echo "Pulsa Ctrl+C para detener la aplicacion."
echo

# La propia aplicacion abre el navegador cuando esta lista
export AbrirNavegador=true
dotnet run --project backend/DronesYMedicamentos.Api.csproj
