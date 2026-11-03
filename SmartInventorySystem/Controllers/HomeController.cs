
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventorySystem.Data;

namespace SmartInventorySystem.Controllers
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
            var products = await _context.Products
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            ViewBag.TotalProducts = products.Count;
            ViewBag.TotalQuantity = products.Sum(p => p.Quantity);
            ViewBag.LowStockCount = products.Count(p => p.Quantity <= p.MinimumStock);
            ViewBag.TotalStockValue = products.Sum(p => p.Price * p.Quantity);

            ViewBag.RecentProducts = products.Take(5).ToList();
            ViewBag.LowStockProducts = products
                .Where(p => p.Quantity <= p.MinimumStock)
                .OrderBy(p => p.Quantity)
                .Take(5)
                .ToList();

            ViewBag.ChartProducts = products
                .OrderByDescending(p => p.Price * p.Quantity)
                .Take(6)
                .OrderBy(p => p.Price * p.Quantity)
                .ToList();

            ViewBag.InStockCount = products.Count(p => p.Quantity > p.MinimumStock);

            return View();
        }
    }
}