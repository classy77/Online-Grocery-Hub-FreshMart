using System.ComponentModel.DataAnnotations;

namespace GroceryStore.ViewModels
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ShippingAddress { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
        public PaymentViewModel? Payment { get; set; }
    }

    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class PaymentViewModel
    {
        public string? Method { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Please select a delivery address")]
        [Display(Name = "Delivery Address")]
        public int SelectedAddressId { get; set; }

        [Required(ErrorMessage = "Please select a payment method")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "CashOnDelivery";

        public List<AddressViewModel> Addresses { get; set; } = new List<AddressViewModel>();
        public CartViewModel? Cart { get; set; }
    }

    public class AddressViewModel
    {
        public int Id { get; set; }
        public string? Label { get; set; }
        public string FullAddress { get; set; } = string.Empty;
    }

    public class CreateAddressViewModel
    {
        [StringLength(20)]
        [Display(Name = "Label (e.g., Home, Work)")]
        public string? Label { get; set; }

        [Required(ErrorMessage = "Address line 1 is required")]
        [StringLength(200)]
        [Display(Name = "Address Line 1")]
        public string Line1 { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Address Line 2")]
        public string? Line2 { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(50)]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        [StringLength(50)]
        [Display(Name = "State")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(10)]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Country")]
        public string Country { get; set; } = "India";
    }
}
