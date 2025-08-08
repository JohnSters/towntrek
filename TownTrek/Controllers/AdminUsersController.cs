using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;
using TownTrek.Services;
using TownTrek.Services.Interfaces;

namespace TownTrek.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/users")]
    public class AdminUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ISubscriptionTierService _tierService;
        private readonly ILogger<AdminUsersController> _logger;

        public AdminUsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ISubscriptionTierService tierService, ILogger<AdminUsersController> logger)
        {
            _context = context;
            _userManager = userManager;
            _tierService = tierService;
            _logger = logger;
        }

        // GET /admin/users
        [HttpGet("")]
        public async Task<IActionResult> Index(string? roleFilter = "All", string? search = null, int page = 1, int pageSize = 20)
        {
            // Base query
            var query = _context.Users.AsQueryable();

            // Apply search on server side (FirstName, LastName, Email)
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                query = query.Where(u =>
                    (u.FirstName != null && u.FirstName.ToLower().Contains(q)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(q)) ||
                    (u.Email != null && u.Email.ToLower().Contains(q))
                );
            }

            // Role filtering
            if (!string.IsNullOrWhiteSpace(roleFilter) && !string.Equals(roleFilter, "All", StringComparison.OrdinalIgnoreCase))
            {
                var userIds = new HashSet<string>();
                if (roleFilter.Equals("Admins", StringComparison.OrdinalIgnoreCase))
                {
                    var admins = await _userManager.GetUsersInRoleAsync("Admin");
                    foreach (var u in admins) userIds.Add(u.Id);
                }
                else if (roleFilter.Equals("Members", StringComparison.OrdinalIgnoreCase))
                {
                    var members = await _userManager.GetUsersInRoleAsync("Member");
                    foreach (var u in members) userIds.Add(u.Id);
                }
                else if (roleFilter.Equals("Clients", StringComparison.OrdinalIgnoreCase))
                {
                    var cb = await _userManager.GetUsersInRoleAsync("Client-Basic");
                    var cs = await _userManager.GetUsersInRoleAsync("Client-Standard");
                    var cp = await _userManager.GetUsersInRoleAsync("Client-Premium");
                    foreach (var u in cb.Concat(cs).Concat(cp)) userIds.Add(u.Id);
                }

                query = query.Where(u => userIds.Contains(u.Id));
            }

            // Count for pagination
            var totalItems = await query.CountAsync();
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 20;
            var skip = (page - 1) * pageSize;

            // Page users with subscription details
            var pagedUsers = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.SubscriptionTier)
                .ToListAsync();

            var pagedUserIds = pagedUsers.Select(u => u.Id).ToList();

            // Business counts for paged users only
            var businessCounts = await _context.Businesses
                .Where(b => b.Status != "Deleted" && pagedUserIds.Contains(b.UserId))
                .GroupBy(b => b.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            var model = new AdminUsersViewModel
            {
                RoleFilter = roleFilter ?? "All",
                Search = search,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            foreach (var user in pagedUsers)
            {
                var roles = (await _userManager.GetRolesAsync(user)).ToList();
                var activeSubscription = user.Subscriptions
                    .OrderByDescending(s => s.StartDate)
                    .FirstOrDefault(s => s.IsActive);

                string? tierName = activeSubscription?.SubscriptionTier?.Name ?? user.CurrentSubscriptionTier;
                string? paymentStatus = activeSubscription?.PaymentStatus;

                model.Users.Add(new AdminUserListItem
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    Roles = roles,
                    SubscriptionTierName = tierName,
                    PaymentStatus = paymentStatus,
                    BusinessesCount = businessCounts.TryGetValue(user.Id, out var count) ? count : 0
                });
            }

            // Top-level stats (global)
            model.TotalUsers = await _context.Users.CountAsync();

            var adminsAll = await _userManager.GetUsersInRoleAsync("Admin");
            var membersAll = await _userManager.GetUsersInRoleAsync("Member");
            var cbAll = await _userManager.GetUsersInRoleAsync("Client-Basic");
            var csAll = await _userManager.GetUsersInRoleAsync("Client-Standard");
            var cpAll = await _userManager.GetUsersInRoleAsync("Client-Premium");
            model.TotalAdmins = adminsAll.Count;
            model.TotalMembers = membersAll.Count;
            model.TotalClients = cbAll.Select(u => u.Id).Concat(csAll.Select(u => u.Id)).Concat(cpAll.Select(u => u.Id)).Distinct().Count();

            model.ActiveSubscriptions = await _context.Subscriptions.CountAsync(s => s.IsActive);
            model.PendingPayments = await _context.Subscriptions.CountAsync(s => s.PaymentStatus == "Pending");

            return View("~/Views/Admin/Users.cshtml", model);
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest();

            var user = await _context.Users
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.SubscriptionTier)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var availableRoles = new List<string> { "Member", "Client-Basic", "Client-Standard", "Client-Premium", "Admin" };

            var activeSubscription = user.Subscriptions.FirstOrDefault(s => s.IsActive);
            var tiers = await _tierService.GetActiveTiersForRegistrationAsync();

            var model = new AdminEditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                SelectedRole = roles.FirstOrDefault() ?? "Member",
                AvailableRoles = availableRoles,
                SelectedSubscriptionTierId = activeSubscription?.SubscriptionTierId,
                AvailableTiers = tiers.Select(t => (t.Id, t.DisplayName)).ToList(),
                PaymentStatus = activeSubscription?.PaymentStatus,
                IsSubscriptionActive = activeSubscription?.IsActive ?? false,
                CurrentSubscriptionTierName = activeSubscription?.SubscriptionTier?.DisplayName ?? user.CurrentSubscriptionTier,
                SubscriptionEndDate = activeSubscription?.EndDate
            };

            return View("~/Views/Admin/EditUser.cshtml", model);
        }

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AdminEditUserViewModel model)
        {
            if (id != model.Id) return BadRequest();

            var user = await _context.Users
                .Include(u => u.Subscriptions)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            // Update basic fields
            user.FirstName = model.FirstName?.Trim() ?? user.FirstName;
            user.LastName = model.LastName?.Trim() ?? user.LastName;
            user.IsActive = model.IsActive;

            // Update role (ensure single primary role for simplicity)
            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, existingRoles);
            }
            await _userManager.AddToRoleAsync(user, model.SelectedRole);

            // Update subscription if client role selected
            var isClient = model.SelectedRole.StartsWith("Client-");
            if (isClient)
            {
                var activeSubscription = user.Subscriptions.FirstOrDefault(s => s.IsActive);

                if (model.SelectedSubscriptionTierId.HasValue)
                {
                    // Create or update active subscription record safely
                    if (activeSubscription == null)
                    {
                        activeSubscription = new Subscription
                        {
                            UserId = user.Id,
                            SubscriptionTierId = model.SelectedSubscriptionTierId.Value,
                            MonthlyPrice = await _context.SubscriptionTiers.Where(t => t.Id == model.SelectedSubscriptionTierId.Value).Select(t => t.MonthlyPrice).FirstAsync(),
                            StartDate = DateTime.UtcNow,
                            IsActive = model.IsSubscriptionActive,
                            PaymentStatus = string.IsNullOrWhiteSpace(model.PaymentStatus) ? "Pending" : model.PaymentStatus
                        };
                        await _context.Subscriptions.AddAsync(activeSubscription);
                    }
                    else
                    {
                        activeSubscription.SubscriptionTierId = model.SelectedSubscriptionTierId.Value;
                        activeSubscription.IsActive = model.IsSubscriptionActive;
                        activeSubscription.PaymentStatus = string.IsNullOrWhiteSpace(model.PaymentStatus) ? activeSubscription.PaymentStatus : model.PaymentStatus;
                    }
                }
            }
            else
            {
                // Non-client role: mark any active subscriptions inactive (administrative override)
                foreach (var sub in user.Subscriptions.Where(s => s.IsActive))
                {
                    sub.IsActive = false;
                    sub.PaymentStatus = "Cancelled";
                    sub.EndDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}


