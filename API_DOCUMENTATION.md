# Attendance Management System - API Documentation

## Base URL
```
http://localhost:5146/api
```

## Authentication
All endpoints except `/auth/login` require a JWT token in the Authorization header:
```
Authorization: Bearer {your_jwt_token}
```

---

## 1. Authentication Endpoints

### Login
```http
POST /auth/login
Content-Type: application/json

{
  "email": "employee@attendance.com",
  "password": "Employee@123"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "A1JSyYcEP0GsIax56tAjkQ==",
    "user": {
      "id": "d8702954-2c7a-44cb-8f4a-b08f35a739e9",
      "email": "employee@attendance.com",
      "firstName": "Employee",
      "lastName": "User",
      "employeeId": "EMP003",
      "role": "Employee",
      "managerId": "..."
    }
  }
}
```

### Get Current User
```http
GET /auth/me
Authorization: Bearer {token}
```

---

## 2. Attendance Endpoints (All Roles)

### Clock In (Login)
```http
POST /attendance/login
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "...",
    "userId": "...",
    "userName": "Employee User",
    "loginTime": "2025-10-17T10:30:00Z",
    "logoutTime": null,
    "date": "2025-10-17",
    "isWeekend": false,
    "isPublicHoliday": false,
    "status": "Pending",
    "workDuration": null
  },
  "message": "Login successful"
}
```

### Clock Out (Logout)
```http
POST /attendance/logout
Authorization: Bearer {token}
```

**Note:** If clocking out on weekend or public holiday, 0.5 compensatory off day is automatically added.

### Get Today's Attendance
```http
GET /attendance/today
Authorization: Bearer {token}
```

### Get Attendance History
```http
GET /attendance/history?startDate=2025-01-01&endDate=2025-12-31
Authorization: Bearer {token}
```

### Get Team Attendance (Manager/Admin Only)
```http
GET /attendance/team?startDate=2025-10-01&endDate=2025-10-31
Authorization: Bearer {token}
```

---

## 3. Leave Management Endpoints

### Create Leave Request
```http
POST /leave/request
Authorization: Bearer {token}
Content-Type: application/json

{
  "leaveType": 1,  // 1=CasualLeave, 2=EarnedLeave, 3=CompensatoryOff
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
    "id": "...",
    "userId": "...",
    "userName": "Employee User",
    "employeeId": "EMP003",
    "leaveType": "CasualLeave",
    "startDate": "2025-10-20",
    "endDate": "2025-10-22",
    "totalDays": 3,
    "reason": "Personal work",
    "status": "Pending",
    "createdAt": "2025-10-17T10:30:00Z"
  },
  "message": "Leave request created successfully"
}
```

### Get My Leave Requests
```http
GET /leave/my-requests
Authorization: Bearer {token}
```

### Get Leave Balance
```http
GET /leave/balance
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "year": 2025,
    "casualLeaveBalance": 12.0,
    "earnedLeaveBalance": 15.0,
    "compensatoryOffBalance": 1.5
  }
}
```

### Get Pending Leave Approvals (Manager/Admin Only)
```http
GET /leave/pending-approvals
Authorization: Bearer {token}
```

### Approve/Reject Leave (Manager/Admin Only)
```http
POST /leave/approve
Authorization: Bearer {token}
Content-Type: application/json

{
  "leaveRequestId": "...",
  "approved": true,  // false to reject
  "rejectionReason": "Optional reason if rejecting"
}
```

---

## 4. Approval Endpoints (Manager/Admin Only)

### Get Pending Attendance Approvals
```http
GET /approval/attendance/pending
Authorization: Bearer {token}
```

### Approve Attendance
```http
POST /approval/attendance/{attendanceId}/approve
Authorization: Bearer {token}
```

### Reject Attendance
```http
POST /approval/attendance/{attendanceId}/reject
Authorization: Bearer {token}
```

---

## 5. Admin Endpoints (Administrator Only)

### Create User
```http
POST /admin/users
Authorization: Bearer {token}
Content-Type: application/json

{
  "email": "newuser@attendance.com",
  "password": "Password@123",
  "firstName": "John",
  "lastName": "Doe",
  "employeeId": "EMP004",
  "role": 1,  // 1=Employee, 2=Manager, 3=Administrator
  "managerId": "manager-guid"  // Optional
}
```

### Update User
```http
PUT /admin/users/{userId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "Jane",
  "role": 2,
  "managerId": "...",
  "isActive": true
}
```

### Get All Users
```http
GET /admin/users
Authorization: Bearer {token}
```

### Allocate Leave Entitlement
```http
POST /admin/leave-entitlement
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "...",
  "year": 2025,
  "casualLeaveBalance": 12.0,
  "earnedLeaveBalance": 15.0,
  "compensatoryOffBalance": 0.0
}
```

### Get User Leave Balance
```http
GET /admin/leave-entitlement/{userId}?year=2025
Authorization: Bearer {token}
```

### Create Public Holiday
```http
POST /admin/holidays
Authorization: Bearer {token}
Content-Type: application/json

{
  "date": "2025-12-25",
  "name": "Christmas Day",
  "description": "National Holiday"
}
```

### Get Public Holidays
```http
GET /admin/holidays?year=2025
Authorization: Bearer {token}
```

### Delete Public Holiday
```http
DELETE /admin/holidays/{holidayId}
Authorization: Bearer {token}
```

---

## Business Logic

### Attendance
1. **Login**: Captures system time when employee clicks login
2. **Logout**: Captures system time when employee clicks logout
3. **Weekend/Holiday Detection**: Automatically detects if attendance is on weekend or public holiday
4. **Compensatory Off**: Adds 0.5 day compensatory off when logging out on weekend/holiday
5. **Approval Required**: All attendance requires manager approval

### Leave Management
1. **Leave Types**:
   - **Casual Leave (1)**: For unplanned absences
   - **Earned Leave (2)**: Vacation leave
   - **Compensatory Off (3)**: Earned from weekend/holiday work

2. **Validation**:
   - Checks sufficient leave balance before creating request
   - Prevents overlapping leave requests
   - Calculates total days automatically

3. **Approval Workflow**:
   - Manager must approve/reject leave requests
   - Approved leaves deduct from leave balance
   - Rejected leaves don't affect balance

### Authorization
- **Employee**: Can manage own attendance and leaves
- **Manager**: All employee features + approve team attendance and leaves
- **Administrator**: All features + user management, leave allocation, holiday management

---

## Test Credentials

| Role | Email | Password | Leave Balance |
|------|-------|----------|---------------|
| Admin | admin@attendance.com | Admin@123 | 12 Casual, 15 Earned |
| Manager | manager@attendance.com | Manager@123 | 12 Casual, 15 Earned |
| Employee | employee@attendance.com | Employee@123 | 12 Casual, 15 Earned |

---

## Error Handling

All endpoints return errors in this format:
```json
{
  "success": false,
  "data": null,
  "error": "Error message here",
  "message": "Additional context"
}
```

**Common Status Codes:**
- `200 OK`: Success
- `400 Bad Request`: Invalid input
- `401 Unauthorized`: Not authenticated or invalid token
- `403 Forbidden`: Not authorized for this action
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error
