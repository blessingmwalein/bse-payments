@echo off
echo ========================================
echo BSE Payment Service - IIS Quick Fix
echo ========================================
echo.

set DEPLOY_PATH=C:\inetpub\wwwroot\BSE-PAYMENTS

echo Checking if deployment path exists...
if not exist "%DEPLOY_PATH%" (
    echo ERROR: Deployment path not found: %DEPLOY_PATH%
    echo Please update DEPLOY_PATH in this script.
    pause
    exit /b 1
)

echo.
echo [1/5] Copying web.config...
copy /Y "web.config" "%DEPLOY_PATH%\web.config"

echo [2/5] Creating logs directory...
if not exist "%DEPLOY_PATH%\logs" mkdir "%DEPLOY_PATH%\logs"

echo [3/5] Setting folder permissions...
icacls "%DEPLOY_PATH%" /grant "IIS_IUSRS:(OI)(CI)(RX)" /T
icacls "%DEPLOY_PATH%" /grant "IIS AppPool\BSEPaymentsPool:(OI)(CI)(RX)" /T
icacls "%DEPLOY_PATH%\logs" /grant "IIS AppPool\BSEPaymentsPool:(OI)(CI)(M)" /T

echo [4/5] Checking .NET Hosting Bundle...
dotnet --list-runtimes | findstr "Microsoft.AspNetCore.App 8"
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo WARNING: .NET 8.0 Hosting Bundle may not be installed!
    echo Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    echo Look for "Hosting Bundle" under .NET Runtime
    echo.
)

echo [5/5] Restarting IIS...
iisreset

echo.
echo ========================================
echo Fix complete!
echo ========================================
echo.
echo Try accessing your site now:
echo   http://localhost:5000
echo   http://localhost/BSE-PAYMENTS
echo.
echo If still not working, check:
echo   1. IIS-SETUP.md for detailed instructions
echo   2. Logs in: %DEPLOY_PATH%\logs
echo   3. Event Viewer - Windows Logs - Application
echo.
pause
