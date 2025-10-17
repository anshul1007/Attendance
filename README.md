# Employee Attendance Management System

A comprehensive attendance and leave management system with role-based access control.

> **🚀 New to this project?** Start with the [Quick Start Guide](QUICK_START.md) or [Project Summary](PROJECT_SUMMARY.md)

## 🎯 Overview

This system provides a complete solution for managing employee attendance, leave requests, and holiday tracking with separate interfaces for Employees, Managers, and Administrators.

## 📚 Documentation

### Getting Started
- **[Quick Start Guide](QUICK_START.md)** - Get up and running in 5 minutes
- **[Complete Setup Guide](SETUP_GUIDE.md)** ⭐ - Azure PostgreSQL, EF Core, and database migrations
- **[Project Status](PROJECT_STATUS.md)** - File organization and current status
- **[Troubleshooting](TROUBLESHOOTING.md)** - Common issues and solutions

### Technical Documentation
- **[Documentation Index](docs/INDEX.md)** - Complete documentation navigation
- **[Architecture](docs/ARCHITECTURE.md)** - System design and patterns
- **[Database Schema](docs/DATABASE.md)** - PostgreSQL schema, tables, and indexes
- **[API Documentation](docs/API.md)** - All API endpoints
- **[Deployment Guide](docs/DEPLOYMENT.md)** - Deployment to Azure, AWS, Docker
- **[Flow Diagrams](docs/diagrams/FLOWS.md)** - Business process flows

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
├── README.md              # Project overview (you are here)
├── QUICK_START.md         # 5-minute setup
├── SETUP_GUIDE.md         # Complete setup guide
├── PROJECT_SUMMARY.md     # Current status
└── TROUBLESHOOTING.md     # Common issues
```

## 🚀 Quick Start

### Prerequisites

- **Frontend**: Node.js 18+ and npm
- **Backend**: .NET 8 SDK
- **Database**: PostgreSQL 13+ (recommended) or SQL Server 2019+

### Frontend Setup

```bash
cd frontend
npm install
npm start
```

The application will run on `http://localhost:4200`

### Backend Setup

```bash
cd backend
dotnet restore
dotnet run
```

The API will run on `https://localhost:7001` and `http://localhost:5001`

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
