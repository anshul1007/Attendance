# Database Schema

## Entity Relationship Diagram

```
┌─────────────────────────┐
│        Users            │
├─────────────────────────┤
│ Id (PK)                 │
│ Email                   │
│ PasswordHash            │
│ FirstName               │
│ LastName                │
│ EmployeeId              │
│ Role (Enum)             │
│ ManagerId (FK)          │
│ IsActive                │
│ CreatedAt               │
│ UpdatedAt               │
└─────────────────────────┘
         │ 1
         │
         │ N
         ▼
┌─────────────────────────┐         ┌─────────────────────────┐
│     Attendance          │         │   LeaveEntitlements     │
├─────────────────────────┤         ├─────────────────────────┤
│ Id (PK)                 │         │ Id (PK)                 │
│ UserId (FK)             │         │ UserId (FK)             │
│ LoginTime               │         │ Year                    │
│ LogoutTime              │         │ CasualLeaveBalance      │
│ Date                    │         │ EarnedLeaveBalance      │
│ IsWeekend               │         │ CompensatoryOffBalance  │
│ IsPublicHoliday         │         │ CreatedAt               │
│ Status (Enum)           │         │ UpdatedAt               │
│ ApprovedBy (FK)         │         └─────────────────────────┘
│ ApprovedAt              │                    ▲
│ CreatedAt               │                    │
│ UpdatedAt               │                    │ 1
└─────────────────────────┘                    │
         │ N                                   │
         │                                     │
         ▼                                     │
┌─────────────────────────┐                    │
│    LeaveRequests        │                    │
├─────────────────────────┤                    │
│ Id (PK)                 │                    │
│ UserId (FK)             │────────────────────┘
│ LeaveType (Enum)        │
│ StartDate               │
│ EndDate                 │
│ TotalDays               │
│ Reason                  │
│ Status (Enum)           │
│ ApprovedBy (FK)         │
│ ApprovedAt              │
│ RejectionReason         │
│ CreatedAt               │
│ UpdatedAt               │
└─────────────────────────┘


┌─────────────────────────┐
│    PublicHolidays       │
├─────────────────────────┤
│ Id (PK)                 │
│ Date                    │
│ Name                    │
│ Description             │
│ Year                    │
│ IsActive                │
│ CreatedAt               │
└─────────────────────────┘


┌─────────────────────────┐
│    AuditLogs            │
├─────────────────────────┤
│ Id (PK)                 │
│ UserId (FK)             │
│ Action                  │
│ EntityType              │
│ EntityId                │
│ OldValue                │
│ NewValue                │
│ Timestamp               │
│ IpAddress               │
└─────────────────────────┘
```

## Table Definitions

### Users Table
Stores all user information including employees, managers, and administrators.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PK | Primary key |
| Email | NVARCHAR(256) | UNIQUE, NOT NULL | User email address |
| PasswordHash | NVARCHAR(MAX) | NOT NULL | Hashed password |
| FirstName | NVARCHAR(100) | NOT NULL | First name |
| LastName | NVARCHAR(100) | NOT NULL | Last name |
| EmployeeId | NVARCHAR(50) | UNIQUE, NOT NULL | Company employee ID |
| Role | INT | NOT NULL | 1=Employee, 2=Manager, 3=Admin |
| ManagerId | UNIQUEIDENTIFIER | FK (Users.Id), NULL | Reference to manager |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Account status |
| CreatedAt | DATETIME2 | NOT NULL | Creation timestamp |
| UpdatedAt | DATETIME2 | NOT NULL | Last update timestamp |

### Attendance Table
Records all login/logout events for employees.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PK | Primary key |
| UserId | UNIQUEIDENTIFIER | FK (Users.Id), NOT NULL | User reference |
| LoginTime | DATETIME2 | NOT NULL | Login timestamp |
| LogoutTime | DATETIME2 | NULL | Logout timestamp |
| Date | DATE | NOT NULL | Attendance date |
| IsWeekend | BIT | NOT NULL, DEFAULT 0 | Weekend indicator |
| IsPublicHoliday | BIT | NOT NULL, DEFAULT 0 | Holiday indicator |
| Status | INT | NOT NULL, DEFAULT 0 | 0=Pending, 1=Approved, 2=Rejected |
| ApprovedBy | UNIQUEIDENTIFIER | FK (Users.Id), NULL | Approver reference |
| ApprovedAt | DATETIME2 | NULL | Approval timestamp |
| CreatedAt | DATETIME2 | NOT NULL | Creation timestamp |
| UpdatedAt | DATETIME2 | NOT NULL | Last update timestamp |

### LeaveRequests Table
Stores all leave requests from employees.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PK | Primary key |
| UserId | UNIQUEIDENTIFIER | FK (Users.Id), NOT NULL | User reference |
| LeaveType | INT | NOT NULL | 1=Casual, 2=Earned, 3=CompensatoryOff |
| StartDate | DATE | NOT NULL | Leave start date |
| EndDate | DATE | NOT NULL | Leave end date |
| TotalDays | DECIMAL(5,2) | NOT NULL | Total leave days |
| Reason | NVARCHAR(500) | NOT NULL | Leave reason |
| Status | INT | NOT NULL, DEFAULT 0 | 0=Pending, 1=Approved, 2=Rejected |
| ApprovedBy | UNIQUEIDENTIFIER | FK (Users.Id), NULL | Approver reference |
| ApprovedAt | DATETIME2 | NULL | Approval timestamp |
| RejectionReason | NVARCHAR(500) | NULL | Rejection reason |
| CreatedAt | DATETIME2 | NOT NULL | Creation timestamp |
| UpdatedAt | DATETIME2 | NOT NULL | Last update timestamp |

### LeaveEntitlements Table
Manages leave balances for each employee per year.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PK | Primary key |
| UserId | UNIQUEIDENTIFIER | FK (Users.Id), NOT NULL | User reference |
| Year | INT | NOT NULL | Calendar year |
| CasualLeaveBalance | DECIMAL(5,2) | NOT NULL | Casual leave balance |
| EarnedLeaveBalance | DECIMAL(5,2) | NOT NULL | Earned leave balance |
| CompensatoryOffBalance | DECIMAL(5,2) | NOT NULL, DEFAULT 0 | Comp-off balance |
| CreatedAt | DATETIME2 | NOT NULL | Creation timestamp |
| UpdatedAt | DATETIME2 | NOT NULL | Last update timestamp |

### PublicHolidays Table
Stores public holidays for the organization.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PK | Primary key |
| Date | DATE | NOT NULL | Holiday date |
| Name | NVARCHAR(200) | NOT NULL | Holiday name |
| Description | NVARCHAR(500) | NULL | Holiday description |
| Year | INT | NOT NULL | Calendar year |
| IsActive | BIT | NOT NULL, DEFAULT 1 | Active status |
| CreatedAt | DATETIME2 | NOT NULL | Creation timestamp |

### AuditLogs Table
Tracks all important system actions for auditing.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | UNIQUEIDENTIFIER | PK | Primary key |
| UserId | UNIQUEIDENTIFIER | FK (Users.Id), NULL | User reference |
| Action | NVARCHAR(100) | NOT NULL | Action performed |
| EntityType | NVARCHAR(100) | NOT NULL | Entity type affected |
| EntityId | NVARCHAR(100) | NOT NULL | Entity ID |
| OldValue | NVARCHAR(MAX) | NULL | Old value (JSON) |
| NewValue | NVARCHAR(MAX) | NULL | New value (JSON) |
| Timestamp | DATETIME2 | NOT NULL | Action timestamp |
| IpAddress | NVARCHAR(50) | NULL | User IP address |

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

## Indexes

### Users Table
- `IX_Users_Email` - UNIQUE on Email
- `IX_Users_EmployeeId` - UNIQUE on EmployeeId
- `IX_Users_ManagerId` - on ManagerId

### Attendance Table
- `IX_Attendance_UserId_Date` - on UserId, Date
- `IX_Attendance_Date` - on Date
- `IX_Attendance_Status` - on Status

### LeaveRequests Table
- `IX_LeaveRequests_UserId` - on UserId
- `IX_LeaveRequests_Status` - on Status
- `IX_LeaveRequests_StartDate_EndDate` - on StartDate, EndDate

### LeaveEntitlements Table
- `IX_LeaveEntitlements_UserId_Year` - UNIQUE on UserId, Year

### PublicHolidays Table
- `IX_PublicHolidays_Date` - UNIQUE on Date
- `IX_PublicHolidays_Year` - on Year

## Sample Data

### Default Administrator Account
```sql
INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, EmployeeId, Role, IsActive, CreatedAt, UpdatedAt)
VALUES (
    NEWID(),
    'admin@company.com',
    -- Password: Admin@123 (hashed)
    '$2a$11$...',
    'System',
    'Administrator',
    'ADMIN001',
    3,
    1,
    GETUTCDATE(),
    GETUTCDATE()
);
```

### Sample Public Holidays (2025)
```sql
INSERT INTO PublicHolidays (Id, Date, Name, Year, IsActive, CreatedAt)
VALUES
    (NEWID(), '2025-01-01', 'New Year Day', 2025, 1, GETUTCDATE()),
    (NEWID(), '2025-01-26', 'Republic Day', 2025, 1, GETUTCDATE()),
    (NEWID(), '2025-08-15', 'Independence Day', 2025, 1, GETUTCDATE()),
    (NEWID(), '2025-10-02', 'Gandhi Jayanti', 2025, 1, GETUTCDATE()),
    (NEWID(), '2025-12-25', 'Christmas', 2025, 1, GETUTCDATE());
```

---

**Last Updated**: October 15, 2025
