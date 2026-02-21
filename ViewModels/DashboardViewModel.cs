namespace GroceryStore.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TodayOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int TotalCustomers { get; set; }
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new List<RecentOrderViewModel>();
        public List<TopProductViewModel> TopProducts { get; set; } = new List<TopProductViewModel>();
    }

    public class RecentOrderViewModel
    {
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TopProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DeliveryDashboardViewModel
    {
        public int AssignedOrders { get; set; }
        public int PendingDeliveries { get; set; }
        public int CompletedToday { get; set; }
        public List<DeliveryOrderViewModel> Orders { get; set; } = new List<DeliveryOrderViewModel>();
    }

    public class DeliveryOrderViewModel
    {
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ScheduledAt { get; set; }
        public List<string> Items { get; set; } = new List<string>();
    }

    public class ReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<DailySalesViewModel> DailySales { get; set; } = new List<DailySalesViewModel>();
        public List<CategorySalesViewModel> CategorySales { get; set; } = new List<CategorySalesViewModel>();
    }

    public class DailySalesViewModel
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class CategorySalesViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
