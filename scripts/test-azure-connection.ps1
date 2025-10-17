# Test Azure PostgreSQL Connection
# Replace {your-password} with your actual password

Write-Host "Testing Azure PostgreSQL Connection..." -ForegroundColor Cyan
Write-Host ""

# Connection details
$Server = "attendance.postgres.database.azure.com"
$Port = "5432"
$Database = "postgres"
$Username = "AttendanceDB"
$Password = Read-Host "Enter your Azure PostgreSQL password" -AsSecureString

# Convert SecureString to plain text for connection string
$BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password)
$PlainPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)

Write-Host "Connecting to: $Server" -ForegroundColor Yellow
Write-Host "Database: $Database" -ForegroundColor Yellow
Write-Host "Username: $Username" -ForegroundColor Yellow
Write-Host ""

# Test using psql (if available)
$psqlExists = Get-Command psql -ErrorAction SilentlyContinue

if ($psqlExists) {
    Write-Host "Testing connection with psql..." -ForegroundColor Green
    
    $env:PGPASSWORD = $PlainPassword
    $env:PGHOST = $Server
    $env:PGPORT = $Port
    $env:PGDATABASE = $Database
    $env:PGUSER = $Username
    
    # Test connection
    $result = psql -c "SELECT version();" 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Connection successful!" -ForegroundColor Green
        Write-Host ""
        Write-Host "PostgreSQL Version:" -ForegroundColor Cyan
        Write-Host $result
        Write-Host ""
        
        # Get database size
        Write-Host "Database Information:" -ForegroundColor Cyan
        psql -c "SELECT pg_size_pretty(pg_database_size('$Database')) as database_size;"
        
        # List databases
        Write-Host "`nAvailable Databases:" -ForegroundColor Cyan
        psql -c "\l"
        
    } else {
        Write-Host "❌ Connection failed!" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
    }
    
    # Clear password from environment
    Remove-Item Env:PGPASSWORD
    
} else {
    Write-Host "⚠️  psql not found. Testing with .NET..." -ForegroundColor Yellow
    Write-Host "To install psql, download PostgreSQL from https://www.postgresql.org/download/" -ForegroundColor Yellow
    Write-Host ""
    
    # Test using .NET
    $connectionString = "Host=$Server;Port=$Port;Database=$Database;Username=$Username;Password=$PlainPassword;SSL Mode=Require;Trust Server Certificate=true;Timeout=30"
    
    try {
        Add-Type -Path "C:\Program Files\dotnet\shared\Microsoft.NETCore.App\*\System.Data.Common.dll" -ErrorAction SilentlyContinue
        
        # Check if Npgsql is available
        $npgsqlPath = Get-ChildItem -Path "$HOME\.nuget\packages\npgsql\*\lib\net8.0\Npgsql.dll" -ErrorAction SilentlyContinue | Select-Object -First 1
        
        if ($npgsqlPath) {
            Add-Type -Path $npgsqlPath.FullName
            
            Write-Host "Testing connection with Npgsql..." -ForegroundColor Green
            
            $connection = New-Object Npgsql.NpgsqlConnection($connectionString)
            $connection.Open()
            
            Write-Host "✅ Connection successful!" -ForegroundColor Green
            
            $command = $connection.CreateCommand()
            $command.CommandText = "SELECT version();"
            $version = $command.ExecuteScalar()
            
            Write-Host ""
            Write-Host "PostgreSQL Version:" -ForegroundColor Cyan
            Write-Host $version
            
            $connection.Close()
            
        } else {
            Write-Host "⚠️  Npgsql not found. Please run setup-packages.ps1 first" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "Run this command:" -ForegroundColor Cyan
            Write-Host "cd E:\Attendance\backend; .\setup-packages.ps1" -ForegroundColor White
        }
        
    } catch {
        Write-Host "❌ Connection failed!" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
        Write-Host ""
        Write-Host "Common issues:" -ForegroundColor Yellow
        Write-Host "1. Check firewall rules in Azure Portal" -ForegroundColor White
        Write-Host "2. Verify your IP is whitelisted" -ForegroundColor White
        Write-Host "3. Ensure SSL mode is enabled" -ForegroundColor White
        Write-Host "4. Double-check username and password" -ForegroundColor White
    }
}

# Clear password from memory
$PlainPassword = $null
[System.GC]::Collect()

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Update appsettings.json with your actual password" -ForegroundColor White
Write-Host "2. Run: cd E:\Attendance\backend; .\setup-packages.ps1" -ForegroundColor White
Write-Host "3. Implement Entity Models" -ForegroundColor White
Write-Host "4. Run: dotnet ef migrations add InitialCreate" -ForegroundColor White
Write-Host "5. Run: dotnet ef database update" -ForegroundColor White
Write-Host ""
Write-Host "For more information, see: AZURE_DATABASE_CONFIG.md" -ForegroundColor Cyan

Read-Host "`nPress Enter to exit"
