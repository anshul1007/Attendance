using AttendanceAPI.Models.DTOs;
using AttendanceAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                
                if (response == null)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse(
                        "Invalid email or password",
                        "The email or password you entered is incorrect. Please try again."
                    ));
                }

                return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred during login",
                    ex.Message
                ));
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            try
            {
                // For JWT-based auth, logout is handled client-side by removing the token
                // This endpoint can be used for logging or additional cleanup if needed
                _logger.LogInformation("User logged out");
                return Ok(ApiResponse<object>.SuccessResponse(new { }, "Logout successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "An error occurred during logout",
                    ex.Message
                ));
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                // Extract user ID from JWT token claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?? User.FindFirst("sub");
                
                if (userIdClaim == null)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));
                }

                if (!Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid user ID"));
                }

                var user = await _authService.GetUserByIdAsync(userId);
                
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
                }

                return Ok(ApiResponse<UserDto>.SuccessResponse(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred", ex.Message));
            }
        }
    }
}
