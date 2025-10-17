using AttendanceAPI.Models.DTOs;
using AttendanceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manager,Administrator")]
    public class ApprovalController : ControllerBase
    {
        private readonly IApprovalService _approvalService;
        private readonly ILogger<ApprovalController> _logger;

        public ApprovalController(IApprovalService approvalService, ILogger<ApprovalController> logger)
        {
            _approvalService = approvalService;
            _logger = logger;
        }

        [HttpGet("attendance/pending")]
        public async Task<IActionResult> GetPendingAttendanceApprovals()
        {
            try
            {
                var managerId = GetCurrentUserId();
                var pendingAttendances = await _approvalService.GetPendingAttendanceApprovalsAsync(managerId);
                
                return Ok(ApiResponse<List<AttendanceDto>>.SuccessResponse(pendingAttendances));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending attendance approvals");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpPost("attendance/{attendanceId}/approve")]
        public async Task<IActionResult> ApproveAttendance(Guid attendanceId)
        {
            try
            {
                var managerId = GetCurrentUserId();
                var attendance = await _approvalService.ApproveAttendanceAsync(managerId, attendanceId);
                
                return Ok(ApiResponse<AttendanceDto>.SuccessResponse(attendance, "Attendance approved successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving attendance");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpPost("attendance/{attendanceId}/reject")]
        public async Task<IActionResult> RejectAttendance(Guid attendanceId)
        {
            try
            {
                var managerId = GetCurrentUserId();
                var attendance = await _approvalService.RejectAttendanceAsync(managerId, attendanceId);
                
                return Ok(ApiResponse<AttendanceDto>.SuccessResponse(attendance, "Attendance rejected successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting attendance");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }
    }
}
