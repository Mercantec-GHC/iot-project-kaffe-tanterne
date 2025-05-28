@echo off
REM Elevate to administrator if not already
whoami /groups | find "S-1-5-32-544" >nul
if errorlevel 1 (
    powershell -Command "Start-Process '%~f0' -ArgumentList 'elevated' -Verb RunAs"
    exit /b
)
REM Prevent infinite elevation loop
if "%1"=="elevated" set IS_BATCH_RUNNING=1
REM Relaunch in a new cmd window if not already
if not defined IS_BATCH_RUNNING (
    set IS_BATCH_RUNNING=1
    start "" cmd /k "%~f0" elevated
    exit /b
)

REM WindowsBuild.bat - Run from repo root or Scripts directory

REM Determine script directory and set project root (iot-project-kaffe-tanterne)
setlocal enabledelayedexpansion
set SCRIPT_DIR=%~dp0
set PROJECT_ROOT=%SCRIPT_DIR%..\
cd /d "%PROJECT_ROOT%"

REM git pull with submodules
call git pull --recurse-submodules

REM Remove package-lock.json if it exists
if exist "KaffeMaskineProjekt.AppHost\package-lock.json" del /f /q "KaffeMaskineProjekt.AppHost\package-lock.json"

REM If publish folder exists, run docker compose down in it
if exist "KaffeMaskineProjekt\publish" (
    pushd KaffeMaskineProjekt\publish
    call docker compose down
    popd
)

REM Run aspire publish -o publish in KaffeMaskineProjekt
pushd KaffeMaskineProjekt
call aspire publish -o publish
popd

REM Attempt to allow traffic through ports 8000-8010 (requires admin)
for /L %%P in (8000,1,8010) do (
    netsh advfirewall firewall add rule name="Allow Port %%P" dir=in action=allow protocol=TCP localport=%%P
)

REM Run docker compose up in the publish folder with KAFFEDBSERVER_PASSWORD env variable
pushd KaffeMaskineProjekt\publish
set KAFFEDBSERVER_PASSWORD=callmeal
call docker compose up
popd

endlocal

REM Keep terminal open after script finishes
pause
