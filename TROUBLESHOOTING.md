# Troubleshooting Guide

## Common Issues and Solutions

### Backend Issues

#### 1. "Couldn't find a project to run"

**Problem:** Running `dotnet run` from wrong directory

**Solution:**
```powershell
# Make sure you're in the correct directory
cd E:\Attendance\backend\AttendanceAPI
dotnet run
```

**Or use the startup script:**
```powershell
cd E:\Attendance\backend
.\start-api.ps1
```

---

#### 2. Port Already in Use

**Problem:** Backend port 5146 (or similar) is already in use

**Solution:**
```powershell
# Option 1: Kill the process using the port
netstat -ano | findstr :5146
taskkill /PID <process_id> /F

# Option 2: Change the port in launchSettings.json
# Edit: backend/AttendanceAPI/Properties/launchSettings.json
```

---

#### 3. NuGet Package Restore Errors

**Problem:** Custom package sources causing errors

**Solution:**
```powershell
# Use the provided setup script
cd E:\Attendance\backend
.\setup-packages.ps1

# Or manually clear and restore
cd AttendanceAPI
dotnet nuget locals all --clear
dotnet restore
```

---

#### 4. Database Connection Failed

**Problem:** Cannot connect to SQL Server

**Solution:**
```powershell
# Check if SQL Server is running
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# Update connection string in appsettings.json
# For local SQL Server:
"Server=localhost;Database=AttendanceDB;Trusted_Connection=True;TrustServerCertificate=True"

# For SQL Server with credentials:
"Server=localhost;Database=AttendanceDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
```

---

### Frontend Issues

#### 1. "npm: command not found" or "ng: command not found"

**Problem:** Node.js or Angular CLI not installed

**Solution:**
```powershell
# Check Node.js version (should be 18+)
node --version

# If not installed, download from: https://nodejs.org

# Install Angular CLI globally
npm install -g @angular/cli@latest
```

---

#### 2. Port 4200 Already in Use

**Problem:** Another Angular app is running on port 4200

**Solution:**
```powershell
# Option 1: Kill the process
netstat -ano | findstr :4200
taskkill /PID <process_id> /F

# Option 2: Use a different port
ng serve --port 4300

# Option 3: Stop existing server
# Press Ctrl+C in the terminal running ng serve
```

---

#### 3. Module Not Found Errors

**Problem:** Dependencies not installed or corrupted

**Solution:**
```powershell
cd E:\Attendance\frontend

# Delete node_modules and package-lock.json
Remove-Item -Recurse -Force node_modules
Remove-Item package-lock.json

# Reinstall
npm install

# If still having issues
npm cache clean --force
npm install
```

---

#### 4. Compilation Errors

**Problem:** TypeScript or Angular compilation errors

**Solution:**
```powershell
# Clear Angular cache
ng cache clean

# Rebuild
ng build

# If using strict mode, you may need to update tsconfig.json
```

---

### Database Issues

#### 1. Migrations Not Applied

**Problem:** Database schema doesn't exist

**Solution:**
```powershell
cd E:\Attendance\backend\AttendanceAPI

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply to database
dotnet ef database update

# If errors, check connection string in appsettings.json
```

---

#### 2. "A network-related or instance-specific error occurred"

**Problem:** Cannot connect to SQL Server

**Solution:**
1. **Verify SQL Server is running:**
   ```powershell
   Get-Service MSSQLSERVER
   # Should show "Running"
   ```

2. **Enable TCP/IP:**
   - Open SQL Server Configuration Manager
   - SQL Server Network Configuration → Protocols
   - Enable TCP/IP
   - Restart SQL Server service

3. **Check firewall:**
   ```powershell
   # Allow SQL Server through firewall
   New-NetFirewallRule -DisplayName "SQL Server" -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
   ```

---

#### 3. Database Already Exists

**Problem:** Trying to create database that exists

**Solution:**
```powershell
# Drop and recreate
dotnet ef database drop --force
dotnet ef database update

# Or connect manually and drop tables
```

---

### Authentication Issues

#### 1. 401 Unauthorized Error

**Problem:** Token not being sent or expired

**Solution:**
- **Check browser console** for token
- **Clear localStorage:**
  ```javascript
  localStorage.clear()
  ```
- **Login again** to get new token
- **Check interceptor** is adding Authorization header

---

#### 2. CORS Errors

**Problem:** Cross-origin requests blocked

**Solution:**
Backend `Program.cs` should have:
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// And use it:
app.UseCors();
```

---

### IDE/Editor Issues

#### 1. VS Code Not Recognizing Angular

**Problem:** IntelliSense not working

**Solution:**
- Install **Angular Language Service** extension
- Reload VS Code window: `Ctrl+Shift+P` → "Reload Window"

---

#### 2. TypeScript Errors in VS Code

**Problem:** Red squiggly lines everywhere

**Solution:**
```powershell
# Use workspace TypeScript version
# In VS Code: Ctrl+Shift+P → "TypeScript: Select TypeScript Version" → "Use Workspace Version"

# Or reinstall
cd frontend
npm install typescript@latest
```

---

## Quick Fixes

### Reset Everything

```powershell
# Frontend
cd E:\Attendance\frontend
Remove-Item -Recurse -Force node_modules
Remove-Item package-lock.json
npm install

# Backend
cd E:\Attendance\backend\AttendanceAPI
dotnet clean
dotnet restore
dotnet build

# Database
dotnet ef database drop --force
dotnet ef database update
```

---

### Check Services

```powershell
# Check if frontend is running
curl http://localhost:4200

# Check if backend is running
curl https://localhost:7001/swagger

# Check SQL Server
Get-Service MSSQLSERVER
```

---

### Verify Installation

```powershell
# Node.js
node --version  # Should be 18+

# npm
npm --version

# Angular CLI
ng version

# .NET
dotnet --version  # Should be 8.0+

# SQL Server
sqlcmd -S localhost -E -Q "SELECT @@VERSION"
```

---

## Error Messages Reference

| Error | Likely Cause | Solution |
|-------|-------------|----------|
| "Couldn't find a project to run" | Wrong directory | `cd AttendanceAPI` |
| "Port already in use" | Another app running | Kill process or change port |
| "Module not found" | Missing dependencies | `npm install` |
| "Cannot connect to database" | SQL Server not running | Start SQL Server service |
| "401 Unauthorized" | No/invalid token | Login again |
| "CORS policy" | Backend CORS not configured | Check Program.cs CORS setup |
| "Command not found" | Tool not installed | Install Node.js, .NET, or Angular CLI |

---

## Getting Help

1. **Check logs:**
   - Frontend: Browser Developer Console (F12)
   - Backend: Terminal output
   - SQL Server: SQL Server Error Logs

2. **Enable verbose logging:**
   - Frontend: Check Network tab in DevTools
   - Backend: Set log level to "Debug" in appsettings.json

3. **Common log locations:**
   - Backend logs: Check terminal output
   - SQL Server logs: `C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\Log`

---

## Useful Commands

```powershell
# Frontend
ng serve                          # Start dev server
ng build                          # Build for production
ng cache clean                    # Clear Angular cache
npm cache clean --force           # Clear npm cache

# Backend
dotnet run                        # Run API
dotnet watch run                  # Run with hot reload
dotnet clean                      # Clean build artifacts
dotnet ef migrations add <name>   # Create migration
dotnet ef database update         # Apply migrations
dotnet ef database drop           # Drop database

# Process Management
Get-Process node                  # List Node processes
Stop-Process -Name node           # Stop all Node processes
Get-Process dotnet               # List .NET processes
Stop-Process -Name dotnet        # Stop all .NET processes

# Port Management
netstat -ano | findstr :4200     # Find process on port 4200
netstat -ano | findstr :5146     # Find process on port 5146
taskkill /PID <id> /F            # Kill process by ID
```

---

## Prevention Tips

✅ Always use the startup scripts  
✅ Keep dependencies updated  
✅ Commit working code regularly  
✅ Use version control  
✅ Test after each change  
✅ Read error messages carefully  
✅ Check documentation first  

---

**Last Updated:** October 15, 2025
