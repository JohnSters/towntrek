using Microsoft.AspNetCore.Mvc.Rendering;
using TownTrek.Models;
using TownTrek.Services;

namespace TownTrek.Extensions
{
    public static class ViewExtensions
    {
        public static bool HasFeature(this IHtmlHelper htmlHelper, string featureKey)
        {
            var limits = htmlHelper.ViewData["UserLimits"] as SubscriptionLimits;
            if (limits == null) return false;

            return featureKey switch
            {
                "BasicSupport" => limits.HasBasicSupport,
                "PrioritySupport" => limits.HasPrioritySupport,
                "DedicatedSupport" => limits.HasDedicatedSupport,
                "BasicAnalytics" => limits.HasBasicAnalytics,
                "AdvancedAnalytics" => limits.HasAdvancedAnalytics,
                "FeaturedPlacement" => limits.HasFeaturedPlacement,
                "PDFUploads" => limits.HasPDFUploads,
                _ => false
            };
        }

        public static bool CanAddBusiness(this IHtmlHelper htmlHelper)
        {
            var limits = htmlHelper.ViewData["UserLimits"] as SubscriptionLimits;
            if (limits == null) return false;

            return limits.MaxBusinesses == -1 || limits.CurrentBusinessCount < limits.MaxBusinesses;
        }

        public static bool CanAddImages(this IHtmlHelper htmlHelper)
        {
            var limits = htmlHelper.ViewData["UserLimits"] as SubscriptionLimits;
            if (limits == null) return false;

            return limits.MaxImages == -1 || limits.CurrentImageCount < limits.MaxImages;
        }

        public static string GetSubscriptionTierName(this IHtmlHelper htmlHelper)
        {
            var tier = htmlHelper.ViewData["UserSubscriptionTier"] as SubscriptionTier;
            return tier?.DisplayName ?? "No Plan";
        }

        public static string GetSubscriptionTierBadgeClass(this IHtmlHelper htmlHelper)
        {
            var tier = htmlHelper.ViewData["UserSubscriptionTier"] as SubscriptionTier;
            if (tier == null) return "badge-secondary";

            return tier.Name.ToLower() switch
            {
                "basic" => "badge-info",
                "standard" => "badge-primary",
                "premium" => "badge-success",
                _ => "badge-secondary"
            };
        }

        public static int GetBusinessesRemaining(this IHtmlHelper htmlHelper)
        {
            var limits = htmlHelper.ViewData["UserLimits"] as SubscriptionLimits;
            if (limits == null) return 0;

            if (limits.MaxBusinesses == -1) return int.MaxValue;
            return Math.Max(0, limits.MaxBusinesses - limits.CurrentBusinessCount);
        }

        public static int GetImagesRemaining(this IHtmlHelper htmlHelper)
        {
            var limits = htmlHelper.ViewData["UserLimits"] as SubscriptionLimits;
            if (limits == null) return 0;

            if (limits.MaxImages == -1) return int.MaxValue;
            return Math.Max(0, limits.MaxImages - limits.CurrentImageCount);
        }
    }
}