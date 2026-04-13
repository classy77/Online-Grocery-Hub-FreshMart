using GroceryStore.Data;
using GroceryStore.Models;
using GroceryStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var todayOrders = await _context.Orders
                .Where(o => o.CreatedAt.Date == today)
                .ToListAsync();

            var dashboard = new AdminDashboardViewModel
            {
                TotalOrders = await _context.Orders.CountAsync(),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending.ToString()),
                TodayOrders = todayOrders.Count,
                TodayRevenue = todayOrders.Sum(o => o.TotalAmountCents) / 100m,
                TotalRevenue = await _context.Orders.SumAsync(o => o.TotalAmountCents) / 100m,
                TotalProducts = await _context.Products.CountAsync(),
                LowStockProducts = await _context.Products.CountAsync(p => p.Stock < 10),
                TotalCustomers = await _context.Users.CountAsync(u => !u.IsAdmin && u.IsActive)
            };

            // Recent orders
            dashboard.RecentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new RecentOrderViewModel
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    CustomerName = o.User.FullName ?? o.User.Email,
                    Amount = o.TotalAmountCents / 100m,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            // Top products
            var topProducts = await _context.OrderItems
                .Include(oi => oi.Product)
                .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
                .Select(g => new TopProductViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPriceCents) / 100m
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(5)
                .ToListAsync();

            dashboard.TopProducts = topProducts;

            return View(dashboard);
        }

        // Products Management
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Unit = p.Unit,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(new ProductViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    CategoryId = model.CategoryId,
                    Name = model.Name,
                    Unit = model.Unit,
                    Description = model.Description,
                    PriceCents = (int)(model.Price * 100),
                    Stock = model.Stock,
                    ImageUrl = model.ImageUrl,
                    IsActive = model.IsActive
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Product created successfully";
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            var model = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                CategoryId = product.CategoryId,
                Unit = product.Unit,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive
            };

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = await _context.Products.FindAsync(model.Id);
                if (product == null)
                    return NotFound();

                product.CategoryId = model.CategoryId;
                product.Name = model.Name;
                product.Unit = model.Unit;
                product.Description = model.Description;
                product.PriceCents = (int)(model.Price * 100);
                product.Stock = model.Stock;
                product.ImageUrl = model.ImageUrl;
                product.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                TempData["Success"] = "Product updated successfully";
                return RedirectToAction(nameof(Products));
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            // Soft delete - mark as inactive
            product.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product deleted successfully";
            return RedirectToAction(nameof(Products));
        }

        // Orders Management
        public async Task<IActionResult> Orders(string? status)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.Status == status);

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderViewModel
                {
                    OrderId = o.Id,
                    OrderNumber = o.OrderNumber,
                    Status = o.Status,
                    TotalAmount = o.TotalAmountCents / 100m,
                    FinalAmount = (o.TotalAmountCents - o.DiscountCents) / 100m,
                    CreatedAt = o.CreatedAt,
                    Items = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        ProductName = oi.Product.Name,
                        Quantity = oi.Quantity
                    }).ToList()
                })
                .ToListAsync();

            ViewBag.Statuses = new[] { "Pending", "Packed", "OutForDelivery", "Delivered", "Cancelled" };
            ViewBag.SelectedStatus = status;

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Payment)
                .Include(o => o.Delivery)
                .ThenInclude(d => d.DeliveryStaff)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var model = new OrderViewModel
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                TotalAmount = order.TotalAmountCents / 100m,
                Discount = order.DiscountCents / 100m,
                FinalAmount = (order.TotalAmountCents - order.DiscountCents) / 100m,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                ShippingAddress = order.AddressSnapshot,
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    ProductImage = oi.Product.ImageUrl,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPriceCents / 100m,
                    Subtotal = oi.Quantity * oi.UnitPriceCents / 100m
                }).ToList(),
                Payment = order.Payment == null ? null : new PaymentViewModel
                {
                    Method = order.Payment.Method,
                    Status = order.Payment.Status,
                    Amount = order.Payment.AmountCents / 100m,
                    PaidAt = order.Payment.PaidAt
                }
            };

            ViewBag.Delivery = order.Delivery;
            ViewBag.DeliveryStaff = await _context.Users
                .Where(u => u.IsDeliveryStaff && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            ViewBag.AvailableStatuses = Enum.GetNames(typeof(OrderStatus));

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            order.Status = status;
            order.UpdatedAt = DateTime.Now;

            // Update delivery status if needed
            if (status == OrderStatus.OutForDelivery.ToString())
            {
                var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.OrderId == orderId);
                if (delivery != null)
                {
                    delivery.Status = DeliveryStatus.OutForDelivery.ToString();
                }
            }
            else if (status == OrderStatus.Delivered.ToString())
            {
                var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.OrderId == orderId);
                if (delivery != null)
                {
                    delivery.Status = DeliveryStatus.Delivered.ToString();
                    delivery.DeliveredAt = DateTime.Now;
                }

                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
                if (payment != null && payment.Method == "CashOnDelivery")
                {
                    payment.Status = PaymentStatus.Completed.ToString();
                    payment.PaidAt = DateTime.Now;
                }
            }

            // Create notification for customer
            var notification = new Notification
            {
                UserId = order.UserId,
                Type = "InApp",
                Title = "Order Status Updated",
                Message = $"Your order #{order.OrderNumber} status has been updated to {status}.",
                TargetUrl = $"/Order/Details/{order.Id}"
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Order status updated successfully";
            return RedirectToAction(nameof(OrderDetails), new { id = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeliveryStaff(int orderId, int deliveryStaffId)
        {
            try
            {
                if (orderId <= 0)
                {
                    TempData["Error"] = "Invalid order ID";
                    return RedirectToAction(nameof(Orders));
                }

                if (deliveryStaffId <= 0)
                {
                    TempData["Error"] = "Please select a delivery staff member";
                    return RedirectToAction(nameof(OrderDetails), new { id = orderId });
                }

                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    TempData["Error"] = "Order not found";
                    return RedirectToAction(nameof(Orders));
                }

                var delivery = await _context.Deliveries.FirstOrDefaultAsync(d => d.OrderId == orderId);
                if (delivery == null)
                {
                    TempData["Error"] = "Delivery record not found. Please try again.";
                    return RedirectToAction(nameof(OrderDetails), new { id = orderId });
                }

                var staff = await _context.Users.FindAsync(deliveryStaffId);
                if (staff == null || staff.IsAdmin)
                {
                    TempData["Error"] = "Invalid delivery staff member";
                    return RedirectToAction(nameof(OrderDetails), new { id = orderId });
                }

                delivery.DeliveryStaffId = deliveryStaffId;
                delivery.Status = DeliveryStatus.Assigned.ToString();
                delivery.ScheduledAt = DateTime.Now;

                await _context.SaveChangesAsync();

                // Create notification for customer
                var notification = new Notification
                {
                    UserId = order.UserId,
                    Type = "InApp",
                    Title = "Delivery Assigned",
                    Message = $"Your order #{order.OrderNumber} has been assigned to {staff.FullName} for delivery.",
                    TargetUrl = $"/Order/Details/{order.Id}"
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Delivery assigned to {staff.FullName} successfully";
                return RedirectToAction(nameof(OrderDetails), new { id = orderId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while assigning delivery staff. Please try again.";
                return RedirectToAction(nameof(OrderDetails), new { id = orderId });
            }
        }

        // Categories Management
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string name, string? slug)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Category name is required";
                return RedirectToAction(nameof(Categories));
            }

            var category = new Category
            {
                Name = name,
                Slug = slug ?? name.ToLower().Replace(" ", "-")
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category created successfully";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            // Check if category has products
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["Error"] = "Cannot delete category with existing products";
                return RedirectToAction(nameof(Categories));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully";
            return RedirectToAction(nameof(Categories));
        }

        // Delivery Staff Management
        public async Task<IActionResult> DeliveryStaff()
        {
            var staff = await _context.Users
                .Where(u => u.IsDeliveryStaff)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(staff);
        }

        public IActionResult CreateDeliveryStaff()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDeliveryStaff(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(model);
            }

            var user = new User
            {
                Email = model.Email,
                FullName = model.FullName,
                Phone = model.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                IsAdmin = false,
                IsDeliveryStaff = true,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create empty cart for user (after user is saved)
            var cart = new Cart { UserId = user.Id };
            _context.Carts.Add(cart);

            // Create notification
            var notification = new Notification
            {
                UserId = user.Id,
                Type = "InApp",
                Title = "Account Created",
                Message = "Your delivery staff account has been created successfully."
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Delivery staff member {model.FullName} has been added successfully";
            return RedirectToAction(nameof(DeliveryStaff));
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateDeliveryStaff(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Delivery staff member {user.FullName} has been deactivated";
            return RedirectToAction(nameof(DeliveryStaff));
        }

        [HttpPost]
        public async Task<IActionResult> ActivateDeliveryStaff(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = true;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Delivery staff member {user.FullName} has been activated";
            return RedirectToAction(nameof(DeliveryStaff));
        }
    }
}
