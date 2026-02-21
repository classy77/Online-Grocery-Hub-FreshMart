using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(30)]
        public string? Type { get; set; } // Email, SMS, InApp

        [StringLength(100)]
        public string? Title { get; set; }

        [Required]
        [StringLength(255)]
        public string Message { get; set; } = string.Empty;

        [StringLength(255)]
        public string? TargetUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
