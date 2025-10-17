using System.ComponentModel.DataAnnotations;

namespace AttendanceAPI.Models.DTOs
{
    public class CreateUserRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        public int Role { get; set; } // 1=Employee, 2=Manager, 3=Administrator

        public Guid? ManagerId { get; set; }
    }

    public class UpdateUserRequest
    {
        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        public int? Role { get; set; }

        public Guid? ManagerId { get; set; }

        public bool? IsActive { get; set; }
    }

    public class AllocateLeaveRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        [Range(0, 365)]
        public decimal CasualLeaveBalance { get; set; }

        [Required]
        [Range(0, 365)]
        public decimal EarnedLeaveBalance { get; set; }

        [Range(0, 365)]
        public decimal CompensatoryOffBalance { get; set; } = 0;
    }

    public class CreatePublicHolidayRequest
    {
        [Required]
        public DateOnly Date { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class PublicHolidayDto
    {
        public Guid Id { get; set; }
        public DateOnly Date { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; }
    }
}
