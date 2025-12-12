# .NET Version Information

## Current Project Configuration

**Target Framework:** .NET 8.0 (LTS)

## Version Compatibility

### .NET 8.0 LTS (Recommended) ✅
- **Support Until:** November 2026
- **Status:** Long Term Support (LTS)
- **Recommended for:** Production deployments
- **Download:** https://dotnet.microsoft.com/download/dotnet/8.0

**Required Runtime on Server:**
- ASP.NET Core Runtime 8.0.x
- .NET Runtime 8.0.x

### Alternative Versions

#### .NET 9.0 (Current)
- **Support Until:** May 2025 (6 months)
- **Status:** Standard Term Support (STS)
- **Not recommended for production** - short support window

#### .NET 10.0 (Preview)
- **Status:** In development, not released
- **Not recommended** - unstable, preview only

#### .NET 6.0 LTS
- **Support Until:** November 2024
- **Status:** End of support approaching
- **Not recommended** - use .NET 8.0 instead

## Installation on Server

### Windows Server

1. **Download .NET 8.0 Hosting Bundle:**
   - Visit: https://dotnet.microsoft.com/download/dotnet/8.0
   - Download: "Hosting Bundle" (includes runtime + IIS support)
   - File: `dotnet-hosting-8.0.x-win.exe`

2. **Install:**
   ```cmd
   dotnet-hosting-8.0.x-win.exe /install /quiet /norestart
   ```

3. **Restart IIS:**
   ```cmd
   net stop was /y
   net start w3svc
   ```

4. **Verify Installation:**
   ```cmd
   dotnet --list-runtimes
   ```
   Should show:
   ```
   Microsoft.AspNetCore.App 8.0.x
   Microsoft.NETCore.App 8.0.x
   ```

### Linux Server (Ubuntu/Debian)

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET 8.0 Runtime
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-8.0

# Verify
dotnet --list-runtimes
```

### Linux Server (RHEL/CentOS)

```bash
# Add Microsoft repository
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm

# Install .NET 8.0 Runtime
sudo yum install aspnetcore-runtime-8.0

# Verify
dotnet --list-runtimes
```

## Checking Your Current Version

### On Development Machine
```cmd
dotnet --version
dotnet --list-sdks
dotnet --list-runtimes
```

### On Server (Runtime Only)
```cmd
dotnet --list-runtimes
```

## Package Versions Used

This project uses the following NuGet packages compatible with .NET 8.0:

```xml
<PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.11" />
<PackageReference Include="NSwag.AspNetCore" Version="14.2.0" />
```

## Updating the Project

If you need to restore packages after changing versions:

```cmd
dotnet restore
dotnet build
```

## Deployment Requirements

### Minimum Server Requirements

**For .NET 8.0:**
- Windows Server 2012 R2 or later
- Windows 10 version 1607 or later
- Ubuntu 20.04 or later
- RHEL 8 or later

**Hardware:**
- CPU: 1 GHz or faster
- RAM: 512 MB minimum (2 GB recommended)
- Disk: 500 MB free space

## Troubleshooting

### Error: "You must install or update .NET to run this application"

**Solution:** Install .NET 8.0 Runtime on the server
```
Download from: https://dotnet.microsoft.com/download/dotnet/8.0
Select: ASP.NET Core Runtime 8.0.x (Hosting Bundle for Windows)
```

### Error: "The framework 'Microsoft.AspNetCore.App', version '8.0.x' was not found"

**Solution:** Install ASP.NET Core Runtime
```cmd
# Windows
Download and install Hosting Bundle

# Linux
sudo apt-get install aspnetcore-runtime-8.0
```

### Checking if .NET is Installed

```cmd
# Check SDK (development)
dotnet --list-sdks

# Check Runtime (production)
dotnet --list-runtimes

# Check version
dotnet --version
```

## Support Lifecycle

| Version | Release Date | End of Support | Type |
|---------|-------------|----------------|------|
| .NET 8.0 | Nov 2023 | Nov 2026 | LTS ✅ |
| .NET 9.0 | Nov 2024 | May 2025 | STS |
| .NET 7.0 | Nov 2022 | May 2024 | STS (Ended) |
| .NET 6.0 | Nov 2021 | Nov 2024 | LTS (Ending) |

**LTS** = Long Term Support (3 years)
**STS** = Standard Term Support (18 months)

## Recommendation

✅ **Use .NET 8.0 LTS** for this project
- Stable and production-ready
- Long-term support until 2026
- Best performance and security updates
- Wide compatibility with hosting providers
