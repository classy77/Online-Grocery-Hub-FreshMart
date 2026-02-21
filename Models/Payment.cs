using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Models
{
    public enum PaymentMethod
    {
        CashOnDelivery,
        UPI,
        Card,
        NetBanking
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }

    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int AmountCents { get; set; }

        [StringLength(20)]
        public string? Method { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = PaymentStatus.Pending.ToString();

        [StringLength(100)]
        public string? TransactionId { get; set; }

        public DateTime? PaidAt { get; set; }

        // Navigation property
        public virtual Order Order { get; set; } = null!;

        // Helper property
        [NotMapped]
        public decimal Amount => AmountCents / 100m;
    }
}
