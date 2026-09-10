@echo off
setlocal

title Cicci Research Sample Manager - Build and Deploy

:: Ensure the script runs as Administrator.
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Requesting Administrator privileges...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

set "PROJECT_DIR=%~dp0"
set "PROJECT_FILE=%PROJECT_DIR%Sample Manager Tool.csproj"
set "PUBLISH_DIR=C:\Program Files\Cicci Research\Sample Manager Tool"
set "SERVICE_NAME=CicciSampleManager"

echo.
echo ==========================================
echo   CICCI Sample Manager Tool
echo   Build and Deploy
echo ==========================================
echo.

if not exist "%PROJECT_FILE%" (
    echo ERROR: Project file not found:
    echo "%PROJECT_FILE%"
    echo.
    echo Place this BAT file in the root folder of the Sample Manager Tool repository.
    pause
    exit /b 1
)

where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: dotnet was not found.
    echo Install the .NET SDK and try again.
    pause
    exit /b 1
)

echo Stopping Windows service if it exists...
sc query "%SERVICE_NAME%" >nul 2>&1
if %errorlevel% equ 0 (
    net stop "%SERVICE_NAME%" >nul 2>&1
)

echo.
echo Publishing Release build...
dotnet publish "%PROJECT_FILE%" ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -o "%PUBLISH_DIR%"

if %errorlevel% neq 0 (
    echo.
    echo BUILD FAILED.
    pause
    exit /b 1
)

echo.
echo Build completed successfully.

sc query "%SERVICE_NAME%" >nul 2>&1
if %errorlevel% equ 0 (
    echo Restarting Windows service...
    net start "%SERVICE_NAME%"
    if %errorlevel% neq 0 (
        echo.
        echo WARNING: Build succeeded, but the Windows service could not be started.
        echo Check Windows Event Viewer for details.
        pause
        exit /b 1
    )
)

echo.
echo ==========================================
echo   DEPLOYMENT COMPLETE
echo ==========================================
echo.
echo Application:
echo %PUBLISH_DIR%
echo.
echo Web interface:
echo http://127.0.0.1:5227
echo.
pause
endlocal
