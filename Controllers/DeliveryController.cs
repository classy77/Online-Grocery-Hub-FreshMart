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
    public class DeliveryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeliveryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound();

            // Check if user is delivery staff (not admin but has deliveries assigned)
            var isDeliveryStaff = await _context.Deliveries
                .AnyAsync(d => d.DeliveryStaffId == userId);

            if (!isDeliveryStaff && !user.IsAdmin)
            {
                return RedirectToAction("Index", "Home");
            }

            var deliveries = await _context.Deliveries
                .Include(d => d.Order)
                .ThenInclude(o => o.User)
                .Include(d => d.Order.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(d => d.DeliveryStaffId == userId)
                .OrderByDescending(d => d.ScheduledAt)
                .ToListAsync();

            var viewModel = new DeliveryDashboardViewModel
            {
                AssignedOrders = deliveries.Count,
                PendingDeliveries = deliveries.Count(d => d.Status != DeliveryStatus.Delivered.ToString() &&
                                                         d.Status != DeliveryStatus.Failed.ToString()),
                CompletedToday = deliveries.Count(d => d.Status == DeliveryStatus.Delivered.ToString() &&
                                                       d.DeliveredAt.HasValue &&
                                                       d.DeliveredAt.Value.Date == DateTime.Today),
                Orders = deliveries.Select(d => new DeliveryOrderViewModel
                {
                    OrderId = d.OrderId,
                    OrderNumber = d.Order.OrderNumber,
                    CustomerName = d.Order.User.FullName ?? d.Order.User.Email,
                    CustomerPhone = d.Order.User.Phone ?? "N/A",
                    Address = d.Address ?? "N/A",
                    Status = d.Status,
                    ScheduledAt = d.ScheduledAt,
                    Items = d.Order.OrderItems.Select(oi => $"{oi.Product.Name} x{oi.Quantity}").ToList()
                }).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = GetCurrentUserId();
            var delivery = await _context.Deliveries
                .Include(d => d.Order)
                .ThenInclude(o => o.User)
                .Include(d => d.Order.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(d => d.Order.Payment)
                .FirstOrDefaultAsync(d => d.OrderId == id && d.DeliveryStaffId == userId);

            if (delivery == null)
                return NotFound();

            var viewModel = new DeliveryOrderViewModel
            {
                OrderId = delivery.OrderId,
                OrderNumber = delivery.Order.OrderNumber,
                CustomerName = delivery.Order.User.FullName ?? delivery.Order.User.Email,
                CustomerPhone = delivery.Order.User.Phone ?? "N/A",
                Address = delivery.Address ?? "N/A",
                Status = delivery.Status,
                ScheduledAt = delivery.ScheduledAt,
                Items = delivery.Order.OrderItems.Select(oi => $"{oi.Product.Name} x{oi.Quantity}").ToList()
            };

            ViewBag.AvailableStatuses = new[]
            {
                DeliveryStatus.Assigned.ToString(),
                DeliveryStatus.PickedUp.ToString(),
                DeliveryStatus.OutForDelivery.ToString(),
                DeliveryStatus.Delivered.ToString(),
                DeliveryStatus.Failed.ToString()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string status, string? notes)
        {
            var userId = GetCurrentUserId();
            var delivery = await _context.Deliveries
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.OrderId == orderId && d.DeliveryStaffId == userId);

            if (delivery == null)
                return NotFound();

            delivery.Status = status;

            if (status == DeliveryStatus.Delivered.ToString())
            {
                delivery.DeliveredAt = DateTime.Now;

                // Update order status
                delivery.Order.Status = OrderStatus.Delivered.ToString();
                delivery.Order.UpdatedAt = DateTime.Now;

                // Update payment for COD
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
                if (payment != null && payment.Method == "CashOnDelivery")
                {
                    payment.Status = PaymentStatus.Completed.ToString();
                    payment.PaidAt = DateTime.Now;
                }
            }
            else if (status == DeliveryStatus.OutForDelivery.ToString())
            {
                delivery.Order.Status = OrderStatus.OutForDelivery.ToString();
                delivery.Order.UpdatedAt = DateTime.Now;
            }
            else if (status == DeliveryStatus.PickedUp.ToString())
            {
                delivery.Order.Status = OrderStatus.Packed.ToString();
                delivery.Order.UpdatedAt = DateTime.Now;
            }

            // Create notification for customer
            var notification = new Notification
            {
                UserId = delivery.Order.UserId,
                Type = "InApp",
                Title = "Delivery Update",
                Message = $"Your order #{delivery.Order.OrderNumber} delivery status: {status}",
                TargetUrl = $"/Order/Details/{delivery.OrderId}"
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Delivery status updated successfully";
            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingDeliveries()
        {
            var userId = GetCurrentUserId();
            var pendingCount = await _context.Deliveries
                .CountAsync(d => d.DeliveryStaffId == userId &&
                                d.Status != DeliveryStatus.Delivered.ToString() &&
                                d.Status != DeliveryStatus.Failed.ToString());

            return Json(new { count = pendingCount });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}
