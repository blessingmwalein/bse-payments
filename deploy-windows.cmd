@echo off
echo ========================================
echo BSE Payment Service - Build for Deploy
echo ========================================
echo.

echo [1/3] Cleaning previous build...
if exist ".\publish" rmdir /s /q ".\publish"

echo [2/3] Building Release version...
dotnet publish -c Release -o ./publish --self-contained false

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo [2.5/3] Ensuring web.config is present...
if not exist ".\publish\web.config" (
    copy "web.config" ".\publish\web.config"
)

echo [3/3] Build complete!
echo.
echo ========================================
echo Deployment files ready in: .\publish
echo ========================================
echo.
echo NEXT STEPS:
echo 1. Edit appsettings.Production.json with your database server details
echo 2. Copy the 'publish' folder to your server
echo 3. Configure IIS or run: dotnet bse-payments.dll
echo.
echo Connection String Format:
echo   Server=YOUR_SERVER;Database=BSEPayments;User Id=USERNAME;Password=PASSWORD;TrustServerCertificate=True
echo.
echo For Windows Authentication use:
echo   Server=YOUR_SERVER;Database=BSEPayments;Integrated Security=True;TrustServerCertificate=True
echo.
pause
