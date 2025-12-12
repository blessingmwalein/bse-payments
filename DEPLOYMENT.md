# Deployment Guide

## Prerequisites
- .NET 8.0 Runtime installed on server
- SQL Server accessible from deployment server
- IIS or reverse proxy (nginx/Apache) configured

## Configuration Steps

### 1. Update Connection Strings

Edit `appsettings.Production.json` with your production database credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=BSEPayments;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true",
    "BoConnection": "Server=YOUR_SERVER_NAME;Database=BO;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Connection String Options:**

**Windows Authentication (if server is on domain):**
```
Server=YOUR_SERVER;Database=BSEPayments;Integrated Security=True;TrustServerCertificate=True
```

**SQL Server Authentication:**
```
Server=YOUR_SERVER;Database=BSEPayments;User Id=sa;Password=YourPassword;TrustServerCertificate=True
```

**Remote Server with Port:**
```
Server=192.168.1.100,1433;Database=BSEPayments;User Id=bse_user;Password=SecurePass123;TrustServerCertificate=True
```

### 2. Build for Production

Run the following command to create a production build:

```bash
dotnet publish -c Release -o ./publish
```

This creates a self-contained deployment in the `./publish` folder.

### 3. Database Setup

Ensure both databases exist on your production SQL Server:

**BSEPayments Database:**
```sql
-- Run Migrations/InitialCreate.sql
-- Run Migrations/AddCdsNumberAndPostedFlag.sql
-- Run Data/SeedData.cs (automatic on first run)
```

**BO Database:**
- Should already exist with CashTrans table

### 4. Deploy Files

Copy the contents of `./publish` folder to your server:
- Windows Server: `C:\inetpub\bse-payments\`
- Linux Server: `/var/www/bse-payments/`

### 5. Configure Web Server

**Option A: IIS (Windows)**

1. Create new Application Pool:
   - Name: BSEPaymentsPool
   - .NET CLR Version: No Managed Code
   - Managed Pipeline Mode: Integrated

2. Create new Website:
   - Site Name: BSE Payments API
   - Physical Path: `C:\inetpub\bse-payments`
   - Application Pool: BSEPaymentsPool
   - Binding: http/*:5000 or https/*:443

3. Install ASP.NET Core Hosting Bundle:
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0

**Option B: Kestrel with Reverse Proxy (Linux)**

1. Create systemd service `/etc/systemd/system/bse-payments.service`:

```ini
[Unit]
Description=BSE Payment Service
After=network.target

[Service]
WorkingDirectory=/var/www/bse-payments
ExecStart=/usr/bin/dotnet /var/www/bse-payments/bse-payments.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=bse-payments
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

2. Enable and start service:
```bash
sudo systemctl enable bse-payments
sudo systemctl start bse-payments
sudo systemctl status bse-payments
```

3. Configure nginx reverse proxy `/etc/nginx/sites-available/bse-payments`:

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### 6. Environment Variables (Alternative to appsettings.json)

For better security, use environment variables instead of storing credentials in files:

**Windows:**
```cmd
setx ConnectionStrings__DefaultConnection "Server=...;Database=BSEPayments;..."
setx ConnectionStrings__BoConnection "Server=...;Database=BO;..."
```

**Linux:**
```bash
export ConnectionStrings__DefaultConnection="Server=...;Database=BSEPayments;..."
export ConnectionStrings__BoConnection="Server=...;Database=BO;..."
```

### 7. Firewall Configuration

Ensure the following ports are open:
- **5000** (HTTP) or **443** (HTTPS) - API access
- **1433** - SQL Server (if remote)

### 8. Test Deployment

```bash
# Health check
curl http://your-server:5000/api/payments/status

# Check Swagger UI (if enabled)
http://your-server:5000/swagger
```

### 9. Security Checklist

- [ ] Change default connection strings
- [ ] Use SQL Server authentication with strong passwords
- [ ] Enable HTTPS/SSL certificates
- [ ] Restrict CORS to specific origins (update Program.cs)
- [ ] Set up firewall rules
- [ ] Enable SQL Server encryption
- [ ] Regular database backups
- [ ] Monitor logs for errors

### 10. Monitoring

**View Logs:**

Windows:
```
C:\inetpub\bse-payments\logs\
```

Linux:
```bash
sudo journalctl -u bse-payments -f
```

**Common Issues:**

1. **Database Connection Failed**
   - Check connection strings
   - Verify SQL Server allows remote connections
   - Check firewall rules

2. **Permission Denied**
   - Ensure IIS/service user has read access to files
   - Check SQL Server user permissions

3. **Port Already in Use**
   - Change port in appsettings.json or launchSettings.json
   - Kill existing process using the port

## Quick Deploy Script (Windows)

```cmd
@echo off
echo Building BSE Payment Service...
dotnet publish -c Release -o ./publish

echo.
echo Build complete! Files are in ./publish folder
echo.
echo Next steps:
echo 1. Update appsettings.Production.json with your database credentials
echo 2. Copy ./publish folder to your server
echo 3. Configure IIS or run: dotnet bse-payments.dll
echo.
pause
```

## Quick Deploy Script (Linux)

```bash
#!/bin/bash
echo "Building BSE Payment Service..."
dotnet publish -c Release -o ./publish

echo ""
echo "Build complete! Files are in ./publish folder"
echo ""
echo "Next steps:"
echo "1. Update appsettings.Production.json with your database credentials"
echo "2. Copy ./publish folder to /var/www/bse-payments"
echo "3. Set up systemd service and nginx"
echo ""
```
