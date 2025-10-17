using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceAPI.Models.Entities
{
    [Table("LeaveEntitlements")]
    public class LeaveEntitlement
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        public int Year { get; set; }

        [Required, Column(TypeName = "decimal(5,2)")]
        public decimal CasualLeaveBalance { get; set; } = 0;

        [Required, Column(TypeName = "decimal(5,2)")]
        public decimal EarnedLeaveBalance { get; set; } = 0;

        [Required, Column(TypeName = "decimal(5,2)")]
        public decimal CompensatoryOffBalance { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
