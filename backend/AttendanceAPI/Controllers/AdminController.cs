using AttendanceAPI.Models.DTOs;
using AttendanceAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = await _adminService.CreateUserAsync(request);
                return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpPut("users/{userId}")]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _adminService.UpdateUserAsync(userId, request);
                return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _adminService.GetAllUsersAsync();
                return Ok(ApiResponse<List<UserDto>>.SuccessResponse(users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpPost("leave-entitlement")]
        public async Task<IActionResult> AllocateLeaveEntitlement([FromBody] AllocateLeaveRequest request)
        {
            try
            {
                var result = await _adminService.AllocateLeaveEntitlementAsync(request);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Leave entitlement allocated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error allocating leave entitlement");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpGet("leave-entitlement/{userId}")]
        public async Task<IActionResult> GetUserLeaveBalance(Guid userId, [FromQuery] int year)
        {
            try
            {
                if (year == 0) year = DateTime.UtcNow.Year;

                var balance = await _adminService.GetUserLeaveBalanceAsync(userId, year);
                if (balance == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("No leave entitlement found for this user and year"));
                }

                return Ok(ApiResponse<LeaveBalanceDto>.SuccessResponse(balance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting leave balance");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpPost("holidays")]
        public async Task<IActionResult> CreatePublicHoliday([FromBody] CreatePublicHolidayRequest request)
        {
            try
            {
                var holiday = await _adminService.CreatePublicHolidayAsync(request);
                return Ok(ApiResponse<PublicHolidayDto>.SuccessResponse(holiday, "Public holiday created successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating public holiday");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpGet("holidays")]
        public async Task<IActionResult> GetPublicHolidays([FromQuery] int year)
        {
            try
            {
                if (year == 0) year = DateTime.UtcNow.Year;

                var holidays = await _adminService.GetPublicHolidaysAsync(year);
                return Ok(ApiResponse<List<PublicHolidayDto>>.SuccessResponse(holidays));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting public holidays");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpDelete("holidays/{holidayId}")]
        public async Task<IActionResult> DeletePublicHoliday(Guid holidayId)
        {
            try
            {
                var result = await _adminService.DeletePublicHolidayAsync(holidayId);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Public holiday deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting public holiday");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        // Department Management
        [HttpPost("departments")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequest request)
        {
            try
            {
                var department = await _adminService.CreateDepartmentAsync(request);
                return Ok(ApiResponse<DepartmentDto>.SuccessResponse(department, "Department created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating department");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetAllDepartments()
        {
            try
            {
                var departments = await _adminService.GetAllDepartmentsAsync();
                return Ok(ApiResponse<List<DepartmentDto>>.SuccessResponse(departments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting departments");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpPut("departments/{departmentId}")]
        public async Task<IActionResult> UpdateDepartment(Guid departmentId, [FromBody] UpdateDepartmentRequest request)
        {
            try
            {
                var department = await _adminService.UpdateDepartmentAsync(departmentId, request);
                return Ok(ApiResponse<DepartmentDto>.SuccessResponse(department, "Department updated successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating department");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpDelete("departments/{departmentId}")]
        public async Task<IActionResult> DeleteDepartment(Guid departmentId)
        {
            try
            {
                var result = await _adminService.DeleteDepartmentAsync(departmentId);
                return Ok(ApiResponse<bool>.SuccessResponse(result, "Department deleted successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting department");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpGet("team-members")]
        public async Task<IActionResult> GetAllTeamMembers()
        {
            try
            {
                var teamMembers = await _adminService.GetAllTeamMembersAsync();
                return Ok(ApiResponse<List<TeamMemberDto>>.SuccessResponse(teamMembers));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team members");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpPost("assign-comp-off")]
        public async Task<IActionResult> AssignCompensatoryOff([FromBody] AssignCompOffRequest request)
        {
            try
            {
                await _adminService.AssignCompensatoryOffAsync(request.EmployeeId, request.Days, request.Reason);
                return Ok(ApiResponse<string>.SuccessResponse("Success", "Compensatory off assigned successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning compensatory off");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpPost("log-past-attendance")]
        public async Task<IActionResult> LogPastAttendance([FromBody] LogPastAttendanceRequest request)
        {
            try
            {
                await _adminService.LogPastAttendanceAsync(request.EmployeeId, request.Date, request.LoginTime, request.LogoutTime);
                return Ok(ApiResponse<string>.SuccessResponse("Success", "Past attendance logged successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging past attendance");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpGet("attendance/history")]
        public async Task<IActionResult> GetTeamAttendanceHistory([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddMonths(-1);
                var end = endDate ?? DateTime.Today;
                var records = await _adminService.GetTeamAttendanceHistoryAsync(start, end);
                return Ok(ApiResponse<List<AttendanceDto>>.SuccessResponse(records, "Team attendance history retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team attendance history");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpGet("leave/history")]
        public async Task<IActionResult> GetTeamLeaveHistory([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.Today.AddMonths(-1);
                var end = endDate ?? DateTime.Today;
                var records = await _adminService.GetTeamLeaveHistoryAsync(start, end);
                return Ok(ApiResponse<List<LeaveRequestResponse>>.SuccessResponse(records, "Team leave history retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team leave history");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpGet("attendance/pending")]
        public async Task<IActionResult> GetPendingAttendanceApprovals([FromQuery] string? date)
        {
            try
            {
                var pendingAttendances = await _adminService.GetPendingAttendanceApprovalsAsync();
                
                // Filter by date if provided
                if (!string.IsNullOrEmpty(date) && DateOnly.TryParse(date, out var filterDate))
                {
                    pendingAttendances = pendingAttendances.Where(a => a.Date == filterDate).ToList();
                }
                
                return Ok(ApiResponse<List<AttendanceDto>>.SuccessResponse(pendingAttendances, "Pending attendance approvals retrieved successfully"));
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
                var attendance = await _adminService.ApproveAttendanceAsync(attendanceId);
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
                var attendance = await _adminService.RejectAttendanceAsync(attendanceId, null);
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

        [HttpGet("leave/pending")]
        public async Task<IActionResult> GetPendingLeaveRequests()
        {
            try
            {
                var pendingRequests = await _adminService.GetPendingLeaveRequestsAsync();
                return Ok(ApiResponse<List<LeaveRequestResponse>>.SuccessResponse(pendingRequests, "Pending leave requests retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending leave requests");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }

        [HttpPost("leave/approve")]
        public async Task<IActionResult> ApproveOrRejectLeave([FromBody] ApproveLeaveRequest request)
        {
            try
            {
                var result = await _adminService.ApproveOrRejectLeaveAsync(request.LeaveRequestId, request.Approved, request.RejectionReason);
                var message = request.Approved ? "Leave request approved successfully" : "Leave request rejected";
                return Ok(ApiResponse<LeaveRequestResponse>.SuccessResponse(result, message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing leave request");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }
    }
}
