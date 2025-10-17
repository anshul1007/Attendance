using AttendanceAPI.Data;
using AttendanceAPI.Models.DTOs;
using AttendanceAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceAPI.Services
{
    public interface ILeaveService
    {
        Task<LeaveRequestResponse> CreateLeaveRequestAsync(Guid userId, LeaveRequestDto request);
        Task<List<LeaveRequestResponse>> GetMyLeaveRequestsAsync(Guid userId);
        Task<List<LeaveRequestResponse>> GetPendingLeaveRequestsForApprovalAsync(Guid managerId);
        Task<LeaveRequestResponse> ApproveOrRejectLeaveAsync(Guid approverId, ApproveLeaveRequest request);
        Task<LeaveBalanceDto?> GetLeaveBalanceAsync(Guid userId);
        Task<LeaveRequestResponse> CancelLeaveRequestAsync(Guid userId, Guid leaveRequestId);
    }

    public class LeaveService : ILeaveService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeaveService> _logger;

        public LeaveService(ApplicationDbContext context, ILogger<LeaveService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<LeaveRequestResponse> CreateLeaveRequestAsync(Guid userId, LeaveRequestDto request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Validate leave type
            if (!Enum.IsDefined(typeof(LeaveType), request.LeaveType))
            {
                throw new InvalidOperationException("Invalid leave type.");
            }

            // Calculate total days
            var totalDays = CalculateLeaveDays(request.StartDate, request.EndDate);

            // Check if user has sufficient balance
            var year = DateTime.UtcNow.Year;
            var entitlement = await _context.LeaveEntitlements
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Year == year);

            if (entitlement == null)
            {
                throw new InvalidOperationException("No leave entitlement found for the current year. Please contact admin.");
            }

            var leaveType = (LeaveType)request.LeaveType;
            var hasBalance = leaveType switch
            {
                LeaveType.CasualLeave => entitlement.CasualLeaveBalance >= totalDays,
                LeaveType.EarnedLeave => entitlement.EarnedLeaveBalance >= totalDays,
                LeaveType.CompensatoryOff => entitlement.CompensatoryOffBalance >= totalDays,
                _ => false
            };

            if (!hasBalance)
            {
                throw new InvalidOperationException($"Insufficient {leaveType} balance. Available: {GetBalance(entitlement, leaveType)}, Required: {totalDays}");
            }

            // Check for overlapping leave requests (exclude rejected and cancelled requests)
            var hasOverlap = await _context.LeaveRequests
                .AnyAsync(lr => lr.UserId == userId &&
                               lr.Status != ApprovalStatus.Rejected &&
                               lr.Status != ApprovalStatus.Cancelled &&
                               ((lr.StartDate <= request.EndDate && lr.EndDate >= request.StartDate)));

            if (hasOverlap)
            {
                throw new InvalidOperationException("You already have a leave request for overlapping dates.");
            }

            var leaveRequest = new LeaveRequest
            {
                UserId = userId,
                LeaveType = leaveType,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalDays = totalDays,
                Reason = request.Reason,
                Status = ApprovalStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "User {UserId} created leave request for {LeaveType} from {StartDate} to {EndDate}",
                userId, leaveType, request.StartDate, request.EndDate
            );

            return MapToResponse(leaveRequest, user, null);
        }

        public async Task<List<LeaveRequestResponse>> GetMyLeaveRequestsAsync(Guid userId)
        {
            var leaveRequests = await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.Approver)
                .Where(lr => lr.UserId == userId)
                .OrderByDescending(lr => lr.CreatedAt)
                .ToListAsync();

            return leaveRequests.Select(lr => MapToResponse(lr, lr.User, lr.Approver)).ToList();
        }

        public async Task<List<LeaveRequestResponse>> GetPendingLeaveRequestsForApprovalAsync(Guid managerId)
        {
            // Get all subordinates of the manager
            var subordinateIds = await _context.Users
                .Where(u => u.ManagerId == managerId && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            var leaveRequests = await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.Approver)
                .Where(lr => subordinateIds.Contains(lr.UserId) && lr.Status == ApprovalStatus.Pending)
                .OrderBy(lr => lr.CreatedAt)
                .ToListAsync();

            return leaveRequests.Select(lr => MapToResponse(lr, lr.User, lr.Approver)).ToList();
        }

        public async Task<LeaveRequestResponse> ApproveOrRejectLeaveAsync(Guid approverId, ApproveLeaveRequest request)
        {
            var leaveRequest = await _context.LeaveRequests
                .Include(lr => lr.User)
                .FirstOrDefaultAsync(lr => lr.Id == request.LeaveRequestId);

            if (leaveRequest == null)
            {
                throw new InvalidOperationException("Leave request not found.");
            }

            if (leaveRequest.Status != ApprovalStatus.Pending)
            {
                throw new InvalidOperationException($"Leave request is already {leaveRequest.Status}.");
            }

            // Verify that the approver is the manager of the employee
            var employee = leaveRequest.User;
            if (employee.ManagerId != approverId)
            {
                throw new InvalidOperationException("You are not authorized to approve this leave request.");
            }

            var year = leaveRequest.StartDate.Year;
            var entitlement = await _context.LeaveEntitlements
                .FirstOrDefaultAsync(e => e.UserId == leaveRequest.UserId && e.Year == year);

            if (entitlement == null)
            {
                throw new InvalidOperationException("Leave entitlement not found.");
            }

            if (request.Approved)
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
                entitlement.UpdatedAt = DateTime.UtcNow;

                leaveRequest.Status = ApprovalStatus.Approved;
                _logger.LogInformation(
                    "Leave request {LeaveRequestId} approved by {ApproverId}",
                    request.LeaveRequestId, approverId
                );
            }
            else
            {
                leaveRequest.Status = ApprovalStatus.Rejected;
                leaveRequest.RejectionReason = request.RejectionReason;
                _logger.LogInformation(
                    "Leave request {LeaveRequestId} rejected by {ApproverId}. Reason: {Reason}",
                    request.LeaveRequestId, approverId, request.RejectionReason
                );
            }

            leaveRequest.ApprovedBy = approverId;
            leaveRequest.ApprovedAt = DateTime.UtcNow;
            leaveRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var approver = await _context.Users.FindAsync(approverId);
            return MapToResponse(leaveRequest, employee, approver);
        }

        public async Task<LeaveRequestResponse> CancelLeaveRequestAsync(Guid userId, Guid leaveRequestId)
        {
            var leaveRequest = await _context.LeaveRequests
                .Include(lr => lr.User)
                .FirstOrDefaultAsync(lr => lr.Id == leaveRequestId);

            if (leaveRequest == null)
            {
                throw new InvalidOperationException("Leave request not found.");
            }

            // Verify that the leave request belongs to the user
            if (leaveRequest.UserId != userId)
            {
                throw new InvalidOperationException("You are not authorized to cancel this leave request.");
            }

            // Only pending requests can be cancelled
            if (leaveRequest.Status != ApprovalStatus.Pending)
            {
                throw new InvalidOperationException($"Cannot cancel leave request. Current status: {leaveRequest.Status}");
            }

            leaveRequest.Status = ApprovalStatus.Cancelled;
            leaveRequest.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Leave request {LeaveRequestId} cancelled by user {UserId}",
                leaveRequestId, userId
            );

            return MapToResponse(leaveRequest, leaveRequest.User, null);
        }

        public async Task<LeaveBalanceDto?> GetLeaveBalanceAsync(Guid userId)
        {
            var year = DateTime.UtcNow.Year;
            var entitlement = await _context.LeaveEntitlements
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Year == year);

            if (entitlement == null)
            {
                return null;
            }

            // Get pending leave requests for the current year
            var pendingLeaves = await _context.LeaveRequests
                .Where(lr => lr.UserId == userId && 
                            lr.Status == ApprovalStatus.Pending &&
                            lr.StartDate.Year == year)
                .ToListAsync();

            // Calculate pending leave days by type
            var pendingCasualLeave = pendingLeaves
                .Where(lr => lr.LeaveType == LeaveType.CasualLeave)
                .Sum(lr => lr.TotalDays);

            var pendingEarnedLeave = pendingLeaves
                .Where(lr => lr.LeaveType == LeaveType.EarnedLeave)
                .Sum(lr => lr.TotalDays);

            var pendingCompensatoryOff = pendingLeaves
                .Where(lr => lr.LeaveType == LeaveType.CompensatoryOff)
                .Sum(lr => lr.TotalDays);

            return new LeaveBalanceDto
            {
                Year = entitlement.Year,
                CasualLeaveBalance = entitlement.CasualLeaveBalance - pendingCasualLeave,
                EarnedLeaveBalance = entitlement.EarnedLeaveBalance - pendingEarnedLeave,
                CompensatoryOffBalance = entitlement.CompensatoryOffBalance - pendingCompensatoryOff
            };
        }

        private decimal CalculateLeaveDays(DateOnly startDate, DateOnly endDate)
        {
            if (endDate < startDate)
            {
                throw new InvalidOperationException("End date cannot be before start date.");
            }

            // Simple calculation: count all days including weekends
            // You can make this more sophisticated by excluding weekends
            var days = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
            return days;
        }

        private decimal GetBalance(LeaveEntitlement entitlement, LeaveType leaveType)
        {
            return leaveType switch
            {
                LeaveType.CasualLeave => entitlement.CasualLeaveBalance,
                LeaveType.EarnedLeave => entitlement.EarnedLeaveBalance,
                LeaveType.CompensatoryOff => entitlement.CompensatoryOffBalance,
                _ => 0
            };
        }

        private LeaveRequestResponse MapToResponse(LeaveRequest leaveRequest, User user, User? approver)
        {
            return new LeaveRequestResponse
            {
                Id = leaveRequest.Id,
                UserId = leaveRequest.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                EmployeeId = user.EmployeeId,
                LeaveType = leaveRequest.LeaveType.ToString(),
                StartDate = leaveRequest.StartDate,
                EndDate = leaveRequest.EndDate,
                TotalDays = leaveRequest.TotalDays,
                Reason = leaveRequest.Reason,
                Status = leaveRequest.Status.ToString(),
                ApproverName = approver != null ? $"{approver.FirstName} {approver.LastName}" : null,
                ApprovedAt = leaveRequest.ApprovedAt,
                RejectionReason = leaveRequest.RejectionReason,
                CreatedAt = leaveRequest.CreatedAt
            };
        }
    }
}
