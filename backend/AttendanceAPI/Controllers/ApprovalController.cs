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

        [HttpGet("attendance/history")]
        public async Task<IActionResult> GetTeamAttendanceHistory([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var managerId = GetCurrentUserId();
                var history = await _approvalService.GetTeamAttendanceHistoryAsync(managerId, startDate, endDate);
                
                return Ok(ApiResponse<List<AttendanceDto>>.SuccessResponse(history));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team attendance history");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpGet("leave/history")]
        public async Task<IActionResult> GetTeamLeaveHistory([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var managerId = GetCurrentUserId();
                var history = await _approvalService.GetTeamLeaveHistoryAsync(managerId, startDate, endDate);
                
                return Ok(ApiResponse<List<LeaveRequestResponse>>.SuccessResponse(history));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team leave history");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpGet("team-members")]
        public async Task<IActionResult> GetTeamMembers()
        {
            try
            {
                var managerId = GetCurrentUserId();
                var teamMembers = await _approvalService.GetTeamMembersAsync(managerId);
                
                return Ok(new ApiResponse<IEnumerable<TeamMemberDto>>
                {
                    Success = true,
                    Data = teamMembers,
                    Message = "Team members retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team members");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("assign-comp-off")]
        public async Task<IActionResult> AssignCompensatoryOff([FromBody] AssignCompOffRequest request)
        {
            try
            {
                var managerId = GetCurrentUserId();
                await _approvalService.AssignCompensatoryOffAsync(managerId, request.EmployeeId, request.Days, request.Reason);
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Successfully assigned {request.Days} compensatory off day(s)"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning compensatory off");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("log-past-attendance")]
        public async Task<IActionResult> LogPastAttendance([FromBody] LogPastAttendanceRequest request)
        {
            try
            {
                var managerId = GetCurrentUserId();
                
                // Parse date and time strings
                if (!DateOnly.TryParse(request.Date, out var date))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid date format"
                    });
                }

                if (!TimeOnly.TryParse(request.LoginTime, out var loginTime))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Invalid login time format"
                    });
                }

                TimeOnly? logoutTime = null;
                if (!string.IsNullOrEmpty(request.LogoutTime))
                {
                    if (!TimeOnly.TryParse(request.LogoutTime, out var parsedLogoutTime))
                    {
                        return BadRequest(new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Invalid logout time format"
                        });
                    }
                    logoutTime = parsedLogoutTime;
                }

                await _approvalService.LogPastAttendanceAsync(managerId, request.EmployeeId, date, loginTime, logoutTime);
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Past attendance logged successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging past attendance");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
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
