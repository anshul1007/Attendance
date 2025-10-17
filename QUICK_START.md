# Quick Start Guide

> **Need detailed setup?** See the [Complete Setup Guide](SETUP_GUIDE.md) for step-by-step instructions including entity models and migrations.

## 🚀 Get Started in 5 Minutes

### Prerequisites Check
```powershell
# Check Node.js (should be 18+)
node --version

# Check .NET (should be 8.0+)
dotnet --version

# Check EF Core tools
dotnet ef --version

# Check Azure database access
cd E:\Attendance
.\scripts\test-azure-connection.ps1
```

---

## Step 1: Install Frontend Dependencies

```powershell
cd E:\Attendance\frontend
npm install
```

**Time:** ~2 minutes

---

## Step 2: Install Backend Packages

```powershell
cd E:\Attendance\backend
.\setup-packages.ps1
```

This installs:
- Entity Framework Core + Npgsql (PostgreSQL)
- JWT Authentication
- BCrypt, AutoMapper, FluentValidation, Serilog
- dotnet-ef tools (if not already installed)

**Time:** ~2 minutes

---

## Step 3: Configure Azure PostgreSQL Connection

1. **Update password** in `backend/AttendanceAPI/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=attendance.postgres.database.azure.com;Port=5432;Database=postgres;Username=AttendanceDB;Password=YOUR_ACTUAL_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
   }
   ```

2. **Configure Azure firewall**:
   - Go to [Azure Portal](https://portal.azure.com)
   - Navigate to your PostgreSQL server
   - Add your client IP address in "Connection Security" or "Networking"

3. **Test connection**:
   ```powershell
   cd E:\Attendance
   .\scripts\test-azure-connection.ps1
   ```

> **Detailed instructions:** See [SETUP_GUIDE.md](SETUP_GUIDE.md) for complete Azure database setup

---

## Step 4: Implement Database Models

⚠️ **Before running the application**, you need to implement the database layer:

### Quick Implementation (Copy-Paste from SETUP_GUIDE.md)

1. **Create entity models** (6 files in `backend/AttendanceAPI/Models/Entities/`):
   - User.cs
   - Attendance.cs
   - LeaveRequest.cs
   - LeaveEntitlement.cs
   - PublicHoliday.cs
   - AuditLog.cs

2. **Create DbContext** (`backend/AttendanceAPI/Data/ApplicationDbContext.cs`)

3. **Update Program.cs** to register DbContext

**All code is ready to copy-paste from [SETUP_GUIDE.md](SETUP_GUIDE.md) Section "Implement Database Models"**

---

## Step 5: Run Database Migrations

```powershell
cd E:\Attendance\backend\AttendanceAPI

# Create migration
dotnet ef migrations add InitialCreate

# Apply to Azure database
dotnet ef database update
```

---

## Step 6: Start Applications

### Option 1: Start Both at Once (Recommended)
```powershell
cd E:\Attendance
.\scripts\start-all.ps1
```

### Option 2: Start Separately

**Terminal 1 - Backend:**
```powershell
cd E:\Attendance\backend
.\start-api.ps1
```
✅ API running at: http://localhost:5146  
✅ Swagger at: http://localhost:5146/swagger

**Terminal 2 - Frontend:**
```powershell
cd E:\Attendance\frontend
npm start
```
✅ App running at: http://localhost:4200

---

## 🎯 Default Login Credentials

⚠️ **Not yet created** - These will be created after implementing the database:

**Administrator:**
- Email: `admin@company.com`
- Password: `Admin@123`

To create the admin user, see [SETUP_GUIDE.md](SETUP_GUIDE.md) Section "Insert Sample Data"

---

## 📝 Current Project Status

✅ **Completed:**
- Project structure organized
- Frontend Angular 18 app with authentication
- Backend .NET 8 API project
- Azure PostgreSQL database configured
- Complete documentation (consolidated and organized)
- Git configuration (.gitignore, .gitattributes)
- All NuGet packages installed
- EF Core tools installed
- Startup scripts created

⏳ **Ready to Implement:**
- Entity Models (code ready in SETUP_GUIDE.md)
- DbContext (code ready in SETUP_GUIDE.md)
- Database migrations
- Controllers and services
- Sample data seeding

---

## 📋 Implementation Checklist

Use this checklist to track your progress:

### Database Layer
- [ ] Create `Models/Entities/` folder
- [ ] Copy 6 entity model files from SETUP_GUIDE.md
- [ ] Create `Data/` folder
- [ ] Copy ApplicationDbContext.cs from SETUP_GUIDE.md
- [ ] Update Program.cs with DbContext configuration
- [ ] Run `dotnet ef migrations add InitialCreate`
- [ ] Run `dotnet ef database update`
- [ ] Insert sample data (admin user, holidays)

### API Layer
- [ ] Implement AuthController (login, register)
- [ ] Implement AttendanceController (login/logout, history)
- [ ] Implement LeaveController (request, approve, list)
- [ ] Implement ManagerController (team approvals)
- [ ] Implement AdminController (user management)
- [ ] Configure JWT authentication
- [ ] Add authorization policies

### Frontend Integration
- [ ] Test login functionality
- [ ] Verify attendance tracking works
- [ ] Test leave request flow
- [ ] Complete manager approval interface
- [ ] Complete admin user management

---

## 🔗 Essential Documentation

| Document | Purpose |
|----------|---------|
| **[SETUP_GUIDE.md](SETUP_GUIDE.md)** | Complete setup with entity code |
| [README.md](README.md) | Project overview |
| [PROJECT_SUMMARY.md](PROJECT_SUMMARY.md) | Current status & features |
| [TROUBLESHOOTING.md](TROUBLESHOOTING.md) | Common issues |
| [docs/DATABASE.md](docs/DATABASE.md) | PostgreSQL schema |
| [docs/API.md](docs/API.md) | API endpoints |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | System design |

---

---

## Next Steps

1. **Run setup script:**
   ```powershell
   cd E:\Attendance\backend
   .\setup-packages.ps1
   ```

2. **Review documentation:**
   - [SETUP_GUIDE.md](SETUP_GUIDE.md) - Entity models & migrations
   - [PROJECT_SUMMARY.md](PROJECT_SUMMARY.md) - Project status
   - [TROUBLESHOOTING.md](TROUBLESHOOTING.md) - Common issues

**Happy Coding! 🎉**
