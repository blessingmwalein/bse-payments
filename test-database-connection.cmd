@echo off
echo ========================================
echo BSE Payment Service - Database Test
echo ========================================
echo.

echo Testing SQL Server Connection...
echo.

REM Get server name from appsettings.json
echo [1] Testing BSEPayments Database Connection
echo.

REM Try to connect using sqlcmd
sqlcmd -S DESKTOP-9RRSD5S -d BSEPayments -E -Q "SELECT @@VERSION AS 'SQL Server Version', DB_NAME() AS 'Database'" -W

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [SUCCESS] BSEPayments database connection OK
    echo.
) else (
    echo.
    echo [FAILED] Cannot connect to BSEPayments database
    echo.
    echo Possible issues:
    echo - SQL Server is not running
    echo - Database 'BSEPayments' does not exist
    echo - Windows Authentication failed
    echo - SQL Server not accepting remote connections
    echo.
)

echo [2] Testing BO Database Connection
echo.

sqlcmd -S DESKTOP-9RRSD5S -d BO -E -Q "SELECT @@VERSION AS 'SQL Server Version', DB_NAME() AS 'Database'" -W

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [SUCCESS] BO database connection OK
    echo.
) else (
    echo.
    echo [FAILED] Cannot connect to BO database
    echo.
)

echo [3] Testing CashTrans Table
echo.

sqlcmd -S DESKTOP-9RRSD5S -d BO -E -Q "SELECT TOP 1 * FROM CashTrans" -W

if %ERRORLEVEL% EQU 0 (
    echo.
    echo [SUCCESS] CashTrans table exists and is accessible
    echo.
) else (
    echo.
    echo [FAILED] Cannot access CashTrans table
    echo.
)

echo ========================================
echo Test Complete
echo ========================================
echo.
echo If any tests failed, check:
echo 1. SQL Server is running (services.msc)
echo 2. Databases exist (use SSMS)
echo 3. Connection strings in appsettings.Production.json
echo 4. IIS Application Pool identity has database access
echo.
pause
