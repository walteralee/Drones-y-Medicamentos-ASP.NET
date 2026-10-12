@echo off
rem ================================================
rem Drones y Medicamentos - arranque desde el codigo en Windows
rem Para usarlo sin instalar nada esta el ZIP de la Release (ver README)
rem ================================================

setlocal EnableExtensions

cd /d "%~dp0"

title Drones y Medicamentos

set "PUERTO=5212"

echo ================================================
echo DRONES Y MEDICAMENTOS
echo ================================================

rem ================================================
rem REQUISITOS
rem ================================================

where dotnet >nul 2>nul
if errorlevel 1 (
    set "MENSAJE=No se encuentra .NET. Instala el SDK 8 o superior desde https://dotnet.microsoft.com/download"
    goto error
)

dotnet --list-sdks | findstr /r "^[89]\. ^1[0-9]\." >nul
if errorlevel 1 (
    set "MENSAJE=Hace falta el SDK de .NET 8 o superior. Descargalo de https://dotnet.microsoft.com/download"
    goto error
)

rem ================================================
rem PUERTO
rem ================================================

netstat -ano | findstr /r /c:":%PUERTO% .*LISTENING" >nul
if not errorlevel 1 (
    set "MENSAJE=El puerto %PUERTO% esta ocupado. Puede que la aplicacion ya este abierta: entra en http://localhost:%PUERTO%"
    goto error
)

rem ================================================
rem SERVIDOR
rem ================================================

echo.
echo Compilando y arrancando (la primera vez tarda un poco)...
echo Se abrira el navegador en http://localhost:%PUERTO%
echo Para detener la aplicacion cierra esta ventana o pulsa Ctrl+C.
echo.

rem La propia aplicacion abre el navegador cuando esta lista
set "AbrirNavegador=true"
dotnet run --project backend\DronesYMedicamentos.Api.csproj
if errorlevel 1 (
    set "MENSAJE=La aplicacion se ha detenido con un error. Revisa los mensajes de arriba"
    goto error
)
exit /b 0

:error
echo.
echo [ERROR] %MENSAJE%
echo.
pause
exit /b 1
