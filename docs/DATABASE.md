# PostgreSQL Database Schema

## Database Configuration

### Connection String Format
```
Host=localhost;Port=5432;Database=AttendanceDB;Username=postgres;Password=your_password
```

### Create Database

```sql
CREATE DATABASE "AttendanceDB"
    WITH 
    ENCODING = 'UTF8'
    LC_COLLATE = 'en_US.UTF-8'
    LC_CTYPE = 'en_US.UTF-8'
    TEMPLATE = template0;

\c AttendanceDB
```

## Table Definitions

### Users Table

```sql
CREATE TABLE "Users" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Email" VARCHAR(256) UNIQUE NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "FirstName" VARCHAR(100) NOT NULL,
    "LastName" VARCHAR(100) NOT NULL,
    "EmployeeId" VARCHAR(50) UNIQUE NOT NULL,
    "Role" INTEGER NOT NULL,
    "ManagerId" UUID NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_Users_Manager" FOREIGN KEY ("ManagerId") REFERENCES "Users"("Id")
);

-- Indexes
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users"("Email");
CREATE UNIQUE INDEX "IX_Users_EmployeeId" ON "Users"("EmployeeId");
CREATE INDEX "IX_Users_ManagerId" ON "Users"("ManagerId");
CREATE INDEX "IX_Users_Role" ON "Users"("Role");
```

### Attendance Table

```sql
CREATE TABLE "Attendance" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "LoginTime" TIMESTAMP NOT NULL,
    "LogoutTime" TIMESTAMP NULL,
    "Date" DATE NOT NULL,
    "IsWeekend" BOOLEAN NOT NULL DEFAULT false,
    "IsPublicHoliday" BOOLEAN NOT NULL DEFAULT false,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "ApprovedBy" UUID NULL,
    "ApprovedAt" TIMESTAMP NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_Attendance_User" FOREIGN KEY ("UserId") REFERENCES "Users"("Id"),
    CONSTRAINT "FK_Attendance_Approver" FOREIGN KEY ("ApprovedBy") REFERENCES "Users"("Id")
);

-- Indexes
CREATE INDEX "IX_Attendance_UserId_Date" ON "Attendance"("UserId", "Date");
CREATE INDEX "IX_Attendance_Date" ON "Attendance"("Date");
CREATE INDEX "IX_Attendance_Status" ON "Attendance"("Status");
```

### LeaveRequests Table

```sql
CREATE TABLE "LeaveRequests" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "LeaveType" INTEGER NOT NULL,
    "StartDate" DATE NOT NULL,
    "EndDate" DATE NOT NULL,
    "TotalDays" DECIMAL(5,2) NOT NULL,
    "Reason" VARCHAR(1000) NOT NULL,
    "Status" INTEGER NOT NULL DEFAULT 0,
    "ApprovedBy" UUID NULL,
    "ApprovedAt" TIMESTAMP NULL,
    "RejectionReason" VARCHAR(500) NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_LeaveRequests_User" FOREIGN KEY ("UserId") REFERENCES "Users"("Id"),
    CONSTRAINT "FK_LeaveRequests_Approver" FOREIGN KEY ("ApprovedBy") REFERENCES "Users"("Id")
);

-- Indexes
CREATE INDEX "IX_LeaveRequests_UserId" ON "LeaveRequests"("UserId");
CREATE INDEX "IX_LeaveRequests_Status" ON "LeaveRequests"("Status");
CREATE INDEX "IX_LeaveRequests_StartDate_EndDate" ON "LeaveRequests"("StartDate", "EndDate");
```

### LeaveEntitlements Table

```sql
CREATE TABLE "LeaveEntitlements" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "Year" INTEGER NOT NULL,
    "CasualLeaveBalance" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "EarnedLeaveBalance" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "CompensatoryOffBalance" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_LeaveEntitlements_User" FOREIGN KEY ("UserId") REFERENCES "Users"("Id")
);

-- Indexes
CREATE UNIQUE INDEX "IX_LeaveEntitlements_UserId_Year" ON "LeaveEntitlements"("UserId", "Year");
```

### PublicHolidays Table

```sql
CREATE TABLE "PublicHolidays" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Date" DATE NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "Year" INTEGER NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT true,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Indexes
CREATE UNIQUE INDEX "IX_PublicHolidays_Date" ON "PublicHolidays"("Date");
CREATE INDEX "IX_PublicHolidays_Year" ON "PublicHolidays"("Year");
```

### AuditLogs Table

```sql
CREATE TABLE "AuditLogs" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NULL,
    "Action" VARCHAR(100) NOT NULL,
    "EntityType" VARCHAR(100) NOT NULL,
    "EntityId" VARCHAR(100) NOT NULL,
    "OldValue" TEXT NULL,
    "NewValue" TEXT NULL,
    "Timestamp" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IpAddress" VARCHAR(50) NULL,
    CONSTRAINT "FK_AuditLogs_User" FOREIGN KEY ("UserId") REFERENCES "Users"("Id")
);

-- Indexes
CREATE INDEX "IX_AuditLogs_UserId" ON "AuditLogs"("UserId");
CREATE INDEX "IX_AuditLogs_Timestamp" ON "AuditLogs"("Timestamp");
CREATE INDEX "IX_AuditLogs_EntityType_EntityId" ON "AuditLogs"("EntityType", "EntityId");
```

## Enumerations

### UserRole
```csharp
public enum UserRole
{
    Employee = 1,
    Manager = 2,
    Administrator = 3
}
```

### LeaveType
```csharp
public enum LeaveType
{
    CasualLeave = 1,
    EarnedLeave = 2,
    CompensatoryOff = 3
}
```

### ApprovalStatus
```csharp
public enum ApprovalStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
```

## Sample Data

### Default Administrator Account
```sql
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "FirstName", "LastName", "EmployeeId", "Role", "IsActive", "CreatedAt", "UpdatedAt")
VALUES (
    gen_random_uuid(),
    'admin@company.com',
    '$2a$11$YourHashedPasswordHere',  -- Use BCrypt to hash 'Admin@123'
    'System',
    'Administrator',
    'ADMIN001',
    3,
    true,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
);
```

### Sample Public Holidays (2025)
```sql
INSERT INTO "PublicHolidays" ("Id", "Date", "Name", "Year", "IsActive", "CreatedAt")
VALUES
    (gen_random_uuid(), '2025-01-01', 'New Year Day', 2025, true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), '2025-01-26', 'Republic Day', 2025, true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), '2025-03-14', 'Holi', 2025, true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), '2025-08-15', 'Independence Day', 2025, true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), '2025-10-02', 'Gandhi Jayanti', 2025, true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), '2025-10-24', 'Diwali', 2025, true, CURRENT_TIMESTAMP),
    (gen_random_uuid(), '2025-12-25', 'Christmas', 2025, true, CURRENT_TIMESTAMP);
```

### Initialize Leave Entitlements for New User
```sql
-- After creating a new user, initialize their leave balance for the current year
INSERT INTO "LeaveEntitlements" ("Id", "UserId", "Year", "CasualLeaveBalance", "EarnedLeaveBalance", "CompensatoryOffBalance", "CreatedAt", "UpdatedAt")
VALUES (
    gen_random_uuid(),
    'USER_UUID_HERE',
    EXTRACT(YEAR FROM CURRENT_DATE),
    12.00,  -- 12 casual leaves per year
    15.00,  -- 15 earned leaves per year
    0.00,   -- Comp-off starts at 0
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP
);
```

## Useful PostgreSQL Commands

### Check Database Size
```sql
SELECT pg_size_pretty(pg_database_size('AttendanceDB'));
```

### List All Tables
```sql
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'public' 
ORDER BY table_name;
```

### Check Table Row Counts
```sql
SELECT 
    schemaname,
    relname as table_name,
    n_live_tup as row_count
FROM pg_stat_user_tables
ORDER BY n_live_tup DESC;
```

### Backup Database
```bash
pg_dump -U postgres -d AttendanceDB -F c -b -v -f attendance_backup.dump
```

### Restore Database
```bash
pg_restore -U postgres -d AttendanceDB -v attendance_backup.dump
```

## Performance Considerations

### Partitioning (Optional for Large Datasets)
For very large attendance data, consider partitioning by date:

```sql
-- Convert Attendance to partitioned table (if needed)
CREATE TABLE "Attendance_Partitioned" (
    LIKE "Attendance" INCLUDING ALL
) PARTITION BY RANGE ("Date");

-- Create partitions for each year
CREATE TABLE "Attendance_2025" PARTITION OF "Attendance_Partitioned"
    FOR VALUES FROM ('2025-01-01') TO ('2026-01-01');

CREATE TABLE "Attendance_2026" PARTITION OF "Attendance_Partitioned"
    FOR VALUES FROM ('2026-01-01') TO ('2027-01-01');
```

### Materialized Views for Reports
```sql
-- Monthly attendance summary
CREATE MATERIALIZED VIEW "MonthlyAttendanceSummary" AS
SELECT 
    u."Id" as "UserId",
    u."FirstName" || ' ' || u."LastName" as "FullName",
    EXTRACT(YEAR FROM a."Date") as "Year",
    EXTRACT(MONTH FROM a."Date") as "Month",
    COUNT(*) as "TotalDays",
    COUNT(*) FILTER (WHERE a."IsWeekend" = false AND a."IsPublicHoliday" = false) as "WorkingDays"
FROM "Users" u
JOIN "Attendance" a ON u."Id" = a."UserId"
GROUP BY u."Id", u."FirstName", u."LastName", EXTRACT(YEAR FROM a."Date"), EXTRACT(MONTH FROM a."Date");

-- Refresh the view
REFRESH MATERIALIZED VIEW "MonthlyAttendanceSummary";
```

## Migration from SQL Server

If migrating from SQL Server:
- `UNIQUEIDENTIFIER` → `UUID`
- `NVARCHAR` → `VARCHAR` or `TEXT`
- `DATETIME2` → `TIMESTAMP`
- `BIT` → `BOOLEAN`
- `NEWID()` → `gen_random_uuid()`
- `GETUTCDATE()` → `CURRENT_TIMESTAMP`

---

**Last Updated**: October 17, 2025
