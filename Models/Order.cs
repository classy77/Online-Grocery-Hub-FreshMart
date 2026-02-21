using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Models
{
    public enum OrderStatus
    {
        Pending,
        Packed,
        OutForDelivery,
        Delivered,
        Cancelled
    }

    public class Order
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(50)]
        public string? OrderNumber { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = OrderStatus.Pending.ToString();

        [Required]
        [Range(0, int.MaxValue)]
        public int TotalAmountCents { get; set; }

        [Range(0, int.MaxValue)]
        public int DiscountCents { get; set; } = 0;

        [ForeignKey("Address")]
        public int? ShippingAddressId { get; set; }

        [Required]
        public string AddressSnapshot { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Address? ShippingAddress { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual Payment? Payment { get; set; }
        public virtual Delivery? Delivery { get; set; }

        // Helper properties
        [NotMapped]
        public decimal TotalAmount => TotalAmountCents / 100m;

        [NotMapped]
        public decimal Discount => DiscountCents / 100m;

        [NotMapped]
        public decimal FinalAmount => (TotalAmountCents - DiscountCents) / 100m;
    }
}
