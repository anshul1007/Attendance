using AttendanceAPI.Data;
using AttendanceAPI.Models.DTOs;
using AttendanceAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceAPI.Services
{
    public interface IApprovalService
    {
        Task<List<AttendanceDto>> GetPendingAttendanceApprovalsAsync(Guid managerId);
        Task<AttendanceDto> ApproveAttendanceAsync(Guid managerId, Guid attendanceId);
        Task<AttendanceDto> RejectAttendanceAsync(Guid managerId, Guid attendanceId);
        Task<List<AttendanceDto>> GetTeamAttendanceHistoryAsync(Guid managerId, DateTime startDate, DateTime endDate);
        Task<List<LeaveRequestResponse>> GetTeamLeaveHistoryAsync(Guid managerId, DateTime startDate, DateTime endDate);
        Task<List<TeamMemberDto>> GetTeamMembersAsync(Guid managerId);
        Task AssignCompensatoryOffAsync(Guid managerId, Guid employeeId, decimal days, string reason);
        Task LogPastAttendanceAsync(Guid managerId, Guid employeeId, DateOnly date, TimeOnly loginTime, TimeOnly? logoutTime);
    }

    public class ApprovalService : IApprovalService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApprovalService> _logger;

        public ApprovalService(ApplicationDbContext context, ILogger<ApprovalService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<AttendanceDto>> GetPendingAttendanceApprovalsAsync(Guid managerId)
        {
            // Get all subordinates
            var subordinateIds = await _context.Users
                .Where(u => u.ManagerId == managerId && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            var pendingAttendances = await _context.Attendance
                .Include(a => a.User)
                .Include(a => a.Approver)
                .Where(a => subordinateIds.Contains(a.UserId) && a.Status == ApprovalStatus.Pending)
                .OrderBy(a => a.Date)
                .ToListAsync();

            return pendingAttendances.Select(a => MapToDto(a, a.User, a.Approver)).ToList();
        }

        public async Task<AttendanceDto> ApproveAttendanceAsync(Guid managerId, Guid attendanceId)
        {
            var attendance = await _context.Attendance
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == attendanceId);

            if (attendance == null)
            {
                throw new InvalidOperationException("Attendance record not found.");
            }

            // Verify that the manager is the employee's manager
            if (attendance.User.ManagerId != managerId)
            {
                throw new InvalidOperationException("You are not authorized to approve this attendance.");
            }

            if (attendance.Status != ApprovalStatus.Pending)
            {
                throw new InvalidOperationException($"Attendance is already {attendance.Status}.");
            }

            attendance.Status = ApprovalStatus.Approved;
            attendance.ApprovedBy = managerId;
            attendance.ApprovedAt = DateTime.UtcNow;
            attendance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Attendance {AttendanceId} approved by manager {ManagerId}",
                attendanceId, managerId
            );

            var approver = await _context.Users.FindAsync(managerId);
            return MapToDto(attendance, attendance.User, approver);
        }

        public async Task<AttendanceDto> RejectAttendanceAsync(Guid managerId, Guid attendanceId)
        {
            var attendance = await _context.Attendance
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == attendanceId);

            if (attendance == null)
            {
                throw new InvalidOperationException("Attendance record not found.");
            }

            // Verify that the manager is the employee's manager
            if (attendance.User.ManagerId != managerId)
            {
                throw new InvalidOperationException("You are not authorized to reject this attendance.");
            }

            if (attendance.Status != ApprovalStatus.Pending)
            {
                throw new InvalidOperationException($"Attendance is already {attendance.Status}.");
            }

            attendance.Status = ApprovalStatus.Rejected;
            attendance.ApprovedBy = managerId;
            attendance.ApprovedAt = DateTime.UtcNow;
            attendance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Attendance {AttendanceId} rejected by manager {ManagerId}",
                attendanceId, managerId
            );

            var approver = await _context.Users.FindAsync(managerId);
            return MapToDto(attendance, attendance.User, approver);
        }

        public async Task<List<AttendanceDto>> GetTeamAttendanceHistoryAsync(Guid managerId, DateTime startDate, DateTime endDate)
        {
            // Get all subordinates
            var subordinateIds = await _context.Users
                .Where(u => u.ManagerId == managerId && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            var attendanceRecords = await _context.Attendance
                .Include(a => a.User)
                .Include(a => a.Approver)
                .Where(a => subordinateIds.Contains(a.UserId) && 
                           a.Date >= DateOnly.FromDateTime(startDate) && 
                           a.Date <= DateOnly.FromDateTime(endDate))
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.User.FirstName)
                .ToListAsync();

            return attendanceRecords.Select(a => MapToDto(a, a.User, a.Approver)).ToList();
        }

        public async Task<List<LeaveRequestResponse>> GetTeamLeaveHistoryAsync(Guid managerId, DateTime startDate, DateTime endDate)
        {
            // Get all subordinates
            var subordinateIds = await _context.Users
                .Where(u => u.ManagerId == managerId && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            // Ensure dates are in UTC
            var startDateUtc = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
            var endDateUtc = DateTime.SpecifyKind(endDate.Date.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);
            var startDateOnly = DateOnly.FromDateTime(startDate);
            var endDateOnly = DateOnly.FromDateTime(endDate);

            var leaveRequests = await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.Approver)
                .Where(lr => subordinateIds.Contains(lr.UserId) &&
                            (
                                // Include leaves that start or end within the date range
                                (lr.StartDate >= startDateOnly && lr.StartDate <= endDateOnly) ||
                                (lr.EndDate >= startDateOnly && lr.EndDate <= endDateOnly) ||
                                // Include leaves that span across the date range
                                (lr.StartDate <= startDateOnly && lr.EndDate >= endDateOnly) ||
                                // Include leaves approved/rejected in this date range
                                (lr.ApprovedAt.HasValue && 
                                 lr.ApprovedAt.Value >= startDateUtc && 
                                 lr.ApprovedAt.Value <= endDateUtc)
                            ))
                .OrderByDescending(lr => lr.CreatedAt)
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
                ApproverName = lr.Approver != null ? $"{lr.Approver.FirstName} {lr.Approver.LastName}" : null,
                ApprovedAt = lr.ApprovedAt,
                RejectionReason = lr.RejectionReason,
                CreatedAt = lr.CreatedAt
            }).ToList();
        }

        private AttendanceDto MapToDto(Attendance attendance, User user, User? approver)
        {
            TimeSpan? workDuration = null;
            if (attendance.LogoutTime.HasValue)
            {
                workDuration = attendance.LogoutTime.Value - attendance.LoginTime;
            }

            return new AttendanceDto
            {
                Id = attendance.Id,
                UserId = attendance.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                EmployeeId = user.EmployeeId,
                LoginTime = attendance.LoginTime,
                LogoutTime = attendance.LogoutTime,
                Date = attendance.Date,
                IsWeekend = attendance.IsWeekend,
                IsPublicHoliday = attendance.IsPublicHoliday,
                Status = attendance.Status.ToString(),
                ApproverName = approver != null ? $"{approver.FirstName} {approver.LastName}" : null,
                ApprovedAt = attendance.ApprovedAt,
                WorkDuration = workDuration
            };
        }

        public async Task<List<TeamMemberDto>> GetTeamMembersAsync(Guid managerId)
        {
            var currentYear = DateTime.Now.Year;
            var today = DateOnly.FromDateTime(DateTime.Now);

            var teamMembers = await _context.Users
                .Where(u => u.ManagerId == managerId && u.IsActive)
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

        public async Task AssignCompensatoryOffAsync(Guid managerId, Guid employeeId, decimal days, string reason)
        {
            // Verify the employee is a subordinate
            var employee = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == employeeId && u.ManagerId == managerId && u.IsActive);

            if (employee == null)
            {
                throw new UnauthorizedAccessException("Employee not found or not a subordinate");
            }

            if (days <= 0)
            {
                throw new InvalidOperationException("Days must be greater than zero");
            }

            // Get or create leave entitlement for current year
            var currentYear = DateTime.Now.Year;
            var entitlement = await _context.LeaveEntitlements
                .FirstOrDefaultAsync(le => le.UserId == employeeId && le.Year == currentYear);

            if (entitlement == null)
            {
                entitlement = new LeaveEntitlement
                {
                    UserId = employeeId,
                    Year = currentYear,
                    CasualLeaveBalance = 12,
                    EarnedLeaveBalance = 12,
                    CompensatoryOffBalance = days
                };
                _context.LeaveEntitlements.Add(entitlement);
            }
            else
            {
                entitlement.CompensatoryOffBalance += days;
            }

            // Add audit log
            var auditLog = new AuditLog
            {
                UserId = employeeId,
                Action = "Compensatory Off Assigned",
                EntityType = "LeaveEntitlement",
                EntityId = entitlement.Id.ToString(),
                NewValue = $"Manager {managerId} assigned {days} compensatory off day(s). Reason: {reason}",
                Timestamp = DateTime.UtcNow
            };
            _context.AuditLogs.Add(auditLog);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Assigned {days} compensatory off days to employee {employeeId} by manager {managerId}");
        }

        public async Task LogPastAttendanceAsync(Guid managerId, Guid employeeId, DateOnly date, TimeOnly loginTime, TimeOnly? logoutTime)
        {
            // Verify the employee is a subordinate
            var employee = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == employeeId && u.ManagerId == managerId && u.IsActive);

            if (employee == null)
            {
                throw new UnauthorizedAccessException("Employee not found or not a subordinate");
            }

            // Validate date is in the past
            if (date >= DateOnly.FromDateTime(DateTime.Now))
            {
                throw new InvalidOperationException("Cannot log attendance for today or future dates");
            }

            // Validate logout time is after login time
            if (logoutTime.HasValue && logoutTime.Value <= loginTime)
            {
                throw new InvalidOperationException("Logout time must be after login time");
            }

            // Check if attendance already exists for this date
            var existingAttendance = await _context.Attendance
                .FirstOrDefaultAsync(a => a.UserId == employeeId && a.Date == date);

            if (existingAttendance != null)
            {
                throw new InvalidOperationException($"Attendance already exists for {date:yyyy-MM-dd}");
            }

            // Check if it's a weekend or public holiday
            var dayOfWeek = date.DayOfWeek;
            var isWeekend = dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;

            var isPublicHoliday = await _context.PublicHolidays
                .AnyAsync(ph => ph.Date == date);

            // Create attendance record with Approved status
            // Convert to DateTime and specify UTC Kind for PostgreSQL
            var loginDateTime = DateTime.SpecifyKind(date.ToDateTime(loginTime), DateTimeKind.Utc);
            var logoutDateTime = logoutTime.HasValue 
                ? DateTime.SpecifyKind(date.ToDateTime(logoutTime.Value), DateTimeKind.Utc) 
                : (DateTime?)null;

            var attendance = new Attendance
            {
                UserId = employeeId,
                Date = date,
                LoginTime = loginDateTime,
                LogoutTime = logoutDateTime,
                IsWeekend = isWeekend,
                IsPublicHoliday = isPublicHoliday,
                Status = ApprovalStatus.Approved,
                ApprovedBy = managerId,
                ApprovedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Attendance.Add(attendance);
            await _context.SaveChangesAsync();

            // Add audit log after saving attendance to get the Id
            var auditLog = new AuditLog
            {
                UserId = employeeId,
                Action = "Past Attendance Logged",
                EntityType = "Attendance",
                EntityId = attendance.Id.ToString(),
                NewValue = $"Manager {managerId} logged past attendance for {date:yyyy-MM-dd} - Login: {loginTime}, Logout: {logoutTime?.ToString() ?? "Not specified"}",
                Timestamp = DateTime.UtcNow
            };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Logged past attendance for employee {employeeId} on {date} by manager {managerId}");
        }
    }
}
