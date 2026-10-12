# Genera la versión para Windows que no necesita instalar nada:
# dist/DronesYMedicamentos-Windows.zip con el .exe (incluye .NET) y la web.
# Uso (desde la raíz del repositorio):  powershell -ExecutionPolicy Bypass -File scripts/publicar-windows.ps1

$ErrorActionPreference = "Stop"
$raiz = Split-Path -Parent $PSScriptRoot
$salida = Join-Path $raiz "dist/DronesYMedicamentos-Windows"
$zip = Join-Path $raiz "dist/DronesYMedicamentos-Windows.zip"

if (Test-Path $salida) { Remove-Item $salida -Recurse -Force }
if (Test-Path $zip) { Remove-Item $zip -Force }

dotnet publish (Join-Path $raiz "backend/API_REST_Drones_y_Medicamentos/API_REST_Drones_y_Medicamentos.csproj") `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -p:DebugType=none `
    -o $salida
if ($LASTEXITCODE -ne 0) { throw "Error al publicar" }

# Nombre fácil para el ejecutable, la web junto a él y fuera los archivos que no hacen falta
Rename-Item (Join-Path $salida "API_REST_Drones_y_Medicamentos.exe") "DronesYMedicamentos.exe"
Copy-Item (Join-Path $raiz "frontend") (Join-Path $salida "wwwroot") -Recurse
foreach ($sobra in "appsettings.Development.json", "web.config", "API_REST_Drones_y_Medicamentos.staticwebassets.endpoints.json") {
    Remove-Item (Join-Path $salida $sobra) -ErrorAction SilentlyContinue
}

Compress-Archive -Path (Join-Path $salida "*") -DestinationPath $zip
Write-Host "Listo: $zip"
