using AttendanceAPI.Models.DTOs;
using AttendanceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LeaveController : ControllerBase
    {
        private readonly ILeaveService _leaveService;
        private readonly ILogger<LeaveController> _logger;

        public LeaveController(ILeaveService leaveService, ILogger<LeaveController> logger)
        {
            _leaveService = leaveService;
            _logger = logger;
        }

        [HttpPost("request")]
        public async Task<IActionResult> CreateLeaveRequest([FromBody] LeaveRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var leaveRequest = await _leaveService.CreateLeaveRequestAsync(userId, request);
                
                return Ok(ApiResponse<LeaveRequestResponse>.SuccessResponse(leaveRequest, "Leave request created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating leave request");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyLeaveRequests()
        {
            try
            {
                var userId = GetCurrentUserId();
                var requests = await _leaveService.GetMyLeaveRequestsAsync(userId);
                
                return Ok(ApiResponse<List<LeaveRequestResponse>>.SuccessResponse(requests));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting leave requests");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetLeaveBalance()
        {
            try
            {
                var userId = GetCurrentUserId();
                var balance = await _leaveService.GetLeaveBalanceAsync(userId);
                
                if (balance == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("No leave entitlement found for current year"));
                }
                
                return Ok(ApiResponse<LeaveBalanceDto>.SuccessResponse(balance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting leave balance");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpGet("pending-approvals")]
        [Authorize(Roles = "Manager,Administrator")]
        public async Task<IActionResult> GetPendingApprovals()
        {
            try
            {
                var managerId = GetCurrentUserId();
                var pendingRequests = await _leaveService.GetPendingLeaveRequestsForApprovalAsync(managerId);
                
                return Ok(ApiResponse<List<LeaveRequestResponse>>.SuccessResponse(pendingRequests));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending approvals");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpPost("approve")]
        [Authorize(Roles = "Manager,Administrator")]
        public async Task<IActionResult> ApproveOrRejectLeave([FromBody] ApproveLeaveRequest request)
        {
            try
            {
                var managerId = GetCurrentUserId();
                var result = await _leaveService.ApproveOrRejectLeaveAsync(managerId, request);
                
                var message = request.Approved ? "Leave request approved successfully" : "Leave request rejected";
                return Ok(ApiResponse<LeaveRequestResponse>.SuccessResponse(result, message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving/rejecting leave");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpPost("cancel/{leaveRequestId}")]
        public async Task<IActionResult> CancelLeaveRequest(Guid leaveRequestId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _leaveService.CancelLeaveRequestAsync(userId, leaveRequestId);
                
                return Ok(ApiResponse<LeaveRequestResponse>.SuccessResponse(result, "Leave request cancelled successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling leave request");
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
