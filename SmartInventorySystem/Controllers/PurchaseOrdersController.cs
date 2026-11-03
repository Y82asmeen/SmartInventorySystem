
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartInventorySystem.Data;
using SmartInventorySystem.Model;

namespace SmartInventorySystem.Controllers
{
    public class PurchaseOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchaseOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all purchase orders
        public async Task<IActionResult> Index(string? searchString)
        {
            var orders = _context.PurchaseOrders
                .Include(p => p.Supplier)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                orders = orders.Where(p =>
                    p.Supplier != null &&
                    p.Supplier.Name.Contains(searchString));
            }

            ViewBag.SearchString = searchString;

            var result = await orders
                .OrderByDescending(p => p.OrderDate)
                .ThenByDescending(p => p.Id)
                .ToListAsync();

            return View(result);
        }

        // Display the create form
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadSuppliersAsync();

            return View(new PurchaseOrder
            {
                OrderDate = DateTime.Today,
                Status = "Pending"
            });
        }

        // Save a new purchase order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrder purchaseOrder)
        {
            var supplierExists = await _context.Suppliers
                .AnyAsync(s => s.Id == purchaseOrder.SupplierId);

            if (!supplierExists)
            {
                ModelState.AddModelError(
                    nameof(purchaseOrder.SupplierId),
                    "Please select a valid supplier.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSuppliersAsync(purchaseOrder.SupplierId);
                return View(purchaseOrder);
            }

            // TotalAmount will be calculated when order items are implemented.
            purchaseOrder.TotalAmount = 0;
            purchaseOrder.Status = "Pending";

            _context.PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Purchase order created successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadSuppliersAsync(int? selectedSupplierId = null)
        {
            var suppliers = await _context.Suppliers
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.SupplierId = new SelectList(
                suppliers,
                "Id",
                "Name",
                selectedSupplierId);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.PurchaseOrders
                .Include(o => o.Supplier)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpGet]
        public async Task<IActionResult> AddItem(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.PurchaseOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending orders can be modified.";
                return RedirectToAction(nameof(Details), new { id });
            }

            await LoadProductsAsync();

            ViewBag.OrderId = order.Id;

            return View(new PurchaseOrderItem
            {
                PurchaseOrderId = order.Id,
                Quantity = 1
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(PurchaseOrderItem item)
        {
            var order = await _context.PurchaseOrders
                .FirstOrDefaultAsync(o => o.Id == item.PurchaseOrderId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Pending")
            {
                TempData["ErrorMessage"] = "Only pending orders can be modified.";
                return RedirectToAction(nameof(Details),
                    new { id = item.PurchaseOrderId });
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == item.ProductId);

            if (product == null)
            {
                ModelState.AddModelError(nameof(item.ProductId),
                    "Please select a valid product.");
            }
            else
            {
                if (product.Price <= 0)
                {
                    ModelState.AddModelError(nameof(item.ProductId),
                        "The selected product must have a price greater than zero.");
                }

                if (product.Quantity < item.Quantity)
                {
                    ModelState.AddModelError(nameof(item.Quantity),
                        $"Only {product.Quantity} units are currently available.");
                }

                item.UnitPrice = product.Price;
            }
            ModelState.Remove(nameof(item.UnitPrice));

            if (!ModelState.IsValid)
            {
                await LoadProductsAsync(item.ProductId);
                ViewBag.OrderId = item.PurchaseOrderId;
                return View(item);
            }

            // Prevent duplicate rows for the same product in one order.
            var existingItem = await _context.PurchaseOrderItems
                .FirstOrDefaultAsync(i =>
                    i.PurchaseOrderId == item.PurchaseOrderId &&
                    i.ProductId == item.ProductId);

            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + item.Quantity;

                if (product!.Quantity < newQuantity)
                {
                    ModelState.AddModelError(nameof(item.Quantity),
                        $"Only {product.Quantity} units are currently available.");

                    await LoadProductsAsync(item.ProductId);
                    ViewBag.OrderId = item.PurchaseOrderId;
                    return View(item);
                }

                existingItem.Quantity = newQuantity;
            }
            else
            {
                var newItem = new PurchaseOrderItem
                {
                    PurchaseOrderId = item.PurchaseOrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product!.Price
                };

                _context.PurchaseOrderItems.Add(newItem);
            }

            await _context.SaveChangesAsync();

            order.TotalAmount = await _context.PurchaseOrderItems
                .Where(i => i.PurchaseOrderId == order.Id)
                .SumAsync(i => i.Quantity * i.UnitPrice);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product added to purchase order successfully.";

            return RedirectToAction(nameof(Details),
                new { id = item.PurchaseOrderId });
        }

        private async Task LoadProductsAsync(int? selectedProductId = null)
        {
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.Quantity > 0 && p.Price > 0)
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.ProductId = new SelectList(
                products,
                "Id",
                "Name",
                selectedProductId);
        }
public async Task<IActionResult> Receive(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var order = await _context.PurchaseOrders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Pending")
            {
                TempData["ErrorMessage"] =
                    "Only pending orders can be received.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (order.Items == null || !order.Items.Any())
            {
                TempData["ErrorMessage"] =
                    "Cannot receive an order without products.";

                return RedirectToAction(nameof(Details), new { id });
            }

            var productIds = order.Items
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var item in order.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                {
                    TempData["ErrorMessage"] =
                        "A product in this order could not be found.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                product.Quantity += item.Quantity;
            }

            order.Status = "Received";

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["SuccessMessage"] =
                "Order received successfully. Inventory has been updated.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int id)
        {
            var item = await _context.PurchaseOrderItems
                .Include(i => i.PurchaseOrder)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            var order = item.PurchaseOrder;

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Pending")
            {
                TempData["ErrorMessage"] =
                    "Products can only be removed from pending orders.";

                return RedirectToAction(nameof(Details),
                    new { id = order.Id });
            }

            int orderId = order.Id;

            _context.PurchaseOrderItems.Remove(item);

            order.TotalAmount = await _context.PurchaseOrderItems
                .Where(i => i.PurchaseOrderId == orderId && i.Id != id)
                .SumAsync(i => i.Quantity * i.UnitPrice);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Product removed from purchase order successfully.";

            return RedirectToAction(nameof(Details),
                new { id = orderId });
        }
    }
}