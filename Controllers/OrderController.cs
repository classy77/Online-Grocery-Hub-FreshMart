using GroceryStore.Data;
using GroceryStore.Models;
using GroceryStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GroceryStore.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderViewModel
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    TotalAmount = o.TotalAmountCents / 100m,
                    Discount = o.DiscountCents / 100m,
                    FinalAmount = (o.TotalAmountCents - o.DiscountCents) / 100m,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        ProductImage = oi.Product.ImageUrl,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPriceCents / 100m,
                        Subtotal = oi.Quantity * oi.UnitPriceCents / 100m
                    }).ToList(),
                    Payment = o.Payment == null ? null : new PaymentViewModel
                    {
                        Method = o.Payment.Method,
                        Status = o.Payment.Status,
                        Amount = o.Payment.AmountCents / 100m,
                        PaidAt = o.Payment.PaidAt
                    }
                })
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .Include(o => o.Delivery)
                .Where(o => o.Id == id && o.UserId == userId)
                .Select(o => new OrderViewModel
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    TotalAmount = o.TotalAmountCents / 100m,
                    Discount = o.DiscountCents / 100m,
                    FinalAmount = (o.TotalAmountCents - o.DiscountCents) / 100m,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    ShippingAddress = o.AddressSnapshot,
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product.Name,
                        ProductImage = oi.Product.ImageUrl,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPriceCents / 100m,
                        Subtotal = oi.Quantity * oi.UnitPriceCents / 100m
                    }).ToList(),
                    Payment = o.Payment == null ? null : new PaymentViewModel
                    {
                        Method = o.Payment.Method,
                        Status = o.Payment.Status,
                        Amount = o.Payment.AmountCents / 100m,
                        PaidAt = o.Payment.PaidAt
                    }
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetCurrentUserId();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Cart");
            }

            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .Select(a => new AddressViewModel
                {
                    Id = a.Id,
                    Label = a.Label,
                    FullAddress = $"{a.Line1}, {a.Line2}{(string.IsNullOrEmpty(a.Line2) ? "" : ", ")}{a.City}, {a.State} - {a.PostalCode}"
                })
                .ToListAsync();

            var cartViewModel = new CartViewModel
            {
                CartId = cart.Id,
                Items = cart.CartItems.Select(ci => new CartItemViewModel
                {
                    CartItemId = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ProductImage = ci.Product.ImageUrl,
                    UnitPrice = ci.UnitPriceCents / 100m,
                    Quantity = ci.Quantity,
                    Subtotal = ci.Quantity * ci.UnitPriceCents / 100m
                }).ToList(),
                TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.UnitPriceCents / 100m),
                TotalItems = cart.CartItems.Sum(ci => ci.Quantity)
            };

            var viewModel = new CheckoutViewModel
            {
                Addresses = addresses,
                Cart = cartViewModel
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var userId = GetCurrentUserId();

            if (!ModelState.IsValid)
            {
                model.Addresses = await _context.Addresses
                    .Where(a => a.UserId == userId)
                    .Select(a => new AddressViewModel
                    {
                        Id = a.Id,
                        Label = a.Label,
                        FullAddress = $"{a.Line1}, {a.Line2}{(string.IsNullOrEmpty(a.Line2) ? "" : ", ")}{a.City}, {a.State} - {a.PostalCode}"
                    })
                    .ToListAsync();

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstAsync(c => c.UserId == userId);

                model.Cart = new CartViewModel
                {
                    Items = cart.CartItems.Select(ci => new CartItemViewModel
                    {
                        CartItemId = ci.Id,
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.Name,
                        UnitPrice = ci.UnitPriceCents / 100m,
                        Quantity = ci.Quantity,
                        Subtotal = ci.Quantity * ci.UnitPriceCents / 100m
                    }).ToList(),
                    TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.UnitPriceCents / 100m)
                };

                return View(model);
            }

            var userCart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null || !userCart.CartItems.Any())
            {
                TempData["Error"] = "Your cart is empty";
                return RedirectToAction("Index", "Cart");
            }

            // Validate stock
            foreach (var item in userCart.CartItems)
            {
                if (item.Product.Stock < item.Quantity)
                {
                    TempData["Error"] = $"Insufficient stock for {item.Product.Name}";
                    return RedirectToAction("Index", "Cart");
                }
            }

            var address = await _context.Addresses.FindAsync(model.SelectedAddressId);
            if (address == null || address.UserId != userId)
            {
                ModelState.AddModelError("SelectedAddressId", "Invalid address selected");
                return View(model);
            }

            // Create order
            var orderNumber = $"ORD{DateTime.Now:yyyyMMdd}{new Random().Next(1000, 9999)}";
            var totalAmountCents = userCart.CartItems.Sum(ci => ci.Quantity * ci.UnitPriceCents);

            var order = new Order
            {
                UserId = userId,
                OrderNumber = orderNumber,
                Status = OrderStatus.Pending.ToString(),
                TotalAmountCents = totalAmountCents,
                DiscountCents = 0,
                ShippingAddressId = address.Id,
                AddressSnapshot = $"{address.Line1}, {address.Line2}{(string.IsNullOrEmpty(address.Line2) ? "" : ", ")}{address.City}, {address.State} - {address.PostalCode}, {address.Country}",
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Create order items and update stock
            foreach (var item in userCart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPriceCents = item.UnitPriceCents
                };
                _context.OrderItems.Add(orderItem);

                // Update stock
                item.Product.Stock -= item.Quantity;
            }

            // Create payment record
            var payment = new Payment
            {
                OrderId = order.Id,
                AmountCents = totalAmountCents,
                Method = model.PaymentMethod,
                Status = model.PaymentMethod == "CashOnDelivery" ? PaymentStatus.Pending.ToString() : PaymentStatus.Pending.ToString()
            };
            _context.Payments.Add(payment);

            // Create delivery record
            var delivery = new Delivery
            {
                OrderId = order.Id,
                AddressId = address.Id,
                Address = order.AddressSnapshot,
                Status = DeliveryStatus.Scheduled.ToString(),
                ScheduledAt = DateTime.Now.AddDays(1)
            };
            _context.Deliveries.Add(delivery);

            // Clear cart
            _context.CartItems.RemoveRange(userCart.CartItems);

            // Create notification
            var notification = new Notification
            {
                UserId = userId,
                Type = "InApp",
                Title = "Order Placed!",
                Message = $"Your order #{orderNumber} has been placed successfully.",
                TargetUrl = $"/Order/Details/{order.Id}"
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Order placed successfully! Your order number is {orderNumber}";
            return RedirectToAction("Details", new { id = order.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = GetCurrentUserId();
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
                return NotFound();

            if (order.Status != OrderStatus.Pending.ToString())
            {
                TempData["Error"] = "Only pending orders can be cancelled";
                return RedirectToAction("Details", new { id });
            }

            // Restore stock
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                    product.Stock += item.Quantity;
            }

            order.Status = OrderStatus.Cancelled.ToString();
            order.UpdatedAt = DateTime.Now;

            // Update payment status
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == order.Id);
            if (payment != null)
                payment.Status = PaymentStatus.Refunded.ToString();

            // Create notification
            var notification = new Notification
            {
                UserId = userId,
                Type = "InApp",
                Title = "Order Cancelled",
                Message = $"Your order #{order.OrderNumber} has been cancelled.",
                TargetUrl = $"/Order/Details/{order.Id}"
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Order cancelled successfully";
            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Track(int id)
        {
            var userId = GetCurrentUserId();
            var order = await _context.Orders
                .Include(o => o.Delivery)
                .Where(o => o.Id == id && o.UserId == userId)
                .Select(o => new
                {
                    o.OrderNumber,
                    o.Status,
                    o.CreatedAt,
                    o.UpdatedAt,
                    DeliveryStatus = o.Delivery != null ? o.Delivery.Status : null,
                    ScheduledAt = o.Delivery != null ? o.Delivery.ScheduledAt : null,
                    DeliveredAt = o.Delivery != null ? o.Delivery.DeliveredAt : null
                })
                .FirstOrDefaultAsync();

            if (order == null)
                return NotFound();

            return Json(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(CreateAddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = string.Join(", ", errors) });
            }

            var userId = GetCurrentUserId();
            
            var address = new Address
            {
                UserId = userId,
                Label = model.Label,
                Line1 = model.Line1,
                Line2 = model.Line2,
                City = model.City,
                State = model.State,
                PostalCode = model.PostalCode,
                Country = model.Country,
                CreatedAt = DateTime.Now
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return Json(new 
            { 
                success = true, 
                message = "Address added successfully",
                addressId = address.Id,
                fullAddress = $"{address.Line1}, {address.Line2}{(string.IsNullOrEmpty(address.Line2) ? "" : ", ")}{address.City}, {address.State} - {address.PostalCode}"
            });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}
