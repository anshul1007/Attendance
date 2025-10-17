# Complete Setup Guide

This guide covers everything needed to set up the Attendance Management System with Azure PostgreSQL database.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Azure PostgreSQL Configuration](#azure-postgresql-configuration)
3. [Install Backend Packages](#install-backend-packages)
4. [Implement Database Models](#implement-database-models)
5. [Run Database Migrations](#run-database-migrations)
6. [Start Applications](#start-applications)
7. [Verify Setup](#verify-setup)

---

## Prerequisites

### Required Software

- **Node.js 18+** and npm - [Download](https://nodejs.org/)
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download)
- **PostgreSQL Client** (psql) - [Download](https://www.postgresql.org/download/)
- **Git** (optional) - For version control

### Check Installation

```powershell
# Check Node.js
node --version  # Should be 18.x or higher

# Check .NET
dotnet --version  # Should be 8.0.x

# Check PostgreSQL client
psql --version
```

---

## Azure PostgreSQL Configuration

### Your Database Details

```
Server:   attendance.postgres.database.azure.com
Port:     5432
Database: postgres
Username: AttendanceDB
Password: {your-password}
```

### Step 1: Update Connection String

Edit `backend/AttendanceAPI/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=attendance.postgres.database.azure.com;Port=5432;Database=postgres;Username=AttendanceDB;Password=YOUR_ACTUAL_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  }
}
```

**⚠️ Important:** Replace `YOUR_ACTUAL_PASSWORD` with your actual Azure database password!

### Step 2: Configure Azure Firewall

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your PostgreSQL server: **attendance**
3. Click **Connection Security** or **Networking**
4. Click **Add current client IP address**
5. Click **Save**

### Step 3: Test Database Connection

```powershell
cd E:\Attendance
.\scripts\test-azure-connection.ps1
```

This will prompt for your password and test the connection.

**Alternative:** Test with psql directly:

```powershell
psql "host=attendance.postgres.database.azure.com port=5432 dbname=postgres user=AttendanceDB password=your-password sslmode=require"
```

Expected output:
```
psql (16.x)
SSL connection (protocol: TLSv1.3, cipher: ...)
Type "help" for help.

postgres=>
```

---

## Install Backend Packages

### Step 1: Run Setup Script

```powershell
cd E:\Attendance\backend
.\setup-packages.ps1
```

This installs:
- ✅ Entity Framework Core 8.0
- ✅ Npgsql (PostgreSQL provider)
- ✅ JWT Authentication
- ✅ BCrypt for password hashing
- ✅ AutoMapper
- ✅ FluentValidation
- ✅ Serilog logging
- ✅ dotnet-ef tools (globally)

### Step 2: Verify Installation

```powershell
cd E:\Attendance\backend\AttendanceAPI
dotnet restore
dotnet ef --version
```

Expected: `Entity Framework Core .NET Command-line Tools 9.0.10`

---

## Implement Database Models

Before running migrations, you need to create the entity models and DbContext.

### Step 1: Create Folders

```powershell
cd E:\Attendance\backend\AttendanceAPI
New-Item -ItemType Directory -Path "Models\Entities" -Force
New-Item -ItemType Directory -Path "Data" -Force
```

### Step 2: Create Entity Models

Create these 6 files in `backend/AttendanceAPI/Models/Entities/`:

#### User.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceAPI.Models.Entities
{
    [Table("Users")]
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public Guid? ManagerId { get; set; }

        [ForeignKey(nameof(ManagerId))]
        public User? Manager { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
        public ICollection<LeaveEntitlement> LeaveEntitlements { get; set; } = new List<LeaveEntitlement>();
        public ICollection<User> Subordinates { get; set; } = new List<User>();
    }

    public enum UserRole
    {
        Employee = 1,
        Manager = 2,
        Administrator = 3
    }
}
```

#### Attendance.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceAPI.Models.Entities
{
    [Table("Attendance")]
    public class Attendance
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        public DateTime LoginTime { get; set; }

        public DateTime? LogoutTime { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        public bool IsWeekend { get; set; } = false;
        public bool IsPublicHoliday { get; set; } = false;

        [Required]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public Guid? ApprovedBy { get; set; }

        [ForeignKey(nameof(ApprovedBy))]
        public User? Approver { get; set; }

        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

#### LeaveRequest.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceAPI.Models.Entities
{
    [Table("LeaveRequests")]
    public class LeaveRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        public LeaveType LeaveType { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        [Required, Column(TypeName = "decimal(5,2)")]
        public decimal TotalDays { get; set; }

        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;

        public Guid? ApprovedBy { get; set; }

        [ForeignKey(nameof(ApprovedBy))]
        public User? Approver { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum LeaveType
    {
        CasualLeave = 1,
        EarnedLeave = 2,
        CompensatoryOff = 3
    }

    public enum ApprovalStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}
```

#### LeaveEntitlement.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceAPI.Models.Entities
{
    [Table("LeaveEntitlements")]
    public class LeaveEntitlement
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        public int Year { get; set; }

        [Required, Column(TypeName = "decimal(5,2)")]
        public decimal CasualLeaveBalance { get; set; } = 0;

        [Required, Column(TypeName = "decimal(5,2)")]
        public decimal EarnedLeaveBalance { get; set; } = 0;

        [Required, Column(TypeName = "decimal(5,2)")]
        public decimal CompensatoryOffBalance { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

#### PublicHoliday.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceAPI.Models.Entities
{
    [Table("PublicHolidays")]
    public class PublicHoliday
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public DateOnly Date { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public int Year { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
```

#### AuditLog.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceAPI.Models.Entities
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }

        [Required, MaxLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string EntityId { get; set; } = string.Empty;

        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? IpAddress { get; set; }
    }
}
```

### Step 3: Create DbContext

Create `backend/AttendanceAPI/Data/ApplicationDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using AttendanceAPI.Models.Entities;

namespace AttendanceAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Attendance> Attendance { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveEntitlement> LeaveEntitlements { get; set; }
        public DbSet<PublicHoliday> PublicHolidays { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.EmployeeId).IsUnique();
                entity.HasIndex(e => e.ManagerId);

                entity.HasOne(e => e.Manager)
                    .WithMany(e => e.Subordinates)
                    .HasForeignKey(e => e.ManagerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Attendance configuration
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.Date });
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Attendances)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Approver)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // LeaveRequest configuration
            modelBuilder.Entity<LeaveRequest>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.StartDate, e.EndDate });

                entity.HasOne(e => e.User)
                    .WithMany(e => e.LeaveRequests)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Approver)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // LeaveEntitlement configuration
            modelBuilder.Entity<LeaveEntitlement>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.Year }).IsUnique();

                entity.HasOne(e => e.User)
                    .WithMany(e => e.LeaveEntitlements)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PublicHoliday configuration
            modelBuilder.Entity<PublicHoliday>(entity =>
            {
                entity.HasIndex(e => e.Date).IsUnique();
                entity.HasIndex(e => e.Year);
            });

            // AuditLog configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.EntityType, e.EntityId });

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
```

### Step 4: Configure in Program.cs

Update `backend/AttendanceAPI/Program.cs`:

```csharp
using AttendanceAPI.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## Run Database Migrations

### Step 1: Create Migration

```powershell
cd E:\Attendance\backend\AttendanceAPI
dotnet ef migrations add InitialCreate
```

This creates migration files in the `Migrations/` folder.

### Step 2: Review Migration

Check the generated migration file to ensure:
- Table names are quoted: `"Users"`, `"Attendance"`, etc.
- Columns use correct PostgreSQL types
- Indexes and foreign keys are correct

### Step 3: Apply Migration to Azure Database

```powershell
dotnet ef database update
```

Expected output:
```
Build started...
Build succeeded.
Applying migration '20251017xxxxxx_InitialCreate'.
Done.
```

### Step 4: Verify Tables Created

```powershell
# Connect to Azure database
psql "host=attendance.postgres.database.azure.com port=5432 dbname=postgres user=AttendanceDB sslmode=require"

# List tables
\dt

# Expected output:
# Users
# Attendance
# LeaveRequests
# LeaveEntitlements
# PublicHolidays
# AuditLogs
# __EFMigrationsHistory

# Describe a table
\d "Users"

# Exit
\q
```

---

## Start Applications

### Backend

```powershell
cd E:\Attendance\backend\AttendanceAPI
dotnet run
```

Expected output:
```
Now listening on: http://localhost:5146
Application started. Press Ctrl+C to shut down.
```

### Frontend

Open a **new terminal**:

```powershell
cd E:\Attendance\frontend
npm install  # If not done already
npm start
```

Expected output:
```
** Angular Live Development Server is listening on localhost:4200 **
```

### Both at Once

```powershell
cd E:\Attendance
.\scripts\start-all.ps1
```

This opens both in separate terminal windows.

---

## Verify Setup

### 1. Check Backend API

Open browser: http://localhost:5146/swagger

You should see Swagger UI with available endpoints.

### 2. Check Frontend

Open browser: http://localhost:4200

You should see the login page.

### 3. Check Database Connection

Backend logs should show:
```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand...
```

No connection errors.

### 4. Test API Health (Optional)

Create a test controller to verify database connection:

```csharp
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Test database connection
            await _context.Database.CanConnectAsync();
            return Ok(new { status = "healthy", database = "connected" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = "unhealthy", error = ex.Message });
        }
    }
}
```

Test: http://localhost:5146/api/health

---

## Next Steps

1. **Insert Sample Data**
   - Create admin user
   - Add public holidays
   - Initialize leave entitlements

2. **Implement Controllers**
   - AuthController (login/logout)
   - AttendanceController
   - LeaveController
   - ManagerController
   - AdminController

3. **Add JWT Authentication**
   - Configure in Program.cs
   - Add [Authorize] attributes

4. **Test Integration**
   - Login from frontend
   - Mark attendance
   - Request leave
   - Test approval workflows

---

## Troubleshooting

See [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for common issues and solutions.

### Quick Fixes

**Can't connect to Azure:**
- Check firewall rules in Azure Portal
- Verify password in appsettings.json
- Ensure SSL Mode=Require in connection string

**Migration fails:**
- Ensure all entity files compile
- Check DbContext is properly configured
- Verify connection string is correct

**Frontend can't reach backend:**
- Check backend is running on port 5146
- Verify frontend environment.ts has correct apiUrl
- Check CORS configuration in Program.cs

---

**Setup Guide Version:** 1.0  
**Last Updated:** October 17, 2025  
**Database:** Azure PostgreSQL  
**Framework:** .NET 8 + Angular 18
