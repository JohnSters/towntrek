using System;
using System.Collections.Generic;

namespace TownTrek.Models.ViewModels
{
    public class AdminUsersViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalMembers { get; set; }
        public int TotalClients { get; set; }
        public int TotalAdmins { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int PendingPayments { get; set; }

        public List<AdminUserListItem> Users { get; set; } = new();

        // Filtering & Search
        public string RoleFilter { get; set; } = "All"; // All | Members | Clients | Admins
        public string? Search { get; set; }

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalItems { get; set; }
        public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling((double)Math.Max(TotalItems, 1) / PageSize);
    }

    public class AdminUserListItem
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public List<string> Roles { get; set; } = new();
        public string? SubscriptionTierName { get; set; }
        public string? PaymentStatus { get; set; }
        public int BusinessesCount { get; set; }

        public string FullName => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
            ? Email
            : $"{FirstName} {LastName}".Trim();
    }

    public class AdminEditUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // Role selection
        public string SelectedRole { get; set; } = "Member"; // Member | Client-* | Admin
        public List<string> AvailableRoles { get; set; } = new();

        // Subscription controls (for Clients)
        public int? SelectedSubscriptionTierId { get; set; }
        public List<(int Id, string Name)> AvailableTiers { get; set; } = new();
        public string? PaymentStatus { get; set; } // Pending | Active | Completed | Failed
        public bool IsSubscriptionActive { get; set; }

        // Display
        public string? CurrentSubscriptionTierName { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
    }
}


