# Genera la versión para Windows que no necesita instalar nada:
# dist/DronesYMedicamentos-Windows.zip con el .exe (incluye .NET), la librería de SQLite y la web.
# Nada se extrae a carpetas temporales: al borrar la carpeta no queda nada en el equipo.
# Uso (desde la raíz del repositorio):  powershell -ExecutionPolicy Bypass -File scripts/publicar-windows.ps1

$ErrorActionPreference = "Stop"
$raiz = Split-Path -Parent $PSScriptRoot
$salida = Join-Path $raiz "dist/DronesYMedicamentos-Windows"
$zip = Join-Path $raiz "dist/DronesYMedicamentos-Windows.zip"

if (Test-Path $salida) { Remove-Item $salida -Recurse -Force }
if (Test-Path $zip) { Remove-Item $zip -Force }

dotnet publish (Join-Path $raiz "backend/DronesYMedicamentos.Api.csproj") `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:DebugType=none `
    -o $salida
if ($LASTEXITCODE -ne 0) { throw "Error al publicar" }

# Nombre fácil para el ejecutable, la web junto a él y fuera los archivos que no hacen falta
Rename-Item (Join-Path $salida "DronesYMedicamentos.Api.exe") "DronesYMedicamentos.exe"
Copy-Item (Join-Path $raiz "frontend") (Join-Path $salida "wwwroot") -Recurse
foreach ($sobra in "appsettings.Development.json", "web.config", "aspnetcorev2_inprocess.dll", "DronesYMedicamentos.Api.staticwebassets.endpoints.json") {
    Remove-Item (Join-Path $salida $sobra) -ErrorAction SilentlyContinue
}

Compress-Archive -Path (Join-Path $salida "*") -DestinationPath $zip
Write-Host "Listo: $zip"
