# Employee Attendance Management System

A comprehensive attendance and leave management system with role-based access control.

> **🚀 New to this project?** Start with the [Quick Start Guide](QUICK_START.md) or [Project Summary](PROJECT_SUMMARY.md)

## 🎯 Overview

This system provides a complete solution for managing employee attendance, leave requests, and holiday tracking with separate interfaces for Employees, Managers, and Administrators.

## 📚 Documentation

- **[QUICK_START.md](QUICK_START.md)** - 5-minute setup guide
- **[SETUP_GUIDE.md](SETUP_GUIDE.md)** - Complete implementation guide
- **[PROJECT_SUMMARY.md](PROJECT_SUMMARY.md)** - Project status & features
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)** - Common issues

### Technical Docs
- [Architecture](docs/ARCHITECTURE.md) - System design
- [Database](docs/DATABASE.md) - PostgreSQL schema
- [API](docs/API.md) - Endpoints & authentication
- [Deployment](docs/DEPLOYMENT.md) - Azure, AWS, Docker
- [Flows](docs/diagrams/FLOWS.md) - Process diagrams

## 📁 Project Structure

```
Attendance/
├── frontend/              # Angular 18 Application
│   ├── src/app/          # Application code
│   └── package.json      # Dependencies
│
├── backend/               # .NET 8 Web API
│   ├── AttendanceAPI/    # API Project
│   ├── setup-packages.ps1    # Package installation script
│   └── start-api.ps1         # Quick start script
│
├── docs/                  # Technical Documentation
│   ├── ARCHITECTURE.md   # System design
│   ├── DATABASE.md       # PostgreSQL schema
│   ├── API.md            # API endpoints
│   ├── DEPLOYMENT.md     # Deployment guides
│   └── diagrams/         # Flow diagrams
│
├── scripts/               # Utility Scripts
│   ├── start-all.ps1     # Start both apps
│   └── test-azure-connection.ps1    # Test database
│
├── archive/               # Legacy/Reference Files
│   └── DATABASE_SQLSERVER.md        # SQL Server version
│
├── .gitignore             # Git ignore rules
├── .gitattributes         # Git line endings
├── README.md              # This file
├── QUICK_START.md         # 5-minute setup
├── SETUP_GUIDE.md         # Complete guide
├── PROJECT_SUMMARY.md     # Project status
└── TROUBLESHOOTING.md     # Common issues
```

## � Version Control

This project uses Git for version control:

```powershell
# Clone repository (if from remote)
git clone <repository-url>

# Check status
git status

# View what's ignored
git check-ignore -v backend/bin/
```

**Important:** Sensitive files are automatically excluded via `.gitignore`:
- Connection strings with real passwords
- Build outputs (bin/, obj/, node_modules/)
- User-specific IDE settings

See **[GIT_SETUP.md](GIT_SETUP.md)** for detailed Git configuration.

## �🚀 Quick Start

### Prerequisites

- **Frontend**: Node.js 18+ and npm
- **Backend**: .NET 8 SDK
- **Database**: Azure PostgreSQL (configured)
- **Git**: For version control

### Frontend Setup

```bash
cd frontend
npm install
npm start
```

The application will run on `http://localhost:4200`

### Backend Setup

```powershell
cd backend
.\setup-packages.ps1  # Install packages & EF tools
.\start-api.ps1       # Start API
```

API: `http://localhost:5146`  
Swagger: `http://localhost:5146/swagger`

## 👥 User Roles

### Employee
- Daily login/logout for attendance tracking
- View attendance history
- Raise leave requests (Casual & Earned)
- View leave balance
- Track compensatory off for weekend/holiday work

### Manager
- All employee features
- Approve/reject team leave requests
- Approve/reject team attendance
- View team attendance reports

### Administrator
- Create and manage user profiles
- Allocate holiday entitlements
- Manage public holidays
- System-wide reports and analytics
- User role management

## 🎨 Features

### Attendance Management
- ✅ Manual login/logout with system timestamp
- ✅ Automatic detection of weekends/public holidays
- ✅ Compensatory off tracking for holiday work
- ✅ Attendance history and reports

### Leave Management
- ✅ Casual Leave requests
- ✅ Earned Leave requests
- ✅ Leave balance tracking
- ✅ Approval workflow (Manager → Approval)

### Admin Features
- ✅ User profile creation
- ✅ Holiday calendar management
- ✅ Leave entitlement allocation
- ✅ Role-based access control

## 📊 Technology Stack

### Frontend
- **Framework**: Angular 18
- **UI Library**: Angular Material
- **State Management**: NgRx (optional) / Services
- **HTTP Client**: Angular HttpClient
- **Authentication**: JWT Token-based

### Backend
- **Framework**: .NET 8 Web API
- **ORM**: Entity Framework Core 8
- **Authentication**: JWT Bearer Authentication
- **Database**: SQL Server / PostgreSQL
- **API Documentation**: Swagger/OpenAPI

## 📖 Documentation

- [Architecture Overview](docs/ARCHITECTURE.md)
- [API Documentation](docs/API.md)
- [Database Schema](docs/DATABASE.md)
- [Deployment Guide](docs/DEPLOYMENT.md)
- [User Guide](docs/USER_GUIDE.md)

## 🔐 Security

- JWT-based authentication
- Role-based authorization
- Secure password hashing (BCrypt)
- HTTPS enforcement
- CORS configuration

## 📦 Independent Deployment

Both frontend and backend are designed to be deployed independently:

### Frontend Deployment
- Build: `npm run build`
- Deploy to: Azure Static Web Apps, Netlify, Vercel, or any static hosting
- Environment configuration via `environment.ts`

### Backend Deployment
- Build: `dotnet publish -c Release`
- Deploy to: Azure App Service, AWS Elastic Beanstalk, Docker, or IIS
- Configuration via `appsettings.json`

## 📝 License

MIT License

## 👨‍💻 Contributing

Please read [CONTRIBUTING.md](docs/CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

---

**Version**: 1.0.0  
**Last Updated**: October 15, 2025
