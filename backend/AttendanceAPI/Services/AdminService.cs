using AttendanceAPI.Data;
using AttendanceAPI.Models.DTOs;
using AttendanceAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceAPI.Services
{
    public interface IAdminService
    {
        Task<UserDto> CreateUserAsync(CreateUserRequest request);
        Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<bool> AllocateLeaveEntitlementAsync(AllocateLeaveRequest request);
        Task<LeaveBalanceDto?> GetUserLeaveBalanceAsync(Guid userId, int year);
        Task<PublicHolidayDto> CreatePublicHolidayAsync(CreatePublicHolidayRequest request);
        Task<List<PublicHolidayDto>> GetPublicHolidaysAsync(int year);
        Task<bool> DeletePublicHolidayAsync(Guid holidayId);
    }

    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminService> _logger;

        public AdminService(ApplicationDbContext context, ILogger<AdminService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            // Check if employee ID already exists
            if (await _context.Users.AnyAsync(u => u.EmployeeId == request.EmployeeId))
            {
                throw new InvalidOperationException("A user with this employee ID already exists.");
            }

            // Validate role
            if (!Enum.IsDefined(typeof(UserRole), request.Role))
            {
                throw new InvalidOperationException("Invalid role.");
            }

            // Validate manager if specified
            if (request.ManagerId.HasValue)
            {
                var manager = await _context.Users.FindAsync(request.ManagerId.Value);
                if (manager == null)
                {
                    throw new InvalidOperationException("Manager not found.");
                }
                if (manager.Role != UserRole.Manager && manager.Role != UserRole.Administrator)
                {
                    throw new InvalidOperationException("The specified manager must have Manager or Administrator role.");
                }
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmployeeId = request.EmployeeId,
                Role = (UserRole)request.Role,
                ManagerId = request.ManagerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new user {Email} with role {Role}", user.Email, user.Role);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmployeeId = user.EmployeeId,
                Role = user.Role.ToString(),
                ManagerId = user.ManagerId
            };
        }

        public async Task<UserDto> UpdateUserAsync(Guid userId, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            if (request.Email != null)
            {
                if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId))
                {
                    throw new InvalidOperationException("A user with this email already exists.");
                }
                user.Email = request.Email;
            }

            if (request.FirstName != null) user.FirstName = request.FirstName;
            if (request.LastName != null) user.LastName = request.LastName;

            if (request.Role.HasValue)
            {
                if (!Enum.IsDefined(typeof(UserRole), request.Role.Value))
                {
                    throw new InvalidOperationException("Invalid role.");
                }
                user.Role = (UserRole)request.Role.Value;
            }

            if (request.ManagerId.HasValue)
            {
                var manager = await _context.Users.FindAsync(request.ManagerId.Value);
                if (manager == null)
                {
                    throw new InvalidOperationException("Manager not found.");
                }
                user.ManagerId = request.ManagerId;
            }

            if (request.IsActive.HasValue)
            {
                user.IsActive = request.IsActive.Value;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated user {UserId}", userId);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmployeeId = user.EmployeeId,
                Role = user.Role.ToString(),
                ManagerId = user.ManagerId
            };
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            return users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                EmployeeId = u.EmployeeId,
                Role = u.Role.ToString(),
                ManagerId = u.ManagerId
            }).ToList();
        }

        public async Task<bool> AllocateLeaveEntitlementAsync(AllocateLeaveRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var existingEntitlement = await _context.LeaveEntitlements
                .FirstOrDefaultAsync(e => e.UserId == request.UserId && e.Year == request.Year);

            if (existingEntitlement != null)
            {
                // Update existing entitlement
                existingEntitlement.CasualLeaveBalance = request.CasualLeaveBalance;
                existingEntitlement.EarnedLeaveBalance = request.EarnedLeaveBalance;
                existingEntitlement.CompensatoryOffBalance = request.CompensatoryOffBalance;
                existingEntitlement.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Updated leave entitlement for user {UserId} for year {Year}", request.UserId, request.Year);
            }
            else
            {
                // Create new entitlement
                var entitlement = new LeaveEntitlement
                {
                    UserId = request.UserId,
                    Year = request.Year,
                    CasualLeaveBalance = request.CasualLeaveBalance,
                    EarnedLeaveBalance = request.EarnedLeaveBalance,
                    CompensatoryOffBalance = request.CompensatoryOffBalance,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.LeaveEntitlements.Add(entitlement);
                _logger.LogInformation("Created leave entitlement for user {UserId} for year {Year}", request.UserId, request.Year);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<LeaveBalanceDto?> GetUserLeaveBalanceAsync(Guid userId, int year)
        {
            var entitlement = await _context.LeaveEntitlements
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Year == year);

            if (entitlement == null)
            {
                return null;
            }

            return new LeaveBalanceDto
            {
                Year = entitlement.Year,
                CasualLeaveBalance = entitlement.CasualLeaveBalance,
                EarnedLeaveBalance = entitlement.EarnedLeaveBalance,
                CompensatoryOffBalance = entitlement.CompensatoryOffBalance
            };
        }

        public async Task<PublicHolidayDto> CreatePublicHolidayAsync(CreatePublicHolidayRequest request)
        {
            // Check if holiday already exists for this date
            if (await _context.PublicHolidays.AnyAsync(h => h.Date == request.Date))
            {
                throw new InvalidOperationException("A public holiday already exists for this date.");
            }

            var holiday = new PublicHoliday
            {
                Date = request.Date,
                Name = request.Name,
                Description = request.Description,
                Year = request.Date.Year,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.PublicHolidays.Add(holiday);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created public holiday {Name} on {Date}", holiday.Name, holiday.Date);

            return new PublicHolidayDto
            {
                Id = holiday.Id,
                Date = holiday.Date,
                Name = holiday.Name,
                Description = holiday.Description,
                Year = holiday.Year,
                IsActive = holiday.IsActive
            };
        }

        public async Task<List<PublicHolidayDto>> GetPublicHolidaysAsync(int year)
        {
            var holidays = await _context.PublicHolidays
                .Where(h => h.Year == year && h.IsActive)
                .OrderBy(h => h.Date)
                .ToListAsync();

            return holidays.Select(h => new PublicHolidayDto
            {
                Id = h.Id,
                Date = h.Date,
                Name = h.Name,
                Description = h.Description,
                Year = h.Year,
                IsActive = h.IsActive
            }).ToList();
        }

        public async Task<bool> DeletePublicHolidayAsync(Guid holidayId)
        {
            var holiday = await _context.PublicHolidays.FindAsync(holidayId);
            if (holiday == null)
            {
                throw new InvalidOperationException("Public holiday not found.");
            }

            holiday.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted public holiday {HolidayId}", holidayId);
            return true;
        }
    }
}
