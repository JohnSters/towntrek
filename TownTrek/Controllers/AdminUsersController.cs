using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TownTrek.Data;
using TownTrek.Models;
using TownTrek.Models.ViewModels;

namespace TownTrek.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/users")]
    public class AdminUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
    }
}


