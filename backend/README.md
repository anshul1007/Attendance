# Backend API# Attendance Management - Backend API



.NET 8 Web API with Azure PostgreSQL..NET 8 Web API for Employee Attendance and Leave Management System.



## Quick Start## Prerequisites



```powershell- .NET 8 SDK or higher

# Install all packages- SQL Server 2019+ or PostgreSQL 14+

.\setup-packages.ps1- Visual Studio 2022 or VS Code



# Start API## Installation

.\start-api.ps1

```### 1. Restore NuGet Packages



**API:** http://localhost:5146  ```powershell

**Swagger:** http://localhost:5146/swaggercd backend/AttendanceAPI

dotnet restore

## Implementation```



See [SETUP_GUIDE.md](../SETUP_GUIDE.md) for:### 2. Install Required Packages

- Entity models

- DbContext setup  ```powershell

- Database migrations# Entity Framework Core

- Controller implementationdotnet add package Microsoft.EntityFrameworkCore

dotnet add package Microsoft.EntityFrameworkCore.SqlServer

## Configurationdotnet add package Microsoft.EntityFrameworkCore.Tools

dotnet add package Microsoft.EntityFrameworkCore.Design

Update `AttendanceAPI/appsettings.json`:

- Azure PostgreSQL password# Authentication

- JWT secret keydotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

dotnet add package System.IdentityModel.Tokens.Jwt

# Password Hashing
dotnet add package BCrypt.Net-Next

# Auto Mapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

# Validation
dotnet add package FluentValidation.AspNetCore

# Logging
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

### 3. Update Database Connection

Update `appsettings.json` with your database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AttendanceDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 4. Apply Migrations

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Development

### Run the API

```powershell
cd AttendanceAPI
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7001` (or check terminal for actual port)
- HTTP: `http://localhost:5001` (or check terminal for actual port)
- Swagger: `https://localhost:7001/swagger`

**Note:** Make sure you're in the `AttendanceAPI` folder when running `dotnet run`

### Watch Mode (Auto-reload)

```powershell
dotnet watch run
```

## Project Structure

```
AttendanceAPI/
├── Controllers/          # API Controllers
│   ├── AuthController.cs
│   ├── AttendanceController.cs
│   ├── LeaveController.cs
│   ├── AdminController.cs
│   └── UserController.cs
├── Models/
│   ├── Entities/         # Database entities
│   ├── DTOs/             # Data Transfer Objects
│   └── ViewModels/       # API response models
├── Services/
│   ├── Interfaces/       # Service interfaces
│   └── Implementations/  # Service implementations
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Migrations/       # EF Core migrations
├── Middleware/           # Custom middleware
├── Helpers/              # Utility classes
├── appsettings.json      # Configuration
└── Program.cs            # Application entry point
```

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/logout` - User logout
- `POST /api/auth/refresh` - Refresh token

### Attendance
- `POST /api/attendance/login` - Record login
- `POST /api/attendance/logout` - Record logout
- `GET /api/attendance/my-attendance` - Get user attendance
- `GET /api/attendance/today` - Today's status

### Leave
- `POST /api/leave/request` - Create leave request
- `GET /api/leave/my-requests` - Get user leave requests
- `GET /api/leave/balance` - Get leave balance
- `DELETE /api/leave/{id}` - Cancel leave request

### Manager
- `GET /api/manager/team-attendance` - Team attendance
- `POST /api/manager/approve-attendance/{id}` - Approve attendance
- `GET /api/manager/pending-leave-requests` - Pending leaves
- `POST /api/manager/approve-leave/{id}` - Approve leave

### Admin
- `POST /api/admin/users` - Create user
- `GET /api/admin/users` - Get all users
- `POST /api/admin/holidays` - Create holiday
- `POST /api/admin/leave-entitlements` - Allocate leaves

## Configuration

### JWT Settings

```json
{
  "Jwt": {
    "Secret": "your-super-secret-key-min-32-characters-long",
    "Issuer": "AttendanceAPI",
    "Audience": "AttendanceApp",
    "ExpirationMinutes": 60
  }
}
```

### CORS Settings

```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200"]
  }
}
```

## Database Schema

See [Database Documentation](../../docs/DATABASE.md) for complete schema details.

## Testing

```powershell
# Run unit tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## Build for Production

```powershell
dotnet publish -c Release -o ./publish
```

## Docker

Build and run with Docker:

```powershell
docker build -t attendance-api .
docker run -p 5001:80 -p 7001:443 attendance-api
```

## Troubleshooting

### Port Already in Use
Change ports in `Properties/launchSettings.json`

### Database Connection Failed
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure firewall allows connections

### Migration Errors
```powershell
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## License

MIT
