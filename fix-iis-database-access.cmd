@echo off
echo ========================================
echo Fix IIS Database Access Issues
echo ========================================
echo.

echo This script will help diagnose and fix common IIS database access issues.
echo.

echo [STEP 1] Check SQL Server Service
echo.
sc query MSSQLSERVER | find "RUNNING"
if %ERRORLEVEL% EQU 0 (
    echo SQL Server is running
) else (
    echo SQL Server is NOT running!
    echo Starting SQL Server...
    net start MSSQLSERVER
)
echo.

echo [STEP 2] Check if databases exist
echo.
sqlcmd -S DESKTOP-9RRSD5S -E -Q "SELECT name FROM sys.databases WHERE name IN ('BSEPayments', 'BO')" -W
echo.

echo [STEP 3] Common Solutions
echo.
echo If the app still fails to start, try these:
echo.
echo 1. GRANT DATABASE ACCESS TO IIS USER
echo    Run this SQL command:
echo.
echo    USE BSEPayments;
echo    CREATE USER [IIS APPPOOL\BSEPaymentsPool] FOR LOGIN [IIS APPPOOL\BSEPaymentsPool];
echo    ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\BSEPaymentsPool];
echo    ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\BSEPaymentsPool];
echo.
echo    USE BO;
echo    CREATE USER [IIS APPPOOL\BSEPaymentsPool] FOR LOGIN [IIS APPPOOL\BSEPaymentsPool];
echo    ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\BSEPaymentsPool];
echo    ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\BSEPaymentsPool];
echo.
echo 2. OR USE SQL AUTHENTICATION
echo    Update appsettings.Production.json:
echo    "Server=DESKTOP-9RRSD5S;Database=BSEPayments;User Id=sa;Password=YourPassword;..."
echo.
echo 3. CHECK APPLICATION POOL IDENTITY
echo    - Open IIS Manager
echo    - Select Application Pool
echo    - Advanced Settings
echo    - Identity should be: ApplicationPoolIdentity
echo.
echo 4. VIEW DETAILED ERRORS
echo    Check logs folder: .\logs\stdout_*.log
echo.

echo [STEP 4] Create SQL Login for IIS (Optional)
echo.
echo Do you want to create a SQL login for IIS? (Y/N)
set /p choice=
if /i "%choice%"=="Y" (
    echo.
    echo Creating SQL login...
    sqlcmd -S DESKTOP-9RRSD5S -E -Q "IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'IIS APPPOOL\BSEPaymentsPool') CREATE LOGIN [IIS APPPOOL\BSEPaymentsPool] FROM WINDOWS;"
    sqlcmd -S DESKTOP-9RRSD5S -E -d BSEPayments -Q "IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'IIS APPPOOL\BSEPaymentsPool') CREATE USER [IIS APPPOOL\BSEPaymentsPool] FOR LOGIN [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\BSEPaymentsPool];"
    sqlcmd -S DESKTOP-9RRSD5S -E -d BO -Q "IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'IIS APPPOOL\BSEPaymentsPool') CREATE USER [IIS APPPOOL\BSEPaymentsPool] FOR LOGIN [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\BSEPaymentsPool];"
    echo.
    echo SQL login created. Restart IIS:
    echo iisreset
)

echo.
echo ========================================
echo.
pause
