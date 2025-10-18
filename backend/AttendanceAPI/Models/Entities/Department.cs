using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceAPI.Models.Entities
{
    [Table("Departments")]
    public class Department
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Weekly off days stored as comma-separated values (e.g., "Saturday,Sunday" or "Tuesday")
        [Required]
        public string WeeklyOffDays { get; set; } = "Saturday,Sunday";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<User> Employees { get; set; } = new List<User>();
    }
}
