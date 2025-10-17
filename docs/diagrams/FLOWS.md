# System Flow Diagrams

## 1. User Authentication Flow

```mermaid
sequenceDiagram
    participant U as User
    participant F as Frontend (Angular)
    participant A as API (.NET)
    participant DB as Database

    U->>F: Enter credentials
    F->>F: Validate input
    F->>A: POST /auth/login
    A->>DB: Query user by email
    DB-->>A: User data
    A->>A: Verify password hash
    alt Valid credentials
        A->>A: Generate JWT token
        A-->>F: Return token + user data
        F->>F: Store token (localStorage)
        F->>F: Redirect to dashboard
        F-->>U: Display dashboard
    else Invalid credentials
        A-->>F: Return error
        F-->>U: Show error message
    end
```

## 2. Attendance Login Flow

```mermaid
flowchart TD
    A[Employee clicks Login] --> B[Frontend captures click]
    B --> C[Send POST /attendance/login]
    C --> D{User authenticated?}
    D -->|No| E[Return 401 Unauthorized]
    D -->|Yes| F[Capture server timestamp]
    F --> G[Get current date]
    G --> H{Check if weekend}
    H -->|Yes| I[Set isWeekend = true]
    H -->|No| J{Check if public holiday}
    J -->|Yes| K[Set isPublicHoliday = true]
    J -->|No| L[Set both flags false]
    I --> M[Save attendance record]
    K --> M
    L --> M
    M --> N{Is weekend or holiday?}
    N -->|Yes| O[Mark for comp-off eligibility]
    N -->|No| P[Regular attendance]
    O --> Q[Return success response]
    P --> Q
    Q --> R[Update UI with confirmation]
    R --> S[Show login time & comp-off status]
```

## 3. Attendance Logout Flow

```mermaid
flowchart TD
    A[Employee clicks Logout] --> B[Frontend captures click]
    B --> C[Get active attendance ID]
    C --> D{Has logged in today?}
    D -->|No| E[Show error: No active login]
    D -->|Yes| F[Send POST /attendance/logout]
    F --> G[Capture server timestamp]
    G --> H[Update attendance record]
    H --> I[Calculate work duration]
    I --> J{Is weekend/holiday attendance?}
    J -->|Yes| K[Add 1 day to comp-off balance]
    J -->|No| L[No comp-off update]
    K --> M[Save changes]
    L --> M
    M --> N[Return success response]
    N --> O[Update UI]
    O --> P[Show logout time & duration]
    P --> Q{Earned comp-off?}
    Q -->|Yes| R[Show comp-off earned notification]
    Q -->|No| S[Show regular logout confirmation]
```

## 4. Leave Request Flow

```mermaid
sequenceDiagram
    participant E as Employee
    participant F as Frontend
    participant A as API
    participant DB as Database
    participant N as Notification Service

    E->>F: Fill leave request form
    F->>F: Validate dates & reason
    F->>A: POST /leave/request
    A->>DB: Get leave balance
    DB-->>A: Current balance
    A->>A: Calculate total days
    alt Sufficient balance
        A->>DB: Create leave request (Pending)
        A->>DB: Reserve leave balance
        DB-->>A: Success
        A->>N: Notify manager
        A-->>F: Return success
        F-->>E: Show confirmation
    else Insufficient balance
        A-->>F: Return error
        F-->>E: Show insufficient balance error
    end
```

## 5. Leave Approval Workflow

```mermaid
flowchart TD
    A[Employee submits leave] --> B[Status: Pending]
    B --> C[Notify Manager]
    C --> D[Manager views request]
    D --> E{Manager decision}
    E -->|Approve| F[Update status: Approved]
    E -->|Reject| G[Update status: Rejected]
    F --> H[Deduct from leave balance]
    G --> I[Restore reserved balance]
    H --> J[Notify employee: Approved]
    I --> K[Notify employee: Rejected]
    J --> L[Update calendar]
    K --> M[Show rejection reason]
    L --> N[End]
    M --> N
```

## 6. Manager Approval Process

```mermaid
sequenceDiagram
    participant M as Manager
    participant F as Frontend
    participant A as API
    participant DB as Database
    participant N as Notification

    M->>F: View pending approvals
    F->>A: GET /manager/pending-leave-requests
    A->>DB: Query pending requests
    DB-->>A: Pending requests
    A-->>F: Return requests list
    F-->>M: Display pending requests
    M->>F: Click Approve/Reject
    alt Approve
        F->>A: POST /manager/approve-leave/{id}
        A->>DB: Update status to Approved
        A->>DB: Deduct from leave balance
    else Reject
        F->>A: POST /manager/reject-leave/{id}
        A->>DB: Update status to Rejected
        A->>DB: Restore leave balance
    end
    A->>N: Notify employee
    A-->>F: Return success
    F-->>M: Show confirmation
```

## 7. Admin User Creation Flow

```mermaid
flowchart TD
    A[Admin clicks Create User] --> B[Fill user details form]
    B --> C[Select role & manager]
    C --> D[Submit form]
    D --> E[Frontend validates input]
    E --> F{Validation passed?}
    F -->|No| G[Show validation errors]
    F -->|Yes| H[POST /admin/users]
    H --> I{Email already exists?}
    I -->|Yes| J[Return error: Duplicate email]
    I -->|No| K[Hash temporary password]
    K --> L[Create user record]
    L --> M[Create leave entitlements]
    M --> N{User is Employee/Manager?}
    N -->|Yes| O[Assign default leave balance]
    N -->|No| P[Skip leave allocation]
    O --> Q[Save to database]
    P --> Q
    Q --> R[Send welcome email]
    R --> S[Return success]
    S --> T[Show user created confirmation]
    J --> U[Show error message]
    G --> V[User corrects input]
    V --> D
```

## 8. Holiday Calendar Management

```mermaid
flowchart TD
    A[Admin accesses Holiday Management] --> B[View current holidays]
    B --> C{Action?}
    C -->|Add| D[Fill holiday form]
    C -->|Edit| E[Select holiday to edit]
    C -->|Delete| F[Select holiday to delete]
    D --> G[Enter date, name, description]
    G --> H[Submit]
    H --> I{Date already exists?}
    I -->|Yes| J[Show error: Duplicate date]
    I -->|No| K[POST /admin/holidays]
    K --> L[Save to database]
    L --> M[Update attendance system]
    M --> N[Return success]
    N --> O[Refresh holiday list]
    E --> P[Update holiday details]
    P --> Q[PUT /admin/holidays/{id}]
    Q --> L
    F --> R{Confirm deletion?}
    R -->|Yes| S[DELETE /admin/holidays/{id}]
    R -->|No| O
    S --> T[Remove from database]
    T --> O
    J --> U[Admin corrects date]
    U --> H
```

## 9. Compensatory Off Accrual

```mermaid
flowchart TD
    A[Employee logs in] --> B{Is today weekend?}
    B -->|Yes| C[Mark attendance.isWeekend = true]
    B -->|No| D{Is today public holiday?}
    D -->|Yes| E[Mark attendance.isPublicHoliday = true]
    D -->|No| F[Regular attendance]
    C --> G[Employee works]
    E --> G
    F --> H[End - No comp-off]
    G --> I[Employee logs out]
    I --> J[Calculate work duration]
    J --> K{Worked >= 4 hours?}
    K -->|Yes| L[Add 1 day to comp-off balance]
    K -->|No| M[No comp-off earned]
    L --> N[Update LeaveEntitlements table]
    N --> O[Notify employee: Comp-off earned]
    M --> P[Notify: Insufficient hours]
    O --> Q[End]
    P --> Q
```

## 10. Role-Based Access Control

```mermaid
flowchart TD
    A[User makes API request] --> B[Extract JWT token]
    B --> C{Token valid?}
    C -->|No| D[Return 401 Unauthorized]
    C -->|Yes| E[Decode token]
    E --> F[Extract user role]
    F --> G{Check endpoint permission}
    G -->|Employee| H{Employee-accessible endpoint?}
    G -->|Manager| I{Manager-accessible endpoint?}
    G -->|Admin| J{Admin-accessible endpoint?}
    H -->|Yes| K{Accessing own data?}
    H -->|No| L[Return 403 Forbidden]
    I -->|Yes| M{Accessing team data?}
    I -->|No| L
    J -->|Yes| N[Allow access]
    J -->|No| L
    K -->|Yes| N
    K -->|No| L
    M -->|Yes| N
    M -->|No| L
    N --> O[Execute business logic]
    O --> P[Return response]
```

## 11. System Startup Flow

```mermaid
sequenceDiagram
    participant U as User
    participant F as Frontend
    participant A as API
    participant DB as Database

    U->>F: Access application URL
    F->>F: Load Angular app
    F->>F: Check stored token
    alt Has valid token
        F->>A: Verify token
        A->>A: Validate JWT
        alt Token valid
            A-->>F: Token valid
            F->>A: GET /user/profile
            A->>DB: Get user data
            DB-->>A: User data
            A-->>F: Return profile
            F->>F: Set user context
            F-->>U: Redirect to dashboard
        else Token invalid
            A-->>F: Token invalid
            F->>F: Clear storage
            F-->>U: Show login page
        end
    else No token
        F-->>U: Show login page
    end
```

## 12. Data Synchronization Flow

```mermaid
flowchart TD
    A[Cron Job Triggers] --> B{Daily midnight task}
    B --> C[Check all pending attendance]
    C --> D{Attendance > 7 days old?}
    D -->|Yes| E[Auto-approve attendance]
    D -->|No| F[Skip]
    E --> G[Update status to Approved]
    G --> H[Notify employee & manager]
    F --> I[Check leave balances]
    H --> I
    I --> J{New year started?}
    J -->|Yes| K[Create new year entitlements]
    J -->|No| L[Skip]
    K --> M[Copy rollover leaves]
    M --> N[Add annual allocation]
    N --> O[Save new balances]
    L --> P[Check upcoming holidays]
    O --> P
    P --> Q{Holiday in next 7 days?}
    Q -->|Yes| R[Send reminder notification]
    Q -->|No| S[End]
    R --> S
```

---

## Diagram Legend

- **Rectangles**: Process steps
- **Diamonds**: Decision points
- **Rounded rectangles**: Start/End points
- **Arrows**: Flow direction
- **Participants**: System components

---

**Last Updated**: October 15, 2025
