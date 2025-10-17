using AttendanceAPI.Data;
using AttendanceAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceAPI.Services
{
    public static class DatabaseSeeder
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Check if any users exist
                if (await context.Users.AnyAsync())
                {
                    logger.LogInformation("Database already has users. Skipping seeding.");
                    return;
                }

                logger.LogInformation("Seeding database with initial users...");

                // Create Admin user
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "admin@attendance.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    FirstName = "Admin",
                    LastName = "User",
                    EmployeeId = "EMP001",
                    Role = UserRole.Administrator,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Create Manager user
                var manager = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "manager@attendance.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"),
                    FirstName = "Manager",
                    LastName = "User",
                    EmployeeId = "EMP002",
                    Role = UserRole.Manager,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Create Employee user
                var employee = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "employee@attendance.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Employee@123"),
                    FirstName = "Employee",
                    LastName = "User",
                    EmployeeId = "EMP003",
                    Role = UserRole.Employee,
                    ManagerId = manager.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Users.AddRange(admin, manager, employee);
                await context.SaveChangesAsync();

                // Create leave entitlements for current year
                var currentYear = DateTime.UtcNow.Year;
                var entitlements = new List<LeaveEntitlement>
                {
                    new LeaveEntitlement
                    {
                        UserId = admin.Id,
                        Year = currentYear,
                        CasualLeaveBalance = 12,
                        EarnedLeaveBalance = 15,
                        CompensatoryOffBalance = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new LeaveEntitlement
                    {
                        UserId = manager.Id,
                        Year = currentYear,
                        CasualLeaveBalance = 12,
                        EarnedLeaveBalance = 15,
                        CompensatoryOffBalance = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new LeaveEntitlement
                    {
                        UserId = employee.Id,
                        Year = currentYear,
                        CasualLeaveBalance = 12,
                        EarnedLeaveBalance = 15,
                        CompensatoryOffBalance = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                context.LeaveEntitlements.AddRange(entitlements);
                await context.SaveChangesAsync();

                logger.LogInformation("Database seeding completed successfully!");
                logger.LogInformation("Admin credentials: admin@attendance.com / Admin@123");
                logger.LogInformation("Manager credentials: manager@attendance.com / Manager@123");
                logger.LogInformation("Employee credentials: employee@attendance.com / Employee@123");
                logger.LogInformation("Leave entitlements allocated: 12 Casual, 15 Earned for each user");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
            }
        }
    }
}
