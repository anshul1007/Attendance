# System Architecture

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Client Layer                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │           Angular 18 Frontend (SPA)                   │   │
│  │  ┌──────────┐  ┌──────────┐  ┌─────────────────┐   │   │
│  │  │ Employee │  │ Manager  │  │  Administrator  │   │   │
│  │  │   View   │  │   View   │  │      View       │   │   │
│  │  └──────────┘  └──────────┘  └─────────────────┘   │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                           │
                           │ HTTPS/REST API
                           │ JWT Authentication
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│  ┌──────────────────────────────────────────────────────┐   │
│  │            .NET 8 Web API                             │   │
│  │  ┌──────────────┐  ┌─────────────────────────────┐  │   │
│  │  │ Controllers  │  │      Services Layer         │  │   │
│  │  │              │  │  ┌────────────────────────┐ │  │   │
│  │  │ - Auth       │  │  │ - AuthService          │ │  │   │
│  │  │ - Attendance │  │  │ - AttendanceService    │ │  │   │
│  │  │ - Leave      │  │  │ - LeaveService         │ │  │   │
│  │  │ - Admin      │  │  │ - UserService          │ │  │   │
│  │  │ - User       │  │  │ - HolidayService       │ │  │   │
│  │  └──────────────┘  │  └────────────────────────┘ │  │   │
│  │                    └─────────────────────────────┘  │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                           │
                           │ Entity Framework Core
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                    Data Layer                                │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              Database (SQL Server)                    │   │
│  │  ┌────────┐ ┌────────┐ ┌──────┐ ┌────────────────┐  │   │
│  │  │ Users  │ │Attend- │ │Leave │ │    Holidays    │  │   │
│  │  │        │ │ ance   │ │      │ │                │  │   │
│  │  └────────┘ └────────┘ └──────┘ └────────────────┘  │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Component Architecture

### Frontend Architecture (Angular 18)

```
frontend/
├── src/
│   ├── app/
│   │   ├── core/                    # Singleton services, guards
│   │   │   ├── auth/
│   │   │   │   ├── auth.service.ts
│   │   │   │   ├── auth.guard.ts
│   │   │   │   └── role.guard.ts
│   │   │   ├── interceptors/
│   │   │   │   ├── auth.interceptor.ts
│   │   │   │   └── error.interceptor.ts
│   │   │   └── services/
│   │   │       └── api.service.ts
│   │   ├── shared/                  # Shared components, pipes
│   │   │   ├── components/
│   │   │   ├── directives/
│   │   │   └── pipes/
│   │   ├── features/                # Feature modules
│   │   │   ├── auth/
│   │   │   │   ├── login/
│   │   │   │   └── auth.module.ts
│   │   │   ├── employee/
│   │   │   │   ├── attendance/
│   │   │   │   ├── leave-request/
│   │   │   │   └── dashboard/
│   │   │   ├── manager/
│   │   │   │   ├── approvals/
│   │   │   │   ├── team-reports/
│   │   │   │   └── manager.module.ts
│   │   │   └── admin/
│   │   │       ├── user-management/
│   │   │       ├── holiday-management/
│   │   │       └── admin.module.ts
│   │   └── app.component.ts
│   ├── environments/
│   └── assets/
```

### Backend Architecture (.NET 8)

```
backend/
├── AttendanceAPI/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── AttendanceController.cs
│   │   ├── LeaveController.cs
│   │   ├── AdminController.cs
│   │   └── UserController.cs
│   ├── Services/
│   │   ├── Interfaces/
│   │   └── Implementations/
│   ├── Models/
│   │   ├── Entities/
│   │   ├── DTOs/
│   │   └── ViewModels/
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   └── Migrations/
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Program.cs
```

## Design Patterns

### Frontend
- **Component-Based Architecture**: Modular, reusable components
- **Service Pattern**: Business logic in injectable services
- **Guard Pattern**: Route protection and authorization
- **Interceptor Pattern**: HTTP request/response handling
- **Reactive Programming**: RxJS for async operations

### Backend
- **Repository Pattern**: Data access abstraction
- **Service Layer Pattern**: Business logic separation
- **Dependency Injection**: Loose coupling and testability
- **DTOs**: Data transfer objects for API contracts
- **Middleware Pipeline**: Cross-cutting concerns

## Security Architecture

### Authentication Flow

```
1. User Login
   └─> Frontend sends credentials
       └─> Backend validates
           └─> Generate JWT Token
               └─> Return token to frontend
                   └─> Store in localStorage/sessionStorage
                       └─> Include in subsequent requests (Authorization header)

2. Protected Route Access
   └─> AuthGuard checks token
       └─> If valid → Allow access
       └─> If invalid → Redirect to login
```

### Authorization Levels

```
┌─────────────────────────────────────────────────────┐
│                    Administrator                     │
│  - Full system access                                │
│  - User management                                   │
│  - System configuration                              │
└─────────────────────────────────────────────────────┘
                      ▲
                      │
┌─────────────────────────────────────────────────────┐
│                     Manager                          │
│  - All Employee features                             │
│  - Approve leave requests                            │
│  - Approve attendance                                │
│  - View team reports                                 │
└─────────────────────────────────────────────────────┘
                      ▲
                      │
┌─────────────────────────────────────────────────────┐
│                     Employee                         │
│  - Login/Logout (Attendance)                         │
│  - Request leave                                     │
│  - View own records                                  │
└─────────────────────────────────────────────────────┘
```

## Data Flow

### Attendance Recording Flow

```
Employee → Click "Login"
    ↓
Frontend captures action
    ↓
POST /api/attendance/login
    ↓
Backend Service:
    - Validate user
    - Capture system timestamp
    - Check if weekend/holiday
    - Save to database
    ↓
Response to Frontend
    ↓
Update UI with confirmation
```

### Leave Approval Workflow

```
Employee submits leave
    ↓
Status: "Pending"
    ↓
Notify Manager
    ↓
Manager Reviews
    ├─> Approve → Status: "Approved"
    └─> Reject → Status: "Rejected"
    ↓
Notify Employee
    ↓
Update leave balance (if approved)
```

## Technology Stack Details

### Frontend Technologies
- **Angular 18**: Latest version with standalone components
- **Angular Material**: UI component library
- **RxJS**: Reactive extensions
- **TypeScript**: Type-safe development
- **Chart.js/ng2-charts**: Data visualization

### Backend Technologies
- **.NET 8**: Latest LTS version
- **ASP.NET Core Web API**: RESTful services
- **Entity Framework Core 8**: ORM
- **SQL Server**: Primary database
- **AutoMapper**: Object-to-object mapping
- **FluentValidation**: Input validation
- **Serilog**: Structured logging

## Deployment Architecture

```
┌────────────────────────────────────────────────────┐
│              Load Balancer / CDN                    │
└────────────────────────────────────────────────────┘
                     │
        ┌────────────┴────────────┐
        ▼                         ▼
┌──────────────┐          ┌──────────────┐
│   Frontend   │          │   Backend    │
│  (Angular)   │          │  (.NET API)  │
│              │          │              │
│  - Azure     │          │  - Azure     │
│  - Static    │◄────────►│  - App       │
│  - Web Apps  │   API    │  - Service   │
└──────────────┘  Calls   └──────────────┘
                                 │
                                 ▼
                          ┌──────────────┐
                          │   Database   │
                          │ (SQL Server) │
                          └──────────────┘
```

## Scalability Considerations

1. **Horizontal Scaling**: Both frontend and backend can scale independently
2. **Caching**: Redis for session management and frequently accessed data
3. **CDN**: Static assets delivery
4. **Database**: Read replicas for reporting
5. **API Gateway**: Rate limiting and request routing

## Performance Optimization

- **Frontend**: Lazy loading, AOT compilation, tree shaking
- **Backend**: Response caching, query optimization, async operations
- **Database**: Proper indexing, stored procedures for complex queries

---

**Last Updated**: October 15, 2025
