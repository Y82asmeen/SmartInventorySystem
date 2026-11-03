
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventorySystem.Data;
using SmartInventorySystem.Model;

namespace SmartInventorySystem.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SuppliersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all suppliers and search by name
        public async Task<IActionResult> Index(string? searchString)
        {
            var suppliers = _context.Suppliers.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                suppliers = suppliers.Where(s =>
                    s.Name.Contains(searchString));
            }

            ViewBag.SearchString = searchString;

            var result = await suppliers
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(result);
        }

        // Display the create form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Save a new supplier
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (!ModelState.IsValid)
            {
                return View(supplier);
            }

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Supplier added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Display the edit form
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }

        // Save supplier changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(supplier);
            }

            _context.Suppliers.Update(supplier);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Supplier updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Display the delete confirmation
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var supplier = await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
            {
                return NotFound();
            }

            return View(supplier);
        }

        // Delete supplier after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);

            if (supplier == null)
            {
                return NotFound();
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Supplier deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}