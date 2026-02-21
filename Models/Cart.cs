using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Helper property to calculate total
        [NotMapped]
        public decimal TotalAmount => CartItems.Sum(item => item.Quantity * item.UnitPriceCents / 100m);

        [NotMapped]
        public int TotalItems => CartItems.Sum(item => item.Quantity);
    }
}
