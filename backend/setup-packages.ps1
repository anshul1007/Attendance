# Setup Script for Backend .NET API
Write-Host "Installing required NuGet packages..." -ForegroundColor Green

# Check if dotnet-ef is installed
Write-Host "`nChecking for Entity Framework Core tools..." -ForegroundColor Yellow
$efInstalled = dotnet tool list --global | Select-String "dotnet-ef"

if (-not $efInstalled) {
    Write-Host "Installing dotnet-ef tools globally..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef --ignore-failed-sources
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ dotnet-ef installed successfully!" -ForegroundColor Green
    } else {
        Write-Host "⚠ Warning: Could not install dotnet-ef. You may need to install it manually." -ForegroundColor Yellow
    }
} else {
    Write-Host "✓ dotnet-ef is already installed" -ForegroundColor Green
}

# Navigate to project directory
Set-Location -Path "E:\Attendance\backend\AttendanceAPI"

# Entity Framework Core packages
Write-Host "`nInstalling Entity Framework Core packages..." -ForegroundColor Yellow
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.11
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.11
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.11
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.0.11

# Authentication packages
Write-Host "`nInstalling Authentication packages..." -ForegroundColor Yellow
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.11
dotnet add package System.IdentityModel.Tokens.Jwt --version 8.1.2

# Password Hashing
Write-Host "`nInstalling BCrypt for password hashing..." -ForegroundColor Yellow
dotnet add package BCrypt.Net-Next --version 4.0.3

# AutoMapper
Write-Host "`nInstalling AutoMapper..." -ForegroundColor Yellow
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection --version 12.0.1

# Validation
Write-Host "`nInstalling FluentValidation..." -ForegroundColor Yellow
dotnet add package FluentValidation.AspNetCore --version 11.3.0

# Logging
Write-Host "`nInstalling Serilog..." -ForegroundColor Yellow
dotnet add package Serilog.AspNetCore --version 8.0.3
dotnet add package Serilog.Sinks.Console --version 6.0.0
dotnet add package Serilog.Sinks.File --version 6.0.0

# Restore packages
Write-Host "`nRestoring packages..." -ForegroundColor Yellow
dotnet restore

Write-Host "`n✓ All packages installed successfully!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Update connection string in appsettings.json (Azure password)" -ForegroundColor White
Write-Host "2. Implement Entity Models in Models/Entities/" -ForegroundColor White
Write-Host "3. Create ApplicationDbContext in Data/" -ForegroundColor White
Write-Host "4. Run: dotnet ef migrations add InitialCreate" -ForegroundColor White
Write-Host "5. Run: dotnet ef database update" -ForegroundColor White
Write-Host "6. Run: dotnet run" -ForegroundColor White
Write-Host "`nNote: Steps 4-5 require DbContext and Entity Models to be implemented first!" -ForegroundColor Yellow
