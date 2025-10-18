namespace AttendanceAPI.Models.DTOs
{
    public class DepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string WeeklyOffDays { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateDepartmentRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string WeeklyOffDays { get; set; } = "Saturday,Sunday";
    }

    public class UpdateDepartmentRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string WeeklyOffDays { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
