# IIS Database Connection Troubleshooting

## HTTP Error 500.30 - Common Causes

This error means the ASP.NET Core app failed to start. Most common cause is **database connection failure**.

## Quick Diagnosis

### 1. Check the Logs

Look in the `logs` folder for detailed error messages:
```
C:\inetpub\bse-payments\logs\stdout_*.log
```

Common errors you'll see:
- "Login failed for user 'IIS APPPOOL\BSEPaymentsPool'"
- "Cannot open database 'BSEPayments'"
- "A network-related or instance-specific error occurred"

### 2. Run Test Scripts

```cmd
# Test database connectivity
test-database-connection.cmd

# Fix IIS database access
fix-iis-database-access.cmd
```

## Solutions

### Solution 1: Grant IIS Access to SQL Server (Recommended)

IIS runs under an Application Pool identity that needs database access.

**Run these SQL commands:**

```sql
-- Create login for IIS Application Pool
USE master;
GO
CREATE LOGIN [IIS APPPOOL\BSEPaymentsPool] FROM WINDOWS;
GO

-- Grant access to BSEPayments database
USE BSEPayments;
GO
CREATE USER [IIS APPPOOL\BSEPaymentsPool] FOR LOGIN [IIS APPPOOL\BSEPaymentsPool];
GO
ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\BSEPaymentsPool];
ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\BSEPaymentsPool];
GO

-- Grant access to BO database
USE BO;
GO
CREATE USER [IIS APPPOOL\BSEPaymentsPool] FOR LOGIN [IIS APPPOOL\BSEPaymentsPool];
GO
ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\BSEPaymentsPool];
ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\BSEPaymentsPool];
GO
```

**Or run this one-liner:**
```cmd
sqlcmd -S DESKTOP-9RRSD5S -E -Q "CREATE LOGIN [IIS APPPOOL\BSEPaymentsPool] FROM WINDOWS; USE BSEPayments; CREATE USER [IIS APPPOOL\BSEPaymentsPool] FOR LOGIN [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\BSEPaymentsPool]; USE BO; CREATE USER [IIS APPPOOL\BSEPaymentsPool] FOR LOGIN [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\BSEPaymentsPool];"
```

### Solution 2: Use SQL Server Authentication

Instead of Windows Authentication, use SQL credentials.

**1. Create SQL User:**
```sql
USE master;
GO
CREATE LOGIN bse_user WITH PASSWORD = 'SecurePass123!';
GO

USE BSEPayments;
GO
CREATE USER bse_user FOR LOGIN bse_user;
ALTER ROLE db_datareader ADD MEMBER bse_user;
ALTER ROLE db_datawriter ADD MEMBER bse_user;
GO

USE BO;
GO
CREATE USER bse_user FOR LOGIN bse_user;
ALTER ROLE db_datareader ADD MEMBER bse_user;
ALTER ROLE db_datawriter ADD MEMBER bse_user;
GO
```

**2. Update appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-9RRSD5S;Database=BSEPayments;User Id=bse_user;Password=SecurePass123!;TrustServerCertificate=True;MultipleActiveResultSets=true",
    "BoConnection": "Server=DESKTOP-9RRSD5S;Database=BO;User Id=bse_user;Password=SecurePass123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### Solution 3: Enable SQL Server Mixed Mode Authentication

If using SQL authentication, ensure SQL Server allows it:

1. Open **SQL Server Management Studio (SSMS)**
2. Right-click server → **Properties**
3. Go to **Security** page
4. Select **SQL Server and Windows Authentication mode**
5. Click **OK**
6. **Restart SQL Server service**

```cmd
net stop MSSQLSERVER
net start MSSQLSERVER
```

### Solution 4: Check SQL Server is Running

```cmd
# Check status
sc query MSSQLSERVER

# Start if stopped
net start MSSQLSERVER
```

### Solution 5: Verify Databases Exist

```cmd
sqlcmd -S DESKTOP-9RRSD5S -E -Q "SELECT name FROM sys.databases WHERE name IN ('BSEPayments', 'BO')"
```

If BSEPayments doesn't exist, create it:
```sql
CREATE DATABASE BSEPayments;
GO
```

Then run the migration scripts:
- `Migrations/InitialCreate.sql`
- `Migrations/AddCdsNumberAndPostedFlag.sql`

### Solution 6: Check Connection Strings

Verify `appsettings.Production.json` has correct values:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-9RRSD5S;Database=BSEPayments;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
    "BoConnection": "Server=DESKTOP-9RRSD5S;Database=BO;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### Solution 7: Restart IIS

After making changes:
```cmd
iisreset
```

Or restart just the app pool:
```cmd
# Stop
%windir%\system32\inetsrv\appcmd stop apppool /apppool.name:BSEPaymentsPool

# Start
%windir%\system32\inetsrv\appcmd start apppool /apppool.name:BSEPaymentsPool
```

## Verification Steps

### 1. Test Database Connection Manually

```cmd
# Windows Authentication
sqlcmd -S DESKTOP-9RRSD5S -d BSEPayments -E -Q "SELECT @@VERSION"

# SQL Authentication
sqlcmd -S DESKTOP-9RRSD5S -d BSEPayments -U bse_user -P SecurePass123! -Q "SELECT @@VERSION"
```

### 2. Check IIS Application Pool Identity

1. Open **IIS Manager**
2. Click **Application Pools**
3. Right-click **BSEPaymentsPool** → **Advanced Settings**
4. Check **Identity** = `ApplicationPoolIdentity`

### 3. View Detailed Error Logs

Check these locations:
- `C:\inetpub\bse-payments\logs\stdout_*.log`
- Windows Event Viewer → Application logs
- IIS logs: `C:\inetpub\logs\LogFiles`

### 4. Test the App Directly (Bypass IIS)

```cmd
cd C:\inetpub\bse-payments
dotnet bse-payments.dll
```

If it works here but not in IIS, it's a permissions issue.

## Common Error Messages

### "Login failed for user 'IIS APPPOOL\BSEPaymentsPool'"
→ Use Solution 1 (Grant IIS access)

### "Cannot open database 'BSEPayments'"
→ Database doesn't exist or user has no access
→ Use Solution 5 (Create database)

### "A network-related or instance-specific error"
→ SQL Server not running or not accessible
→ Use Solution 4 (Start SQL Server)

### "The certificate chain was issued by an authority that is not trusted"
→ Add `TrustServerCertificate=True` to connection string

## Still Not Working?

1. **Enable detailed errors** in web.config:
   ```xml
   <aspNetCore ... stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" />
   ```

2. **Check Windows Firewall** - ensure port 1433 is open

3. **Verify SQL Server TCP/IP is enabled:**
   - SQL Server Configuration Manager
   - SQL Server Network Configuration
   - Protocols for MSSQLSERVER
   - Enable TCP/IP
   - Restart SQL Server

4. **Run as Administrator** - try running IIS as admin temporarily to test

5. **Check antivirus/security software** - may be blocking connections

## Quick Fix Command

Run this to grant IIS access and restart:
```cmd
sqlcmd -S DESKTOP-9RRSD5S -E -Q "IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'IIS APPPOOL\BSEPaymentsPool') CREATE LOGIN [IIS APPPOOL\BSEPaymentsPool] FROM WINDOWS;" && sqlcmd -S DESKTOP-9RRSD5S -E -d BSEPayments -Q "IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'IIS APPPOOL\BSEPaymentsPool') CREATE USER [IIS APPPOOL\BSEPaymentsPool] FOR LOGIN [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\BSEPaymentsPool];" && sqlcmd -S DESKTOP-9RRSD5S -E -d BO -Q "IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'IIS APPPOOL\BSEPaymentsPool') CREATE USER [IIS APPPOOL\BSEPaymentsPool] FOR LOGIN [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\BSEPaymentsPool];" && iisreset
```
