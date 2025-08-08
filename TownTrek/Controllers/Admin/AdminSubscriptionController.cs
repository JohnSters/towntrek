using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Subscriptions")]
    public class AdminSubscriptionController : Controller
    {
        private readonly ISubscriptionTierService _tierService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminSubscriptionController> _logger;

        public AdminSubscriptionController(
            ISubscriptionTierService tierService,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminSubscriptionController> logger)
        {
            _tierService = tierService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var viewModel = await _tierService.GetTierListViewModelAsync();
            return View(viewModel);
        }

        [HttpGet("Create")]
        public IActionResult Create()
        {
            var model = new SubscriptionTierViewModel
            {
                IsActive = true,
                SortOrder = 1,
                HasBasicSupport = true
            };
            return View(model);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriptionTierViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var adminUserId = _userManager.GetUserId(User) ?? "temp-admin-user";
            var result = await _tierService.CreateTierAsync(model, adminUserId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = $"Subscription tier '{model.DisplayName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result.ErrorMessage!);
            return View(model);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _tierService.GetTierByIdAsync(id);
            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubscriptionTierViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            var adminUserId = _userManager.GetUserId(User) ?? "temp-admin-user";
            var result = await _tierService.UpdateTierAsync(model, adminUserId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = $"Subscription tier '{model.DisplayName}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result.ErrorMessage!);
            return View(model);
        }

        [HttpGet("ChangePrice/{id:int}")]
        public async Task<IActionResult> ChangePrice(int id)
        {
            try
            {
                var model = await _tierService.GetPriceChangeModelAsync(id);
                return View(model);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpPost("ChangePrice/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePrice(int id, PriceChangeViewModel model)
        {
            if (id != model.SubscriptionTierId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            // Additional validation
            if (model.NewPrice == model.CurrentPrice)
            {
                ModelState.AddModelError(nameof(model.NewPrice), "New price must be different from current price");
                return View(model);
            }

            if (model.EffectiveDate < DateTime.UtcNow.Date)
            {
                ModelState.AddModelError(nameof(model.EffectiveDate), "Effective date cannot be in the past");
                return View(model);
            }

            // Require 30-day notice for price increases
            if (model.NewPrice > model.CurrentPrice && model.EffectiveDate < DateTime.UtcNow.AddDays(30))
            {
                ModelState.AddModelError(nameof(model.EffectiveDate), "Price increases require at least 30 days notice");
                return View(model);
            }

            var adminUserId = _userManager.GetUserId(User) ?? "temp-admin-user";
            var result = await _tierService.UpdateTierPriceAsync(model, adminUserId);

            if (result.IsSuccess)
            {
                var changeType = model.NewPrice > model.CurrentPrice ? "increase" : "decrease";
                TempData["SuccessMessage"] = $"Price {changeType} scheduled successfully! " +
                    (model.SendNotification ? "Customer notifications have been sent." : "No notifications were sent.");
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result.ErrorMessage!);
            return View(model);
        }

        [HttpPost("Deactivate/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var adminUserId = _userManager.GetUserId(User) ?? "temp-admin-user";
            var result = await _tierService.DeactivateTierAsync(id, adminUserId);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Subscription tier deactivated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _tierService.GetTierByIdAsync(id);
            if (model == null)
                return NotFound();

            return View(model);
        }

        // Debug action to test if routing works
        [HttpGet("Test/{id:int}")]
        public IActionResult Test(int id)
        {
            return Json(new { Message = "Test successful", Id = id });
        }
    }
}