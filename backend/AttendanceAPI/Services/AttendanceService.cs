using AttendanceAPI.Data;
using AttendanceAPI.Models.DTOs;
using AttendanceAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceAPI.Services
{
    public interface IAttendanceService
    {
        Task<AttendanceDto> LoginAsync(Guid userId);
        Task<AttendanceDto> LogoutAsync(Guid userId);
        Task<List<AttendanceDto>> GetAttendanceHistoryAsync(Guid userId, DateTime? startDate, DateTime? endDate);
        Task<List<AttendanceDto>> GetTeamAttendanceAsync(Guid managerId, DateTime? startDate, DateTime? endDate);
        Task<AttendanceDto?> GetTodayAttendanceAsync(Guid userId);
    }

    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AttendanceService> _logger;

        public AttendanceService(ApplicationDbContext context, ILogger<AttendanceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AttendanceDto> LoginAsync(Guid userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Check if user already logged in today
            var existingAttendance = await _context.Attendance
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == today);

            if (existingAttendance != null && existingAttendance.LogoutTime == null)
            {
                throw new InvalidOperationException("You are already logged in today. Please logout first.");
            }

            if (existingAttendance != null && existingAttendance.LogoutTime != null)
            {
                throw new InvalidOperationException("You have already completed your attendance for today.");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Check if today is a weekend or public holiday
            var isWeekend = DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday || 
                           DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday;
            
            var isPublicHoliday = await _context.PublicHolidays
                .AnyAsync(h => h.Date == today && h.IsActive);

            var attendance = new Attendance
            {
                UserId = userId,
                LoginTime = DateTime.UtcNow,
                Date = today,
                IsWeekend = isWeekend,
                IsPublicHoliday = isPublicHoliday,
                Status = ApprovalStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Attendance.Add(attendance);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} logged in at {LoginTime}. Weekend: {IsWeekend}, Holiday: {IsPublicHoliday}",
                userId, attendance.LoginTime, isWeekend, isPublicHoliday
            );

            return MapToDto(attendance, user);
        }

        public async Task<AttendanceDto> LogoutAsync(Guid userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var attendance = await _context.Attendance
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == today && a.LogoutTime == null);

            if (attendance == null)
            {
                throw new InvalidOperationException("No active login found for today. Please login first.");
            }

            attendance.LogoutTime = DateTime.UtcNow;
            attendance.UpdatedAt = DateTime.UtcNow;

            // If it's a weekend or holiday, automatically create compensatory off balance
            if (attendance.IsWeekend || attendance.IsPublicHoliday)
            {
                var year = DateTime.UtcNow.Year;
                var entitlement = await _context.LeaveEntitlements
                    .FirstOrDefaultAsync(e => e.UserId == userId && e.Year == year);

                if (entitlement == null)
                {
                    entitlement = new LeaveEntitlement
                    {
                        UserId = userId,
                        Year = year,
                        CasualLeaveBalance = 0,
                        EarnedLeaveBalance = 0,
                        CompensatoryOffBalance = 0.5m, // Half day for now, can be adjusted
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.LeaveEntitlements.Add(entitlement);
                }
                else
                {
                    entitlement.CompensatoryOffBalance += 0.5m; // Half day
                    entitlement.UpdatedAt = DateTime.UtcNow;
                }

                _logger.LogInformation(
                    "Added 0.5 day compensatory off to user {UserId} for working on {Date}",
                    userId, attendance.Date
                );
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} logged out at {LogoutTime}",
                userId, attendance.LogoutTime
            );

            return MapToDto(attendance, attendance.User);
        }

        public async Task<AttendanceDto?> GetTodayAttendanceAsync(Guid userId)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var attendance = await _context.Attendance
                .Include(a => a.User)
                .Include(a => a.Approver)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == today);

            return attendance != null ? MapToDto(attendance, attendance.User) : null;
        }

        public async Task<List<AttendanceDto>> GetAttendanceHistoryAsync(Guid userId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Attendance
                .Include(a => a.User)
                .Include(a => a.Approver)
                .Where(a => a.UserId == userId);

            if (startDate.HasValue)
            {
                var start = DateOnly.FromDateTime(startDate.Value);
                query = query.Where(a => a.Date >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateOnly.FromDateTime(endDate.Value);
                query = query.Where(a => a.Date <= end);
            }

            var attendances = await query.OrderByDescending(a => a.Date).ToListAsync();

            return attendances.Select(a => MapToDto(a, a.User)).ToList();
        }

        public async Task<List<AttendanceDto>> GetTeamAttendanceAsync(Guid managerId, DateTime? startDate, DateTime? endDate)
        {
            // Get all subordinates of the manager
            var subordinateIds = await _context.Users
                .Where(u => u.ManagerId == managerId && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            var query = _context.Attendance
                .Include(a => a.User)
                .Include(a => a.Approver)
                .Where(a => subordinateIds.Contains(a.UserId));

            if (startDate.HasValue)
            {
                var start = DateOnly.FromDateTime(startDate.Value);
                query = query.Where(a => a.Date >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateOnly.FromDateTime(endDate.Value);
                query = query.Where(a => a.Date <= end);
            }

            var attendances = await query.OrderByDescending(a => a.Date).ToListAsync();

            return attendances.Select(a => MapToDto(a, a.User)).ToList();
        }

        private AttendanceDto MapToDto(Attendance attendance, User user)
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
                LoginTime = attendance.LoginTime,
                LogoutTime = attendance.LogoutTime,
                Date = attendance.Date,
                IsWeekend = attendance.IsWeekend,
                IsPublicHoliday = attendance.IsPublicHoliday,
                Status = attendance.Status.ToString(),
                ApproverName = attendance.Approver != null ? $"{attendance.Approver.FirstName} {attendance.Approver.LastName}" : null,
                ApprovedAt = attendance.ApprovedAt,
                WorkDuration = workDuration
            };
        }
    }
}
