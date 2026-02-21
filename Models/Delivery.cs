using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryStore.Models
{
    public enum DeliveryStatus
    {
        Scheduled,
        Assigned,
        PickedUp,
        OutForDelivery,
        Delivered,
        Failed
    }

    public class Delivery
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [ForeignKey("Address")]
        public int? AddressId { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = DeliveryStatus.Scheduled.ToString();

        public DateTime? ScheduledAt { get; set; }

        public DateTime? DeliveredAt { get; set; }

        [ForeignKey("DeliveryStaff")]
        public int? DeliveryStaffId { get; set; }

        // Navigation properties
        public virtual Order Order { get; set; } = null!;
        public virtual Address? DeliveryAddress { get; set; }
        public virtual User? DeliveryStaff { get; set; }
    }
}
