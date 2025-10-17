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
    }
}
