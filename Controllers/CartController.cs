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
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            var cart = await GetOrCreateCart(userId);

            var cartViewModel = await BuildCartViewModel(cart);
            return View(cartViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = GetCurrentUserId();
            var cart = await GetOrCreateCart(userId);

            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive)
                return Json(new { success = false, message = "Product not found" });

            if (product.Stock < quantity)
                return Json(new { success = false, message = "Insufficient stock" });

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPriceCents = product.PriceCents
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();

            var cartCount = await _context.CartItems
                .Where(ci => ci.CartId == cart.Id)
                .SumAsync(ci => ci.Quantity);

            return Json(new { success = true, cartCount, message = "Added to cart" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = GetCurrentUserId();
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

            if (cartItem == null)
                return Json(new { success = false, message = "Item not found" });

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                if (cartItem.Product.Stock < quantity)
                    return Json(new { success = false, message = "Insufficient stock" });

                cartItem.Quantity = quantity;
            }

            await _context.SaveChangesAsync();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstAsync(c => c.Id == cartItem.CartId);

            var cartViewModel = await BuildCartViewModel(cart);

            return Json(new
            {
                success = true,
                cartTotal = cartViewModel.TotalAmount,
                itemCount = cartViewModel.TotalItems,
                itemSubtotal = cartItem.Quantity * cartItem.UnitPriceCents / 100m
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var userId = GetCurrentUserId();
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

            if (cartItem == null)
                return Json(new { success = false, message = "Item not found" });

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstAsync(c => c.Id == cartItem.CartId);

            var cartViewModel = await BuildCartViewModel(cart);

            return Json(new
            {
                success = true,
                cartTotal = cartViewModel.TotalAmount,
                itemCount = cartViewModel.TotalItems,
                message = "Item removed"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Cart cleared" });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = GetCurrentUserId();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var count = cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;
            return Json(new { count });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim ?? "0");
        }

        private async Task<Cart> GetOrCreateCart(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        private async Task<CartViewModel> BuildCartViewModel(Cart cart)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.CartId == cart.Id)
                .Select(ci => new CartItemViewModel
                {
                    CartItemId = ci.Id,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ProductImage = ci.Product.ImageUrl,
                    UnitPrice = ci.UnitPriceCents / 100m,
                    Quantity = ci.Quantity,
                    Subtotal = ci.Quantity * ci.UnitPriceCents / 100m,
                    Stock = ci.Product.Stock
                })
                .ToListAsync();

            return new CartViewModel
            {
                CartId = cart.Id,
                Items = cartItems,
                TotalAmount = cartItems.Sum(i => i.Subtotal),
                TotalItems = cartItems.Sum(i => i.Quantity)
            };
        }
    }
}
