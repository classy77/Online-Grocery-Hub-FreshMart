using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(20)]
        public string? Label { get; set; } // Home, Work, etc.

        [Required]
        [StringLength(200)]
        public string Line1 { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Line2 { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? PostalCode { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
}
