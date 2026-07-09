using Damanak.Data;
using Damanak.Services;
using Damanak.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Damanak.Controllers
{
    [Authorize]
    public class GuaranteesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IWalletPassApiService _walletPassApiService;
        private readonly IConfiguration _configuration;
        public GuaranteesController(
       ApplicationDbContext context,
       IWebHostEnvironment environment,
       IWalletPassApiService walletPassApiService,
       IConfiguration configuration)
        {
            _context = context;
            _environment = environment;
            _walletPassApiService = walletPassApiService;
            _configuration = configuration;
        }
        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private string GetWarrantyStatus(DateTime warrantyEndDate)
        {
            if (DateTime.Today > warrantyEndDate)
            {
                return "Expired";
            }

            if ((warrantyEndDate - DateTime.Today).TotalDays <= 30)
            {
                return "ExpiringSoon";
            }

            return "Active";
        }

        private async Task<string?> SaveInvoiceFile(IFormFile? invoiceImage)
        {
            if (invoiceImage == null || invoiceImage.Length == 0)
            {
                return null;
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var extension = Path.GetExtension(invoiceImage.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("نوع الملف غير مسموح. ارفع صورة أو PDF فقط.");
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "invoices");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await invoiceImage.CopyToAsync(stream);
            }

            return $"/uploads/invoices/{fileName}";
        }

        public async Task<IActionResult> Index(string? searchTerm, string? status, string? category)
        {
            var userId = GetCurrentUserId();

            var query = _context.Guarantees
                .Where(g => g.AppUserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(g =>
                    g.ProductName.Contains(searchTerm) ||
                    g.StoreName.Contains(searchTerm) ||
                    (g.Brand != null && g.Brand.Contains(searchTerm)) ||
                    (g.SerialNumber != null && g.SerialNumber.Contains(searchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(g => g.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(g => g.Category == category);
            }

            var guarantees = await query
                .OrderBy(g => g.WarrantyEndDate)
                .ToListAsync();

            var allUserGuarantees = await _context.Guarantees
                .Where(g => g.AppUserId == userId)
                .ToListAsync();

            ViewBag.TotalGuarantees = allUserGuarantees.Count;
            ViewBag.ActiveGuarantees = allUserGuarantees.Count(g => g.Status == "Active");
            ViewBag.ExpiringSoonGuarantees = allUserGuarantees.Count(g => g.Status == "ExpiringSoon");
            ViewBag.ExpiredGuarantees = allUserGuarantees.Count(g => g.Status == "Expired");

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;
            ViewBag.Category = category;

            return View(guarantees);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var guarantee = await _context.Guarantees
                .Include(g => g.Activities)
                .FirstOrDefaultAsync(g => g.Id == id && g.AppUserId == userId);

            if (guarantee == null)
            {
                return NotFound();
            }

            return View(guarantee);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guarantee guarantee, IFormFile? invoiceImage)
        {
            if (guarantee.WarrantyEndDate < guarantee.PurchaseDate)
            {
                ModelState.AddModelError("WarrantyEndDate", "تاريخ انتهاء الضمان لا يمكن أن يكون قبل تاريخ الشراء");
            }

            if (ModelState.IsValid)
            {
                var userId = GetCurrentUserId();

                try
                {
                    guarantee.InvoiceImagePath = await SaveInvoiceFile(invoiceImage);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("InvoiceImagePath", ex.Message);
                    return View(guarantee);
                }

                guarantee.AppUserId = userId;
                guarantee.CreatedAt = DateTime.Now;
                guarantee.Status = GetWarrantyStatus(guarantee.WarrantyEndDate);

                _context.Guarantees.Add(guarantee);
                await _context.SaveChangesAsync();

                var activity = new GuaranteeActivity
                {
                    GuaranteeId = guarantee.Id,
                    Action = "تمت إضافة ضمان منتج",
                    Notes = $"تمت إضافة ضمان {guarantee.ProductName} من متجر {guarantee.StoreName}",
                    CreatedAt = DateTime.Now
                };

                _context.GuaranteeActivities.Add(activity);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(guarantee);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var guarantee = await _context.Guarantees
                .FirstOrDefaultAsync(g => g.Id == id && g.AppUserId == userId);

            if (guarantee == null)
            {
                return NotFound();
            }

            return View(guarantee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Guarantee guarantee, IFormFile? invoiceImage)
        {
            if (id != guarantee.Id)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var existingGuarantee = await _context.Guarantees
                .FirstOrDefaultAsync(g => g.Id == id && g.AppUserId == userId);

            if (existingGuarantee == null)
            {
                return NotFound();
            }

            if (guarantee.WarrantyEndDate < guarantee.PurchaseDate)
            {
                ModelState.AddModelError("WarrantyEndDate", "تاريخ انتهاء الضمان لا يمكن أن يكون قبل تاريخ الشراء");
            }

            if (ModelState.IsValid)
            {
                existingGuarantee.ProductName = guarantee.ProductName;
                existingGuarantee.Category = guarantee.Category;
                existingGuarantee.StoreName = guarantee.StoreName;
                existingGuarantee.Brand = guarantee.Brand;
                existingGuarantee.ModelName = guarantee.ModelName;
                existingGuarantee.SerialNumber = guarantee.SerialNumber;
                existingGuarantee.Price = guarantee.Price;
                existingGuarantee.PurchaseDate = guarantee.PurchaseDate;
                existingGuarantee.WarrantyEndDate = guarantee.WarrantyEndDate;
                existingGuarantee.Notes = guarantee.Notes;
                existingGuarantee.Status = GetWarrantyStatus(guarantee.WarrantyEndDate);

                if (invoiceImage != null && invoiceImage.Length > 0)
                {
                    try
                    {
                        existingGuarantee.InvoiceImagePath = await SaveInvoiceFile(invoiceImage);
                    }
                    catch (InvalidOperationException ex)
                    {
                        ModelState.AddModelError("InvoiceImagePath", ex.Message);
                        return View(guarantee);
                    }
                }

                var activity = new GuaranteeActivity
                {
                    GuaranteeId = existingGuarantee.Id,
                    Action = "تم تعديل ضمان المنتج",
                    Notes = $"تم تحديث بيانات ضمان {existingGuarantee.ProductName}",
                    CreatedAt = DateTime.Now
                };

                _context.GuaranteeActivities.Add(activity);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(guarantee);
        }

        public async Task<IActionResult> Wallet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var guarantee = await _context.Guarantees
                .FirstOrDefaultAsync(g => g.Id == id && g.AppUserId == userId);

            if (guarantee == null)
            {
                return NotFound();
            }

            return View(guarantee);
        }
        public async Task<IActionResult> AddToWallet(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var guarantee = await _context.Guarantees
                .FirstOrDefaultAsync(g => g.Id == id && g.AppUserId == userId);

            if (guarantee == null)
            {
                return NotFound();
            }

            var walletLink = _configuration["WalletPass:TemplateLink"];

            if (string.IsNullOrWhiteSpace(walletLink))
            {
                TempData["WalletError"] = "رابط بطاقة Wallet غير مضاف في appsettings.json";
                return RedirectToAction("Wallet", new { id = guarantee.Id });
            }

            return Redirect(walletLink);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetCurrentUserId();

            var guarantee = await _context.Guarantees
                .FirstOrDefaultAsync(g => g.Id == id && g.AppUserId == userId);

            if (guarantee == null)
            {
                return NotFound();
            }

            return View(guarantee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetCurrentUserId();

            var guarantee = await _context.Guarantees
                .Include(g => g.Activities)
                .FirstOrDefaultAsync(g => g.Id == id && g.AppUserId == userId);

            if (guarantee != null)
            {
                _context.Guarantees.Remove(guarantee);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}