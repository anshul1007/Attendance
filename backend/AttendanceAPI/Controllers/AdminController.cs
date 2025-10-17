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
    }
}
