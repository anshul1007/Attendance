using AttendanceAPI.Data;
using AttendanceAPI.Models.DTOs;
using AttendanceAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceAPI.Services
{
    public interface IAdminService
    {
        Task<UserDto> CreateUserAsync(CreateUserRequest request);
        Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<bool> AllocateLeaveEntitlementAsync(AllocateLeaveRequest request);
        Task<LeaveBalanceDto?> GetUserLeaveBalanceAsync(Guid userId, int year);
        Task<PublicHolidayDto> CreatePublicHolidayAsync(CreatePublicHolidayRequest request);
        Task<List<PublicHolidayDto>> GetPublicHolidaysAsync(int year);
        Task<bool> DeletePublicHolidayAsync(Guid holidayId);
        Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request);
        Task<DepartmentDto> UpdateDepartmentAsync(Guid departmentId, UpdateDepartmentRequest request);
        Task<List<DepartmentDto>> GetAllDepartmentsAsync();
        Task<bool> DeleteDepartmentAsync(Guid departmentId);
        Task<List<TeamMemberDto>> GetAllTeamMembersAsync();
        Task AssignCompensatoryOffAsync(Guid employeeId, decimal days, string reason);
        Task LogPastAttendanceAsync(Guid employeeId, string date, string loginTime, string? logoutTime);
        Task<List<AttendanceDto>> GetTeamAttendanceHistoryAsync(DateTime startDate, DateTime endDate);
        Task<List<LeaveRequestResponse>> GetTeamLeaveHistoryAsync(DateTime startDate, DateTime endDate);
        Task<List<AttendanceDto>> GetPendingAttendanceApprovalsAsync();
        Task<AttendanceDto> ApproveAttendanceAsync(Guid attendanceId);
        Task<AttendanceDto> RejectAttendanceAsync(Guid attendanceId, string? reason);
        Task<List<LeaveRequestResponse>> GetPendingLeaveRequestsAsync();
        Task<LeaveRequestResponse> ApproveOrRejectLeaveAsync(Guid leaveRequestId, bool approved, string? rejectionReason);
    }

    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminService> _logger;

        public AdminService(ApplicationDbContext context, ILogger<AdminService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            // Check if employee ID already exists
            if (await _context.Users.AnyAsync(u => u.EmployeeId == request.EmployeeId))
            {
                throw new InvalidOperationException("A user with this employee ID already exists.");
            }

            // Validate role
            if (!Enum.IsDefined(typeof(UserRole), request.Role))
            {
                throw new InvalidOperationException("Invalid role.");
            }

            // Validate manager if specified
            if (request.ManagerId.HasValue)
            {
                var manager = await _context.Users.FindAsync(request.ManagerId.Value);
                if (manager == null)
                {
                    throw new InvalidOperationException("Manager not found.");
                }
                if (manager.Role != UserRole.Manager && manager.Role != UserRole.Administrator)
                {
                    throw new InvalidOperationException("The specified manager must have Manager or Administrator role.");
                }
            }

            // Validate department if specified
            if (request.DepartmentId.HasValue)
            {
                var department = await _context.Departments.FindAsync(request.DepartmentId.Value);
                if (department == null || !department.IsActive)
                {
                    throw new InvalidOperationException("Department not found or inactive.");
                }
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmployeeId = request.EmployeeId,
                Role = (UserRole)request.Role,
                ManagerId = request.ManagerId,
                DepartmentId = request.DepartmentId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new user {Email} with role {Role}", user.Email, user.Role);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmployeeId = user.EmployeeId,
                Role = user.Role.ToString(),
                ManagerId = user.ManagerId
            };
        }

        public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            if (request.Email != null)
            {
                if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId))
                {
                    throw new InvalidOperationException("A user with this email already exists.");
                }
                user.Email = request.Email;
            }

            if (request.FirstName != null) user.FirstName = request.FirstName;
            if (request.LastName != null) user.LastName = request.LastName;

            if (request.Role.HasValue)
            {
                if (!Enum.IsDefined(typeof(UserRole), request.Role.Value))
                {
                    throw new InvalidOperationException("Invalid role.");
                }
                user.Role = (UserRole)request.Role.Value;
            }

            if (request.ManagerId.HasValue)
            {
                var manager = await _context.Users.FindAsync(request.ManagerId.Value);
                if (manager == null)
                {
                    throw new InvalidOperationException("Manager not found.");
                }
                user.ManagerId = request.ManagerId;
            }

            if (request.DepartmentId.HasValue)
            {
                var department = await _context.Departments.FindAsync(request.DepartmentId.Value);
                if (department == null || !department.IsActive)
                {
                    throw new InvalidOperationException("Department not found or inactive.");
                }
                user.DepartmentId = request.DepartmentId;
            }

            if (request.IsActive.HasValue)
            {
                user.IsActive = request.IsActive.Value;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated user {UserId}", userId);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmployeeId = user.EmployeeId,
                Role = user.Role.ToString(),
                ManagerId = user.ManagerId
            };
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.Department)
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                EmployeeId = u.EmployeeId,
                Role = u.Role.ToString(),
                ManagerId = u.ManagerId,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department?.Name
            }).ToList();
        }

        public async Task<bool> AllocateLeaveEntitlementAsync(AllocateLeaveRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var existingEntitlement = await _context.LeaveEntitlements
                .FirstOrDefaultAsync(e => e.UserId == request.UserId && e.Year == request.Year);

            if (existingEntitlement != null)
            {
                // Update existing entitlement
                existingEntitlement.CasualLeaveBalance = request.CasualLeaveBalance;
                existingEntitlement.EarnedLeaveBalance = request.EarnedLeaveBalance;
                existingEntitlement.CompensatoryOffBalance = request.CompensatoryOffBalance;
                existingEntitlement.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Updated leave entitlement for user {UserId} for year {Year}", request.UserId, request.Year);
            }
            else
            {
                // Create new entitlement
                var entitlement = new LeaveEntitlement
                {
                    UserId = request.UserId,
                    Year = request.Year,
                    CasualLeaveBalance = request.CasualLeaveBalance,
                    EarnedLeaveBalance = request.EarnedLeaveBalance,
                    CompensatoryOffBalance = request.CompensatoryOffBalance,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.LeaveEntitlements.Add(entitlement);
                _logger.LogInformation("Created leave entitlement for user {UserId} for year {Year}", request.UserId, request.Year);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<LeaveBalanceDto?> GetUserLeaveBalanceAsync(Guid userId, int year)
        {
            var entitlement = await _context.LeaveEntitlements
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Year == year);

            if (entitlement == null)
            {
                return null;
            }

            return new LeaveBalanceDto
            {
                Year = entitlement.Year,
                CasualLeaveBalance = entitlement.CasualLeaveBalance,
                EarnedLeaveBalance = entitlement.EarnedLeaveBalance,
                CompensatoryOffBalance = entitlement.CompensatoryOffBalance
            };
        }

        public async Task<PublicHolidayDto> CreatePublicHolidayAsync(CreatePublicHolidayRequest request)
        {
            // Check if holiday already exists for this date
            if (await _context.PublicHolidays.AnyAsync(h => h.Date == request.Date))
            {
                throw new InvalidOperationException("A public holiday already exists for this date.");
            }

            var holiday = new PublicHoliday
            {
                Date = request.Date,
                Name = request.Name,
                Description = request.Description,
                Year = request.Date.Year,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.PublicHolidays.Add(holiday);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created public holiday {Name} on {Date}", holiday.Name, holiday.Date);

            return new PublicHolidayDto
            {
                Id = holiday.Id,
                Date = holiday.Date,
                Name = holiday.Name,
                Description = holiday.Description,
                Year = holiday.Year,
                IsActive = holiday.IsActive
            };
        }

        public async Task<List<PublicHolidayDto>> GetPublicHolidaysAsync(int year)
        {
            var holidays = await _context.PublicHolidays
                .Where(h => h.Year == year && h.IsActive)
                .OrderBy(h => h.Date)
                .ToListAsync();

            return holidays.Select(h => new PublicHolidayDto
            {
                Id = h.Id,
                Date = h.Date,
                Name = h.Name,
                Description = h.Description,
                Year = h.Year,
                IsActive = h.IsActive
            }).ToList();
        }

        public async Task<bool> DeletePublicHolidayAsync(Guid holidayId)
        {
            var holiday = await _context.PublicHolidays.FindAsync(holidayId);
            if (holiday == null)
            {
                throw new InvalidOperationException("Public holiday not found.");
            }

            holiday.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted public holiday {HolidayId}", holidayId);
            return true;
        }

        // Department Management
        public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentRequest request)
        {
            var department = new Department
            {
                Name = request.Name,
                Description = request.Description,
                WeeklyOffDays = request.WeeklyOffDays,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created department {DepartmentId} - {Name}", department.Id, department.Name);

            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                WeeklyOffDays = department.WeeklyOffDays,
                IsActive = department.IsActive,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt
            };
        }

        public async Task<DepartmentDto> UpdateDepartmentAsync(Guid departmentId, UpdateDepartmentRequest request)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
            {
                throw new InvalidOperationException("Department not found.");
            }

            department.Name = request.Name;
            department.Description = request.Description;
            department.WeeklyOffDays = request.WeeklyOffDays;
            department.IsActive = request.IsActive;
            department.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated department {DepartmentId}", departmentId);

            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                WeeklyOffDays = department.WeeklyOffDays,
                IsActive = department.IsActive,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt
            };
        }

        public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var departments = await _context.Departments
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();

            return departments.Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                WeeklyOffDays = d.WeeklyOffDays,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            }).ToList();
        }

        public async Task<bool> DeleteDepartmentAsync(Guid departmentId)
        {
            var department = await _context.Departments.FindAsync(departmentId);
            if (department == null)
            {
                throw new InvalidOperationException("Department not found.");
            }

            // Check if department has employees
            var hasEmployees = await _context.Users.AnyAsync(u => u.DepartmentId == departmentId && u.IsActive);
            if (hasEmployees)
            {
                throw new InvalidOperationException("Cannot delete department with active employees. Please reassign employees first.");
            }

            department.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted department {DepartmentId}", departmentId);
            return true;
        }

        public async Task<List<TeamMemberDto>> GetAllTeamMembersAsync()
        {
            var currentYear = DateTime.Now.Year;
            var today = DateOnly.FromDateTime(DateTime.Now);

            var teamMembers = await _context.Users
                .Where(u => u.IsActive && u.Role != UserRole.Administrator)
                .OrderBy(u => u.EmployeeId)
                .Select(u => new TeamMemberDto
                {
                    Id = u.Id,
                    EmployeeId = u.EmployeeId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    CasualLeaveBalance = _context.LeaveEntitlements
                        .Where(le => le.UserId == u.Id && le.Year == currentYear)
                        .Select(le => (decimal?)le.CasualLeaveBalance)
                        .FirstOrDefault(),
                    EarnedLeaveBalance = _context.LeaveEntitlements
                        .Where(le => le.UserId == u.Id && le.Year == currentYear)
                        .Select(le => (decimal?)le.EarnedLeaveBalance)
                        .FirstOrDefault(),
                    CompensatoryOffBalance = _context.LeaveEntitlements
                        .Where(le => le.UserId == u.Id && le.Year == currentYear)
                        .Select(le => (decimal?)le.CompensatoryOffBalance)
                        .FirstOrDefault(),
                    UpcomingLeaves = _context.LeaveRequests
                        .Where(lr => lr.UserId == u.Id && 
                                   lr.StartDate >= today && 
                                   (lr.Status == ApprovalStatus.Pending || lr.Status == ApprovalStatus.Approved))
                        .OrderBy(lr => lr.StartDate)
                        .Take(5)
                        .Select(lr => new UpcomingLeaveDto
                        {
                            Id = lr.Id,
                            LeaveType = lr.LeaveType.ToString(),
                            StartDate = lr.StartDate,
                            EndDate = lr.EndDate,
                            TotalDays = lr.TotalDays,
                            Status = lr.Status.ToString()
                        })
                        .ToList()
                })
                .ToListAsync();

            return teamMembers;
        }

        public async Task AssignCompensatoryOffAsync(Guid employeeId, decimal days, string reason)
        {
            var employee = await _context.Users.FindAsync(employeeId);
            if (employee == null || !employee.IsActive)
            {
                throw new InvalidOperationException("Employee not found or inactive");
            }

            var currentYear = DateTime.Now.Year;
            var entitlement = await _context.LeaveEntitlements
                .FirstOrDefaultAsync(e => e.UserId == employeeId && e.Year == currentYear);

            if (entitlement == null)
            {
                throw new InvalidOperationException("Leave entitlement not found for current year");
            }

            entitlement.CompensatoryOffBalance += days;
            entitlement.UpdatedAt = DateTime.UtcNow;

            var auditLog = new AuditLog
            {
                UserId = employeeId,
                Action = "Compensatory Off Assigned",
                EntityType = "LeaveEntitlement",
                EntityId = entitlement.Id.ToString(),
                NewValue = $"Admin assigned {days} compensatory off day(s). Reason: {reason}",
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assigned {Days} comp off days to employee {EmployeeId}", days, employeeId);
        }

        public async Task LogPastAttendanceAsync(Guid employeeId, string date, string loginTime, string? logoutTime)
        {
            var employee = await _context.Users.FindAsync(employeeId);
            if (employee == null || !employee.IsActive)
            {
                throw new InvalidOperationException("Employee not found or inactive");
            }

            if (!DateOnly.TryParse(date, out var attendanceDate))
            {
                throw new ArgumentException("Invalid date format");
            }

            if (!TimeOnly.TryParse(loginTime, out var parsedLoginTime))
            {
                throw new ArgumentException("Invalid login time format");
            }

            TimeOnly? parsedLogoutTime = null;
            if (!string.IsNullOrWhiteSpace(logoutTime) && !TimeOnly.TryParse(logoutTime, out var tempLogoutTime))
            {
                throw new ArgumentException("Invalid logout time format");
            }
            else if (!string.IsNullOrWhiteSpace(logoutTime))
            {
                parsedLogoutTime = TimeOnly.Parse(logoutTime);
            }

            // Check if attendance already exists
            var existingAttendance = await _context.Attendance
                .FirstOrDefaultAsync(a => a.UserId == employeeId && a.Date == attendanceDate);

            if (existingAttendance != null)
            {
                throw new InvalidOperationException("Attendance record already exists for this date");
            }

            // Check if date is a public holiday
            var isHoliday = await _context.PublicHolidays
                .AnyAsync(h => h.Date == attendanceDate && h.Year == attendanceDate.Year && h.IsActive);

            // Check if it's a weekend (Saturday or Sunday)
            var dayOfWeek = attendanceDate.DayOfWeek;
            var isWeekend = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;

            var attendance = new Attendance
            {
                UserId = employeeId,
                Date = attendanceDate,
                LoginTime = DateTime.SpecifyKind(attendanceDate.ToDateTime(parsedLoginTime), DateTimeKind.Utc),
                LogoutTime = parsedLogoutTime.HasValue 
                    ? DateTime.SpecifyKind(attendanceDate.ToDateTime(parsedLogoutTime.Value), DateTimeKind.Utc) 
                    : null,
                Status = ApprovalStatus.Approved,
                IsWeekend = isWeekend,
                IsPublicHoliday = isHoliday,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Attendance.Add(attendance);

            var auditLog = new AuditLog
            {
                UserId = employeeId,
                Action = "Past Attendance Logged",
                EntityType = "Attendance",
                EntityId = attendance.Id.ToString(),
                NewValue = $"Admin logged past attendance for {attendanceDate:yyyy-MM-dd}. Login: {parsedLoginTime}, Logout: {parsedLogoutTime?.ToString() ?? "Not set"}",
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Logged past attendance for employee {EmployeeId} on {Date}", employeeId, attendanceDate);
        }

        public async Task<List<AttendanceDto>> GetTeamAttendanceHistoryAsync(DateTime startDate, DateTime endDate)
        {
            var attendanceRecords = await _context.Attendance
                .Include(a => a.User)
                .Include(a => a.Approver)
                .Where(a => a.Date >= DateOnly.FromDateTime(startDate) && 
                           a.Date <= DateOnly.FromDateTime(endDate))
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.User.FirstName)
                .ToListAsync();

            return attendanceRecords.Select(a => new AttendanceDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = $"{a.User.FirstName} {a.User.LastName}",
                EmployeeId = a.User.EmployeeId,
                LoginTime = a.LoginTime,
                LogoutTime = a.LogoutTime,
                Date = a.Date,
                IsWeekend = a.IsWeekend,
                IsPublicHoliday = a.IsPublicHoliday,
                Status = a.Status.ToString(),
                ApproverName = a.Approver != null ? $"{a.Approver.FirstName} {a.Approver.LastName}" : null,
                ApprovedAt = a.ApprovedAt,
                WorkDuration = a.LogoutTime.HasValue ? a.LogoutTime.Value - a.LoginTime : null
            }).ToList();
        }

        public async Task<List<LeaveRequestResponse>> GetTeamLeaveHistoryAsync(DateTime startDate, DateTime endDate)
        {
            var startDateOnly = DateOnly.FromDateTime(startDate);
            var endDateOnly = DateOnly.FromDateTime(endDate);

            var leaveRequests = await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.Approver)
                .Where(lr => lr.StartDate <= endDateOnly && lr.EndDate >= startDateOnly)
                .OrderByDescending(lr => lr.StartDate)
                .ToListAsync();

            return leaveRequests.Select(lr => new LeaveRequestResponse
            {
                Id = lr.Id,
                UserId = lr.UserId,
                EmployeeId = lr.User.EmployeeId,
                UserName = $"{lr.User.FirstName} {lr.User.LastName}",
                LeaveType = lr.LeaveType.ToString(),
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                TotalDays = lr.TotalDays,
                Reason = lr.Reason,
                Status = lr.Status.ToString(),
                CreatedAt = lr.CreatedAt,
                ApprovedAt = lr.ApprovedAt,
                RejectionReason = lr.RejectionReason,
                ApproverName = lr.Approver != null ? $"{lr.Approver.FirstName} {lr.Approver.LastName}" : null
            }).ToList();
        }

        public async Task<List<AttendanceDto>> GetPendingAttendanceApprovalsAsync()
        {
            var pendingAttendances = await _context.Attendance
                .Include(a => a.User)
                .Include(a => a.Approver)
                .Where(a => a.Status == ApprovalStatus.Pending)
                .OrderBy(a => a.Date)
                .ToListAsync();

            return pendingAttendances.Select(a => new AttendanceDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserName = $"{a.User.FirstName} {a.User.LastName}",
                EmployeeId = a.User.EmployeeId,
                LoginTime = a.LoginTime,
                LogoutTime = a.LogoutTime,
                Date = a.Date,
                IsWeekend = a.IsWeekend,
                IsPublicHoliday = a.IsPublicHoliday,
                Status = a.Status.ToString(),
                ApproverName = a.Approver != null ? $"{a.Approver.FirstName} {a.Approver.LastName}" : null,
                ApprovedAt = a.ApprovedAt,
                WorkDuration = a.LogoutTime.HasValue ? a.LogoutTime.Value - a.LoginTime : null
            }).ToList();
        }

        public async Task<AttendanceDto> ApproveAttendanceAsync(Guid attendanceId)
        {
            var attendance = await _context.Attendance
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == attendanceId);

            if (attendance == null)
            {
                throw new InvalidOperationException("Attendance record not found.");
            }

            if (attendance.Status != ApprovalStatus.Pending)
            {
                throw new InvalidOperationException($"Attendance is already {attendance.Status}.");
            }

            attendance.Status = ApprovalStatus.Approved;
            attendance.ApprovedAt = DateTime.UtcNow;
            attendance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Attendance {AttendanceId} approved by admin", attendanceId);

            return new AttendanceDto
            {
                Id = attendance.Id,
                UserId = attendance.UserId,
                UserName = $"{attendance.User.FirstName} {attendance.User.LastName}",
                EmployeeId = attendance.User.EmployeeId,
                LoginTime = attendance.LoginTime,
                LogoutTime = attendance.LogoutTime,
                Date = attendance.Date,
                IsWeekend = attendance.IsWeekend,
                IsPublicHoliday = attendance.IsPublicHoliday,
                Status = attendance.Status.ToString(),
                ApprovedAt = attendance.ApprovedAt,
                WorkDuration = attendance.LogoutTime.HasValue ? attendance.LogoutTime.Value - attendance.LoginTime : null
            };
        }

        public async Task<AttendanceDto> RejectAttendanceAsync(Guid attendanceId, string? reason)
        {
            var attendance = await _context.Attendance
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == attendanceId);

            if (attendance == null)
            {
                throw new InvalidOperationException("Attendance record not found.");
            }

            if (attendance.Status != ApprovalStatus.Pending)
            {
                throw new InvalidOperationException($"Attendance is already {attendance.Status}.");
            }

            attendance.Status = ApprovalStatus.Rejected;
            attendance.ApprovedAt = DateTime.UtcNow;
            attendance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Attendance {AttendanceId} rejected by admin. Reason: {Reason}", attendanceId, reason);

            return new AttendanceDto
            {
                Id = attendance.Id,
                UserId = attendance.UserId,
                UserName = $"{attendance.User.FirstName} {attendance.User.LastName}",
                EmployeeId = attendance.User.EmployeeId,
                LoginTime = attendance.LoginTime,
                LogoutTime = attendance.LogoutTime,
                Date = attendance.Date,
                IsWeekend = attendance.IsWeekend,
                IsPublicHoliday = attendance.IsPublicHoliday,
                Status = attendance.Status.ToString(),
                ApprovedAt = attendance.ApprovedAt,
                WorkDuration = attendance.LogoutTime.HasValue ? attendance.LogoutTime.Value - attendance.LoginTime : null
            };
        }

        public async Task<List<LeaveRequestResponse>> GetPendingLeaveRequestsAsync()
        {
            var leaveRequests = await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.Approver)
                .Where(lr => lr.Status == ApprovalStatus.Pending)
                .OrderBy(lr => lr.CreatedAt)
                .ToListAsync();

            return leaveRequests.Select(lr => new LeaveRequestResponse
            {
                Id = lr.Id,
                UserId = lr.UserId,
                UserName = $"{lr.User.FirstName} {lr.User.LastName}",
                EmployeeId = lr.User.EmployeeId,
                LeaveType = lr.LeaveType.ToString(),
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                TotalDays = lr.TotalDays,
                Reason = lr.Reason,
                Status = lr.Status.ToString(),
                CreatedAt = lr.CreatedAt,
                ApprovedAt = lr.ApprovedAt,
                RejectionReason = lr.RejectionReason,
                ApproverName = lr.Approver != null ? $"{lr.Approver.FirstName} {lr.Approver.LastName}" : null
            }).ToList();
        }

        public async Task<LeaveRequestResponse> ApproveOrRejectLeaveAsync(Guid leaveRequestId, bool approved, string? rejectionReason)
        {
            var leaveRequest = await _context.LeaveRequests
                .Include(lr => lr.User)
                .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId);

            if (leaveRequest == null)
            {
                throw new InvalidOperationException("Leave request not found.");
            }

            if (leaveRequest.Status != ApprovalStatus.Pending)
            {
                throw new InvalidOperationException($"Leave request is already {leaveRequest.Status}.");
            }

            var year = leaveRequest.StartDate.Year;
            var entitlement = await _context.LeaveEntitlements
                .FirstOrDefaultAsync(e => e.UserId == leaveRequest.UserId && e.Year == year);

            if (entitlement == null)
            {
                throw new InvalidOperationException("Leave entitlement not found.");
            }

            if (approved)
            {
                // Deduct leave balance
                switch (leaveRequest.LeaveType)
                {
                    case LeaveType.CasualLeave:
                        entitlement.CasualLeaveBalance -= leaveRequest.TotalDays;
                        break;
                    case LeaveType.EarnedLeave:
                        entitlement.EarnedLeaveBalance -= leaveRequest.TotalDays;
                        break;
                    case LeaveType.CompensatoryOff:
                        entitlement.CompensatoryOffBalance -= leaveRequest.TotalDays;
                        break;
                }

                leaveRequest.Status = ApprovalStatus.Approved;
                _logger.LogInformation("Leave request {LeaveRequestId} approved by admin", leaveRequestId);
            }
            else
            {
                leaveRequest.Status = ApprovalStatus.Rejected;
                leaveRequest.RejectionReason = rejectionReason;
                _logger.LogInformation("Leave request {LeaveRequestId} rejected by admin. Reason: {Reason}", leaveRequestId, rejectionReason);
            }

            leaveRequest.ApprovedAt = DateTime.UtcNow;
            leaveRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new LeaveRequestResponse
            {
                Id = leaveRequest.Id,
                UserId = leaveRequest.UserId,
                UserName = $"{leaveRequest.User.FirstName} {leaveRequest.User.LastName}",
                EmployeeId = leaveRequest.User.EmployeeId,
                LeaveType = leaveRequest.LeaveType.ToString(),
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                TotalDays = leaveRequest.TotalDays,
                Reason = leaveRequest.Reason,
                Status = leaveRequest.Status.ToString(),
                CreatedAt = leaveRequest.CreatedAt,
                ApprovedAt = leaveRequest.ApprovedAt,
                RejectionReason = leaveRequest.RejectionReason
            };
        }
    }
}
