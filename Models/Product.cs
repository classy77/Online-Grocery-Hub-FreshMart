using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Unit { get; set; } // kg, pcs, liter, etc.

        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int PriceCents { get; set; } = 0;

        [Range(0, int.MaxValue)]
        public int Stock { get; set; } = 0;

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Helper property to display price in rupees
        [NotMapped]
        public decimal Price => PriceCents / 100m;
    }
}
