using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TownTrek.Data;
using TownTrek.Models;

namespace TownTrek.Controllers.Admin
{
    [Authorize]
    public class AdminCategoriesController(ApplicationDbContext context, ILogger<AdminCategoriesController> logger) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<AdminCategoriesController> _logger = logger;

        public async Task<IActionResult> Index()
        {
            var categories = await _context.BusinessCategories
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
            return View("~/Views/Admin/Categories/Index.cshtml", categories);
        }

        public IActionResult Create()
        {
            return View("~/Views/Admin/Categories/Create.cshtml", new BusinessCategory());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BusinessCategory model)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Categories/Create.cshtml", model);
            }

            // Ensure unique key
            var exists = await _context.BusinessCategories.AnyAsync(c => c.Key == model.Key);
            if (exists)
            {
                ModelState.AddModelError("Key", "A category with this key already exists.");
                return View("~/Views/Admin/Categories/Create.cshtml", model);
            }

            model.IsActive = true;
            _context.BusinessCategories.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _context.BusinessCategories.FindAsync(id);
            if (category == null) return RedirectToAction(nameof(Index));
            return View("~/Views/Admin/Categories/Edit.cshtml", category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BusinessCategory input)
        {
            var category = await _context.BusinessCategories.FindAsync(id);
            if (category == null) return RedirectToAction(nameof(Index));

            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/Categories/Edit.cshtml", input);
            }

            // Key is immutable by design
            category.Name = input.Name;
            category.Description = input.Description;
            category.IconClass = input.IconClass;
            category.FormType = input.FormType;
            category.IsActive = input.IsActive;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Category updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var category = await _context.BusinessCategories.FindAsync(id);
            if (category != null)
            {
                category.IsActive = !category.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Category {(category.IsActive ? "activated" : "deactivated")}.";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Subcategories(int id)
        {
            var category = await _context.BusinessCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return RedirectToAction(nameof(Index));

            var subs = await _context.BusinessSubCategories
                .Where(sc => sc.CategoryId == id)
                .OrderBy(sc => sc.Name)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Category = category;
            return View("~/Views/Admin/Categories/Subcategories.cshtml", subs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSubcategory(int categoryId, string key, string name)
        {
            var category = await _context.BusinessCategories.FindAsync(categoryId);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Category not found.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(name))
            {
                TempData["ErrorMessage"] = "Key and Name are required.";
                return RedirectToAction(nameof(Subcategories), new { id = categoryId });
            }

            var exists = await _context.BusinessSubCategories.AnyAsync(sc => sc.CategoryId == categoryId && sc.Key == key);
            if (exists)
            {
                TempData["ErrorMessage"] = "A subcategory with this key already exists for this category.";
                return RedirectToAction(nameof(Subcategories), new { id = categoryId });
            }

            _context.BusinessSubCategories.Add(new BusinessSubCategory
            {
                CategoryId = categoryId,
                Key = key.Trim(),
                Name = name.Trim(),
                IsActive = true
            });
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Subcategory added.";
            return RedirectToAction(nameof(Subcategories), new { id = categoryId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSubcategory(int id)
        {
            var sub = await _context.BusinessSubCategories.FindAsync(id);
            if (sub != null)
            {
                sub.IsActive = !sub.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Subcategory {(sub.IsActive ? "activated" : "deactivated")}.";
                return RedirectToAction(nameof(Subcategories), new { id = sub.CategoryId });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}


