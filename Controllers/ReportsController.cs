using GroceryStore.Data;
using GroceryStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Where(o => o.CreatedAt.Date >= start.Date && o.CreatedAt.Date <= end.Date)
                .ToListAsync();

            var report = new ReportViewModel
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = orders.Sum(o => o.TotalAmountCents - o.DiscountCents) / 100m,
                TotalOrders = orders.Count,
                AverageOrderValue = orders.Any() ? orders.Average(o => (o.TotalAmountCents - o.DiscountCents) / 100m) : 0
            };

            // Daily sales
            report.DailySales = orders
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new DailySalesViewModel
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => (o.TotalAmountCents - o.DiscountCents) / 100m)
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Category sales
            var categorySales = await _context.OrderItems
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Where(oi => oi.Order.CreatedAt.Date >= start.Date && oi.Order.CreatedAt.Date <= end.Date)
                .GroupBy(oi => new { oi.Product.CategoryId, CategoryName = oi.Product.Category != null ? oi.Product.Category.Name : "Uncategorized" })
                .Select(g => new CategorySalesViewModel
                {
                    CategoryName = g.Key.CategoryName,
                    ProductCount = g.Select(oi => oi.ProductId).Distinct().Count(),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPriceCents) / 100m
                })
                .OrderByDescending(c => c.Revenue)
                .ToListAsync();

            report.CategorySales = categorySales;

            return View(report);
        }

        public async Task<IActionResult> DailyOrders(DateTime? date)
        {
            var targetDate = date ?? DateTime.Today;

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.CreatedAt.Date == targetDate.Date)
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

            ViewBag.Date = targetDate;
            ViewBag.TotalRevenue = orders.Sum(o => o.FinalAmount);
            ViewBag.TotalOrders = orders.Count;

            return View(orders);
        }

        public async Task<IActionResult> TopProducts(DateTime? startDate, DateTime? endDate, int topCount = 10)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            var topProducts = await _context.OrderItems
                .Include(oi => oi.Product)
                .ThenInclude(p => p.Category)
                .Where(oi => oi.Order.CreatedAt.Date >= start.Date && oi.Order.CreatedAt.Date <= end.Date)
                .GroupBy(oi => new { oi.ProductId, ProductName = oi.Product.Name })
                .Select(g => new TopProductViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPriceCents) / 100m
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(topCount)
                .ToListAsync();

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;
            ViewBag.TopCount = topCount;

            return View(topProducts);
        }

        public async Task<IActionResult> InventoryReport()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Stock)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                    p.Stock,
                    p.PriceCents,
                    StockValue = p.Stock * p.PriceCents / 100m
                })
                .ToListAsync();

            var lowStockProducts = products.Where(p => p.Stock < 10).ToList();
            var totalStockValue = products.Sum(p => p.StockValue);

            ViewBag.LowStockCount = lowStockProducts.Count;
            ViewBag.TotalStockValue = totalStockValue;

            return View(products);
        }

        public async Task<IActionResult> ExportSales(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today;

            var orders = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.CreatedAt.Date >= start.Date && o.CreatedAt.Date <= end.Date)
                .OrderBy(o => o.CreatedAt)
                .Select(o => new
                {
                    OrderNumber = o.OrderNumber,
                    Customer = o.User != null ? (o.User.FullName ?? o.User.Email) : "Unknown",
                    Date = o.CreatedAt.ToString("yyyy-MM-dd"),
                    Status = o.Status,
                    Total = (o.TotalAmountCents - o.DiscountCents) / 100m
                })
                .ToListAsync();

            var csv = "Order Number,Customer,Date,Status,Total\n";
            foreach (var order in orders)
            {
                csv += $"{order.OrderNumber},{order.Customer},{order.Date},{order.Status},{order.Total:F2}\n";
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"sales_report_{start:yyyyMMdd}_{end:yyyyMMdd}.csv");
        }
    }
}