using GroceryStore.Data;
using GroceryStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string? search, decimal? minPrice, decimal? maxPrice, string? sort)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive);

            // Apply filters
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
                ViewBag.SelectedCategory = await _context.Categories.FindAsync(categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchLower) ||
                                        (p.Description != null && p.Description.ToLower().Contains(searchLower)));
                ViewBag.SearchTerm = search;
            }

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // Apply sorting
            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.PriceCents),
                "price_desc" => query.OrderByDescending(p => p.PriceCents),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            ViewBag.SortOrder = sort;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            var products = await query.Select(p => new ProductViewModel
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
            }).ToListAsync();

            ViewBag.Categories = await _context.Categories
                .Select(c => new { c.Id, c.Name, ProductCount = c.Products.Count(p => p.IsActive) })
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == id && p.IsActive)
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
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            // Get related products
            var relatedProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
                .Take(4)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Unit = p.Unit,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            var searchLower = query.ToLower();
            var products = await _context.Products
                .Where(p => p.IsActive && (p.Name.ToLower().Contains(searchLower) ||
                         (p.Description != null && p.Description.ToLower().Contains(searchLower))))
                .Take(10)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.ImageUrl,
                    Price = p.PriceCents / 100m
                })
                .ToListAsync();

            return Json(products);
        }
    }
}
