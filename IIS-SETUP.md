# IIS Deployment Guide for BSE Payment Service

## Prerequisites

### 1. Install .NET 8.0 Hosting Bundle

Download and install from: https://dotnet.microsoft.com/download/dotnet/8.0

**Direct Link:** Look for "Hosting Bundle" under ".NET Runtime"

After installation, restart IIS:
```cmd
net stop was /y
net start w3svc
```

### 2. Verify Installation

```cmd
dotnet --list-runtimes
```

You should see:
- Microsoft.AspNetCore.App 8.x.x
- Microsoft.NETCore.App 8.x.x

## Step-by-Step IIS Setup

### Step 1: Build the Application

Run the deployment script:
```cmd
deploy-windows.cmd
```

This creates a `publish` folder with all necessary files.

### Step 2: Update Configuration

Edit `publish\appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-9RRSD5S;Database=BSEPayments;Integrated Security=True;TrustServerCertificate=True",
    "BoConnection": "Server=DESKTOP-9RRSD5S;Database=BO;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

**For SQL Authentication:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-9RRSD5S;Database=BSEPayments;User Id=bse_user;Password=YourPassword;TrustServerCertificate=True",
    "BoConnection": "Server=DESKTOP-9RRSD5S;Database=BO;User Id=bse_user;Password=YourPassword;TrustServerCertificate=True"
  }
}
```

### Step 3: Copy Files to IIS Directory

```cmd
xcopy /E /I /Y publish C:\inetpub\wwwroot\BSE-PAYMENTS
```

Or manually copy the `publish` folder contents to `C:\inetpub\wwwroot\BSE-PAYMENTS`

### Step 4: Create Logs Directory

```cmd
mkdir C:\inetpub\wwwroot\BSE-PAYMENTS\logs
```

### Step 5: Set Folder Permissions

Right-click on `C:\inetpub\wwwroot\BSE-PAYMENTS` → Properties → Security → Edit

Add permissions for:
- **IIS_IUSRS** - Read & Execute, List folder contents, Read
- **IIS AppPool\BSEPaymentsPool** - Read & Execute, List folder contents, Read

For the logs folder, also grant Write permissions:
```cmd
icacls "C:\inetpub\wwwroot\BSE-PAYMENTS\logs" /grant "IIS AppPool\BSEPaymentsPool:(OI)(CI)(M)"
```

### Step 6: Create Application Pool

Open IIS Manager → Application Pools → Add Application Pool

**Settings:**
- Name: `BSEPaymentsPool`
- .NET CLR version: `No Managed Code`
- Managed pipeline mode: `Integrated`
- Start application pool immediately: ✓

**Advanced Settings** (right-click pool → Advanced Settings):
- Identity: `ApplicationPoolIdentity` (default)
- Enable 32-Bit Applications: `False`
- Start Mode: `AlwaysRunning` (optional, for better performance)

### Step 7: Create Website or Application

#### Option A: Create New Website (Recommended)

1. Open IIS Manager → Sites → Add Website

**Settings:**
- Site name: `BSE-Payments`
- Application pool: `BSEPaymentsPool`
- Physical path: `C:\inetpub\wwwroot\BSE-PAYMENTS`
- Binding:
  - Type: `http`
  - IP address: `All Unassigned`
  - Port: `5000` (or your preferred port)
  - Host name: (leave empty or add domain)

2. Click OK

#### Option B: Create Application under Default Website

1. Open IIS Manager → Sites → Default Web Site → Add Application

**Settings:**
- Alias: `BSE-PAYMENTS`
- Application pool: `BSEPaymentsPool`
- Physical path: `C:\inetpub\wwwroot\BSE-PAYMENTS`

2. Click OK

Access at: `http://localhost/BSE-PAYMENTS`

### Step 8: Configure Application Pool Identity for Database Access

If using Windows Authentication for SQL Server:

```cmd
# Add IIS AppPool identity to SQL Server
sqlcmd -S DESKTOP-9RRSD5S -E -Q "CREATE LOGIN [IIS APPPOOL\BSEPaymentsPool] FROM WINDOWS"
sqlcmd -S DESKTOP-9RRSD5S -E -d BSEPayments -Q "CREATE USER [IIS APPPOOL\BSEPaymentsPool] FOR LOGIN [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\BSEPaymentsPool]"
sqlcmd -S DESKTOP-9RRSD5S -E -d BO -Q "CREATE USER [IIS APPPOOL\BSEPaymentsPool] FOR LOGIN [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datareader ADD MEMBER [IIS APPPOOL\BSEPaymentsPool]; ALTER ROLE db_datawriter ADD MEMBER [IIS APPPOOL\BSEPaymentsPool]"
```

### Step 9: Test the Deployment

1. Start the Application Pool (if not already started)
2. Browse to: `http://localhost:5000` (or your configured port)
3. Test API endpoint: `http://localhost:5000/api/payments/status`

### Step 10: Enable Swagger (Optional, for testing)

The Swagger UI should be available at:
- `http://localhost:5000/swagger`

## Troubleshooting

### Error 500.19 - Invalid Configuration

**Cause:** Missing or invalid web.config

**Solution:**
1. Ensure `web.config` exists in the deployment folder
2. Verify ASP.NET Core Hosting Bundle is installed
3. Restart IIS: `iisreset`

### Error 500.30 - In-Process Start Failure

**Cause:** Application failed to start

**Solution:**
1. Check logs in `C:\inetpub\wwwroot\BSE-PAYMENTS\logs\`
2. Verify connection strings in `appsettings.Production.json`
3. Test database connectivity
4. Check Event Viewer → Windows Logs → Application

### Error 500.31 - Failed to Load Runtime

**Cause:** .NET Runtime not found

**Solution:**
1. Install .NET 8.0 Hosting Bundle
2. Restart IIS: `iisreset`
3. Verify: `dotnet --list-runtimes`

### Error 403 - Forbidden

**Cause:** Permission issues

**Solution:**
```cmd
icacls "C:\inetpub\wwwroot\BSE-PAYMENTS" /grant "IIS_IUSRS:(OI)(CI)(RX)"
icacls "C:\inetpub\wwwroot\BSE-PAYMENTS" /grant "IIS AppPool\BSEPaymentsPool:(OI)(CI)(RX)"
icacls "C:\inetpub\wwwroot\BSE-PAYMENTS\logs" /grant "IIS AppPool\BSEPaymentsPool:(OI)(CI)(M)"
```

### Database Connection Failed

**Check connection string:**
```cmd
# Test SQL connection
sqlcmd -S DESKTOP-9RRSD5S -E -d BSEPayments -Q "SELECT @@VERSION"
```

**For Windows Auth, verify AppPool identity has access:**
```sql
-- Check if login exists
SELECT * FROM sys.server_principals WHERE name = 'IIS APPPOOL\BSEPaymentsPool'

-- Check database access
USE BSEPayments
SELECT * FROM sys.database_principals WHERE name = 'IIS APPPOOL\BSEPaymentsPool'
```

### Application Won't Start

**Enable detailed errors:**

Edit `web.config`, change:
```xml
<aspNetCore ... stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" ...>
```

Check logs in `C:\inetpub\wwwroot\BSE-PAYMENTS\logs\`

**Or temporarily enable developer exception page:**

Edit `appsettings.Production.json`:
```json
{
  "DetailedErrors": true
}
```

### Port Already in Use

**Find what's using the port:**
```cmd
netstat -ano | findstr :5000
```

**Kill the process:**
```cmd
taskkill /PID <process_id> /F
```

**Or change the port in IIS binding**

## Quick Commands Reference

```cmd
# Restart IIS
iisreset

# Stop/Start specific site
%windir%\system32\inetsrv\appcmd stop site "BSE-Payments"
%windir%\system32\inetsrv\appcmd start site "BSE-Payments"

# Stop/Start Application Pool
%windir%\system32\inetsrv\appcmd stop apppool "BSEPaymentsPool"
%windir%\system32\inetsrv\appcmd start apppool "BSEPaymentsPool"

# View Application Pool status
%windir%\system32\inetsrv\appcmd list apppool

# View logs
type C:\inetpub\wwwroot\BSE-PAYMENTS\logs\stdout_*.log

# Test database connection
sqlcmd -S DESKTOP-9RRSD5S -E -d BSEPayments -Q "SELECT 1"
```

## Performance Tuning

### Enable Application Initialization

In IIS Manager → Application Pools → BSEPaymentsPool → Advanced Settings:
- Start Mode: `AlwaysRunning`

In IIS Manager → Sites → BSE-Payments → Advanced Settings:
- Preload Enabled: `True`

### Configure Recycling

Application Pools → BSEPaymentsPool → Recycling:
- Disable time-based recycling if not needed
- Set specific times for recycling (e.g., 3 AM)

## Security Checklist

- [ ] Use HTTPS (install SSL certificate)
- [ ] Restrict CORS in Program.cs to specific origins
- [ ] Use SQL authentication with strong passwords
- [ ] Enable request filtering in IIS
- [ ] Set up IP restrictions if needed
- [ ] Regular Windows Updates
- [ ] Monitor logs for suspicious activity
- [ ] Backup databases regularly

## Monitoring

### View Real-time Logs

```cmd
powershell Get-Content C:\inetpub\wwwroot\BSE-PAYMENTS\logs\stdout_*.log -Wait -Tail 50
```

### Event Viewer

Windows Logs → Application → Filter by "IIS AspNetCore Module"

### Performance Monitor

Monitor:
- ASP.NET Core → Requests/sec
- Process → % Processor Time (w3wp.exe)
- Memory → Available MBytes
