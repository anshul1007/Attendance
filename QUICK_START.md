# Quick Start Guide

> **Need detailed setup?** See the [Complete Setup Guide](SETUP_GUIDE.md) for step-by-step instructions.

## 🚀 Get Started in 5 Minutes

### Prerequisites Check
```powershell
# Check Node.js (should be 18+)
node --version

# Check .NET (should be 8.0+)
dotnet --version

# Check Azure database access
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

## Step 2: Setup Backend

```powershell
cd E:\Attendance\backend
.\setup-packages.ps1
```

**Time:** ~2 minutes

---

## Step 3: Configure Azure Database

1. **Update password** in `backend/AttendanceAPI/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=attendance.postgres.database.azure.com;Port=5432;Database=postgres;Username=AttendanceDB;Password=YOUR_ACTUAL_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
   }
   ```

2. **Configure Azure firewall** (add your IP in Azure Portal)

3. **Test connection**:
   ```powershell
   .\scripts\test-azure-connection.ps1
   ```

> **Detailed instructions:** See [SETUP_GUIDE.md](SETUP_GUIDE.md) for complete database setup

---

## Step 4: Run Applications

### Terminal 1 - Backend
```powershell
# Option 1: Use the startup script (Recommended)
cd E:\Attendance\backend
.\start-api.ps1

# Option 2: Manual start
cd E:\Attendance\backend\AttendanceAPI
dotnet run
```
✅ API running at: https://localhost:7001 (check terminal for actual port)  
✅ Swagger at: https://localhost:7001/swagger

### Terminal 2 - Frontend
```powershell
# Option 1: Use the startup script (Recommended)
cd E:\Attendance\frontend
.\start-frontend.ps1

# Option 2: Manual start
cd E:\Attendance\frontend
npm start
```
✅ App running at: http://localhost:4200

### Option 3: Start Both at Once
```powershell
# This will launch both in separate windows
cd E:\Attendance
.\start-all.ps1
```

---

## 🎯 Default Login Credentials

(To be created after database setup)

**Administrator:**
- Email: `admin@company.com`
- Password: `Admin@123`

**Manager:**
- Email: `manager@company.com`
- Password: `Manager@123`

**Employee:**
- Email: `employee@company.com`
- Password: `Employee@123`

---

## 📝 What's Already Done

✅ Project structure created  
✅ Frontend Angular app initialized  
✅ Backend .NET API project created  
✅ Complete documentation  
✅ Database schema designed  
✅ API endpoints documented  
✅ Authentication & authorization setup  
✅ Core services and models  
✅ Login page & employee dashboard  
✅ Deployment guides  

---

## 📋 What Needs to Be Completed

### Backend (Priority)
1. Create entity models (User, Attendance, Leave, etc.)
2. Create DbContext and configure relationships
3. Implement controllers (Auth, Attendance, Leave, Admin)
4. Implement services with business logic
5. Configure JWT authentication in Program.cs
6. Create and apply EF Core migrations
7. Seed initial data

### Frontend (After Backend)
1. Complete employee module (attendance history, leave request)
2. Complete manager module (approvals, team reports)
3. Complete admin module (user management, holidays)
4. Add validation and error handling
5. Add loading states and animations

---

## 🔗 Important Links

| Resource | Location |
|----------|----------|
| Main README | `/README.md` |
| Architecture | `/docs/ARCHITECTURE.md` |
| Database Schema | `/docs/DATABASE.md` |
| API Documentation | `/docs/API.md` |
| Deployment Guide | `/docs/DEPLOYMENT.md` |
| Flow Diagrams | `/docs/diagrams/FLOWS.md` |
| Project Summary | `/PROJECT_SUMMARY.md` |

---

## 💡 Helpful Commands

### Frontend
```powershell
# Start dev server
npm start

# Build for production
npm run build -- --configuration production

# Run tests
npm test

# Generate component
ng generate component features/employee/attendance-history
```

### Backend
```powershell
# Run API
dotnet run

# Run with hot reload
dotnet watch run

# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Build for production
dotnet publish -c Release
```

---

## 🐛 Troubleshooting

### Frontend won't start
```powershell
# Clear npm cache and reinstall
cd frontend
Remove-Item -Recurse -Force node_modules
Remove-Item package-lock.json
npm install
```

### Backend build errors
```powershell
# Clean and restore
cd backend/AttendanceAPI
dotnet clean
dotnet restore
dotnet build
```

### Database connection failed
1. Verify SQL Server is running
2. Check connection string in appsettings.json
3. Test connection with SQL Server Management Studio

### Port already in use
- Frontend: Update `angular.json` port configuration
- Backend: Update `Properties/launchSettings.json` ports

---

## 📞 Need Help?

1. Check `/docs` folder for detailed documentation
2. Review `PROJECT_SUMMARY.md` for current status
3. Check README files in frontend and backend folders
4. Review flow diagrams in `/docs/diagrams`

---

## ✅ Next Actions

Run this command to install backend packages:
```powershell
cd E:\Attendance\backend
.\setup-packages.ps1
```

Then review:
- `/PROJECT_SUMMARY.md` - See what's done and what's next
- `/docs/ARCHITECTURE.md` - Understand the system design
- `/docs/DATABASE.md` - Review database structure

---

**Happy Coding! 🎉**
