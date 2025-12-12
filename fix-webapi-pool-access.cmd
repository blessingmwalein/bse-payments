@echo off
echo ========================================
echo Grant Database Access to WebApi Pool
echo ========================================
echo.

echo Creating SQL login for IIS APPPOOL\WebApi...
echo.

sqlcmd -S DESKTOP-9RRSD5S -E -Q "IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'IIS APPPOOL\WebApi') BEGIN CREATE LOGIN [IIS APPPOOL\WebApi] FROM WINDOWS; PRINT 'Login created successfully'; END ELSE PRINT 'Login already exists';"

echo.
echo Granting access to BSEPayments database...
echo.

sqlcmd -S DESKTOP-9RRSD5S -E -d BSEPayments -Q "IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'IIS APPPOOL\WebApi') CREATE USER [IIS APPPOOL\WebApi] FOR LOGIN [IIS APPPOOL\WebApi]; ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\WebApi]; ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\WebApi]; PRINT 'BSEPayments access granted';"

echo.
echo Granting access to BO database...
echo.

sqlcmd -S DESKTOP-9RRSD5S -E -d BO -Q "IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'IIS APPPOOL\WebApi') CREATE USER [IIS APPPOOL\WebApi] FOR LOGIN [IIS APPPOOL\WebApi]; ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\WebApi]; ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\WebApi]; PRINT 'BO database access granted';"

echo.
echo ========================================
echo Access granted successfully!
echo ========================================
echo.
echo Restarting IIS...
iisreset

echo.
echo Done! Try accessing your application now.
echo.
pause
