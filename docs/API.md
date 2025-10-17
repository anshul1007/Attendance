# API Documentation

## Base URL
- **Development**: `http://localhost:5001/api`
- **Production**: `https://api.attendance.company.com/api`

## Authentication
All protected endpoints require JWT Bearer token in the Authorization header:
```
Authorization: Bearer <token>
```

## Response Format

### Success Response
```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful"
}
```

### Error Response
```json
{
  "success": false,
  "error": "Error message",
  "errors": ["Validation error 1", "Validation error 2"]
}
```

---

## Authentication Endpoints

### POST /auth/login
Authenticate user and receive JWT token.

**Request:**
```json
{
  "email": "john.doe@company.com",
  "password": "SecurePassword123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "user": {
      "id": "guid",
      "email": "john.doe@company.com",
      "firstName": "John",
      "lastName": "Doe",
      "role": "Employee"
    }
  }
}
```

### POST /auth/refresh
Refresh JWT token.

**Request:**
```json
{
  "token": "current-token"
}
```

### POST /auth/logout
Invalidate current session.

**Headers:** Authorization required

---

## Attendance Endpoints

### POST /attendance/login
Record employee login/attendance.

**Authorization:** Employee, Manager, Administrator

**Request:**
```json
{
  "timestamp": "2025-10-15T09:00:00Z" // Optional, uses server time if not provided
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "attendanceId": "guid",
    "loginTime": "2025-10-15T09:00:00Z",
    "date": "2025-10-15",
    "isWeekend": false,
    "isPublicHoliday": false,
    "message": "Login recorded successfully"
  }
}
```

### POST /attendance/logout
Record employee logout.

**Authorization:** Employee, Manager, Administrator

**Request:**
```json
{
  "attendanceId": "guid",
  "timestamp": "2025-10-15T18:00:00Z" // Optional
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "attendanceId": "guid",
    "loginTime": "2025-10-15T09:00:00Z",
    "logoutTime": "2025-10-15T18:00:00Z",
    "duration": "9 hours",
    "compensatoryOffEarned": false
  }
}
```

### GET /attendance/my-attendance
Get current user's attendance records.

**Authorization:** Employee, Manager, Administrator

**Query Parameters:**
- `startDate` (optional): Filter from date
- `endDate` (optional): Filter to date
- `page` (default: 1): Page number
- `pageSize` (default: 10): Records per page

**Response:**
```json
{
  "success": true,
  "data": {
    "attendance": [
      {
        "id": "guid",
        "date": "2025-10-15",
        "loginTime": "2025-10-15T09:00:00Z",
        "logoutTime": "2025-10-15T18:00:00Z",
        "isWeekend": false,
        "isPublicHoliday": false,
        "status": "Approved"
      }
    ],
    "totalRecords": 100,
    "page": 1,
    "pageSize": 10
  }
}
```

### GET /attendance/{id}
Get specific attendance record.

**Authorization:** Employee (own), Manager (team), Administrator (all)

### GET /attendance/today
Get today's attendance status.

**Authorization:** Employee, Manager, Administrator

**Response:**
```json
{
  "success": true,
  "data": {
    "hasLoggedIn": true,
    "hasLoggedOut": false,
    "attendanceId": "guid",
    "loginTime": "2025-10-15T09:00:00Z"
  }
}
```

---

## Leave Management Endpoints

### POST /leave/request
Submit a leave request.

**Authorization:** Employee, Manager, Administrator

**Request:**
```json
{
  "leaveType": "CasualLeave", // CasualLeave, EarnedLeave, CompensatoryOff
  "startDate": "2025-10-20",
  "endDate": "2025-10-22",
  "reason": "Personal work"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "leaveRequestId": "guid",
    "totalDays": 3,
    "status": "Pending",
    "remainingBalance": 7
  }
}
```

### GET /leave/my-requests
Get current user's leave requests.

**Authorization:** Employee, Manager, Administrator

**Query Parameters:**
- `status` (optional): Pending, Approved, Rejected
- `year` (optional): Filter by year

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "leaveType": "CasualLeave",
      "startDate": "2025-10-20",
      "endDate": "2025-10-22",
      "totalDays": 3,
      "reason": "Personal work",
      "status": "Pending",
      "createdAt": "2025-10-15T10:00:00Z"
    }
  ]
}
```

### GET /leave/balance
Get leave balance for current user.

**Authorization:** Employee, Manager, Administrator

**Response:**
```json
{
  "success": true,
  "data": {
    "year": 2025,
    "casualLeaveBalance": 7,
    "earnedLeaveBalance": 12,
    "compensatoryOffBalance": 2
  }
}
```

### GET /leave/{id}
Get specific leave request details.

**Authorization:** Employee (own), Manager (team), Administrator (all)

### DELETE /leave/{id}
Cancel a pending leave request.

**Authorization:** Employee (own), Manager (own), Administrator (all)

---

## Manager Endpoints

### GET /manager/team-attendance
Get team attendance records.

**Authorization:** Manager, Administrator

**Query Parameters:**
- `startDate`, `endDate`: Date range
- `employeeId` (optional): Specific employee
- `status` (optional): Filter by status

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "employeeId": "guid",
      "employeeName": "John Doe",
      "date": "2025-10-15",
      "loginTime": "2025-10-15T09:00:00Z",
      "logoutTime": "2025-10-15T18:00:00Z",
      "status": "Pending"
    }
  ]
}
```

### POST /manager/approve-attendance/{id}
Approve an attendance record.

**Authorization:** Manager, Administrator

**Response:**
```json
{
  "success": true,
  "message": "Attendance approved successfully"
}
```

### POST /manager/reject-attendance/{id}
Reject an attendance record.

**Authorization:** Manager, Administrator

**Request:**
```json
{
  "reason": "Invalid timing"
}
```

### GET /manager/pending-leave-requests
Get pending leave requests for approval.

**Authorization:** Manager, Administrator

### POST /manager/approve-leave/{id}
Approve a leave request.

**Authorization:** Manager, Administrator

### POST /manager/reject-leave/{id}
Reject a leave request.

**Authorization:** Manager, Administrator

**Request:**
```json
{
  "reason": "Team already short-staffed on those dates"
}
```

---

## Admin Endpoints

### POST /admin/users
Create a new user.

**Authorization:** Administrator

**Request:**
```json
{
  "email": "new.employee@company.com",
  "firstName": "New",
  "lastName": "Employee",
  "employeeId": "EMP001",
  "role": "Employee",
  "managerId": "guid",
  "password": "TempPassword123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "userId": "guid",
    "email": "new.employee@company.com",
    "employeeId": "EMP001"
  }
}
```

### GET /admin/users
Get all users.

**Authorization:** Administrator

**Query Parameters:**
- `role` (optional): Filter by role
- `isActive` (optional): Filter by status
- `search` (optional): Search by name/email

### PUT /admin/users/{id}
Update user details.

**Authorization:** Administrator

### DELETE /admin/users/{id}
Deactivate user account.

**Authorization:** Administrator

### POST /admin/leave-entitlements
Allocate leave entitlements to users.

**Authorization:** Administrator

**Request:**
```json
{
  "userId": "guid",
  "year": 2025,
  "casualLeave": 10,
  "earnedLeave": 15
}
```

### POST /admin/holidays
Add public holiday.

**Authorization:** Administrator

**Request:**
```json
{
  "date": "2025-12-25",
  "name": "Christmas",
  "description": "Christmas Day"
}
```

### GET /admin/holidays
Get all public holidays.

**Query Parameters:**
- `year` (optional): Filter by year

### PUT /admin/holidays/{id}
Update holiday details.

### DELETE /admin/holidays/{id}
Delete a holiday.

### GET /admin/reports/attendance
Get attendance reports.

**Query Parameters:**
- `startDate`, `endDate`: Date range
- `departmentId` (optional)
- `exportFormat` (optional): csv, excel, pdf

### GET /admin/reports/leave
Get leave reports.

---

## User Profile Endpoints

### GET /user/profile
Get current user's profile.

**Authorization:** Employee, Manager, Administrator

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "email": "john.doe@company.com",
    "firstName": "John",
    "lastName": "Doe",
    "employeeId": "EMP001",
    "role": "Employee",
    "manager": {
      "id": "guid",
      "name": "Jane Smith"
    },
    "isActive": true
  }
}
```

### PUT /user/profile
Update current user's profile.

**Request:**
```json
{
  "firstName": "John",
  "lastName": "Doe"
}
```

### PUT /user/change-password
Change password.

**Request:**
```json
{
  "currentPassword": "OldPassword123",
  "newPassword": "NewPassword123"
}
```

---

## Status Codes

- `200 OK`: Successful request
- `201 Created`: Resource created successfully
- `400 Bad Request`: Invalid request data
- `401 Unauthorized`: Missing or invalid authentication
- `403 Forbidden`: Insufficient permissions
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

---

## Rate Limiting

- **Authentication**: 5 requests per minute
- **Standard APIs**: 100 requests per minute
- **Reports**: 10 requests per minute

---

**Last Updated**: October 15, 2025
