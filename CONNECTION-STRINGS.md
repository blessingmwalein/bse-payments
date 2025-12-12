# Database Connection String Reference

## Quick Setup

Edit `appsettings.Production.json` and replace `YOUR_SERVER_NAME`, `YOUR_USERNAME`, and `YOUR_PASSWORD` with your actual values.

## Connection String Formats

### 1. SQL Server Authentication (Recommended for Production)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=BSEPayments;User Id=bse_user;Password=SecurePass123;TrustServerCertificate=True;MultipleActiveResultSets=true",
  "BoConnection": "Server=YOUR_SERVER;Database=BO;User Id=bse_user;Password=SecurePass123;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

**Example with IP Address:**
```
Server=192.168.1.100;Database=BSEPayments;User Id=bse_user;Password=SecurePass123;TrustServerCertificate=True
```

**Example with Port:**
```
Server=192.168.1.100,1433;Database=BSEPayments;User Id=bse_user;Password=SecurePass123;TrustServerCertificate=True
```

### 2. Windows Authentication (Domain/Local)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=BSEPayments;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
  "BoConnection": "Server=YOUR_SERVER;Database=BO;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

**Alternative syntax:**
```
Server=YOUR_SERVER;Database=BSEPayments;Trusted_Connection=True;TrustServerCertificate=True
```

### 3. Named Instance

```
Server=YOUR_SERVER\SQLEXPRESS;Database=BSEPayments;User Id=sa;Password=YourPass;TrustServerCertificate=True
```

### 4. Local Development (Current Setup)

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=DESKTOP-9RRSD5S;Database=BSEPayments;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true",
  "BoConnection": "Server=DESKTOP-9RRSD5S;Database=BO;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

## Common Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| `Server` | SQL Server hostname or IP | `192.168.1.100` or `sql.company.com` |
| `Database` | Database name | `BSEPayments` |
| `User Id` | SQL Server username | `bse_user` or `sa` |
| `Password` | SQL Server password | `SecurePass123` |
| `Integrated Security=True` | Use Windows Authentication | - |
| `Trusted_Connection=True` | Same as Integrated Security | - |
| `TrustServerCertificate=True` | Skip SSL certificate validation | Required for self-signed certs |
| `MultipleActiveResultSets=true` | Allow multiple queries | Recommended |
| `Encrypt=True` | Force encrypted connection | Optional |
| `Connection Timeout=30` | Connection timeout in seconds | Default: 15 |

## Testing Connection

### Using sqlcmd (Command Line)

**SQL Authentication:**
```bash
sqlcmd -S YOUR_SERVER -U bse_user -P SecurePass123 -d BSEPayments -Q "SELECT @@VERSION"
```

**Windows Authentication:**
```bash
sqlcmd -S YOUR_SERVER -E -d BSEPayments -Q "SELECT @@VERSION"
```

### Using PowerShell

```powershell
$connectionString = "Server=YOUR_SERVER;Database=BSEPayments;User Id=bse_user;Password=SecurePass123;TrustServerCertificate=True"
$connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
try {
    $connection.Open()
    Write-Host "Connection successful!" -ForegroundColor Green
    $connection.Close()
} catch {
    Write-Host "Connection failed: $_" -ForegroundColor Red
}
```

## Security Best Practices

1. **Never commit passwords to source control**
   - Use environment variables
   - Use Azure Key Vault or similar
   - Use `.gitignore` for `appsettings.Production.json`

2. **Create dedicated SQL user**
   ```sql
   CREATE LOGIN bse_user WITH PASSWORD = 'SecurePass123';
   CREATE USER bse_user FOR LOGIN bse_user;
   
   -- Grant permissions
   USE BSEPayments;
   ALTER ROLE db_datareader ADD MEMBER bse_user;
   ALTER ROLE db_datawriter ADD MEMBER bse_user;
   
   USE BO;
   ALTER ROLE db_datareader ADD MEMBER bse_user;
   ALTER ROLE db_datawriter ADD MEMBER bse_user;
   ```

3. **Enable SQL Server encryption**
   - Force encrypted connections
   - Use valid SSL certificates

4. **Restrict network access**
   - Configure SQL Server firewall
   - Use VPN for remote access
   - Whitelist specific IP addresses

## Environment Variables (Alternative)

Instead of storing in `appsettings.json`, use environment variables:

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

**Docker:**
```yaml
environment:
  - ConnectionStrings__DefaultConnection=Server=...;Database=BSEPayments;...
  - ConnectionStrings__BoConnection=Server=...;Database=BO;...
```

## Troubleshooting

### Error: "Login failed for user"
- Check username and password
- Verify user has permissions on both databases
- Check SQL Server authentication mode (mixed mode required for SQL auth)

### Error: "A network-related or instance-specific error"
- Verify server name/IP is correct
- Check SQL Server is running
- Verify TCP/IP is enabled in SQL Server Configuration Manager
- Check firewall allows port 1433
- Test with `telnet YOUR_SERVER 1433`

### Error: "Cannot open database"
- Verify database exists
- Check user has access to the database
- Run: `EXEC sp_helpdb 'BSEPayments'`

### Error: "Certificate chain was issued by an authority that is not trusted"
- Add `TrustServerCertificate=True` to connection string
- Or install proper SSL certificate on SQL Server
