using GroceryStore.Data;
using GroceryStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.Stock > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
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

            var categories = await _context.Categories
                .Select(c => new { c.Id, c.Name, c.Slug, ProductCount = c.Products.Count(p => p.IsActive) })
                .ToListAsync();

            ViewBag.Categories = categories;
            return View(featuredProducts);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
