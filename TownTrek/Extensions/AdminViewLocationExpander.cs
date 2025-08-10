using Microsoft.AspNetCore.Mvc.Razor;

namespace TownTrek.Extensions
{
    public class AdminViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // No values needed
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var controller = context.ControllerName ?? string.Empty;
            if (!controller.StartsWith("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return viewLocations;
            }

            var trimmed = controller.StartsWith("Admin", StringComparison.OrdinalIgnoreCase)
                ? controller.Substring("Admin".Length)
                : controller;

            // Provide both singular and plural folder fallbacks
            var candidates = new List<string>();
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                candidates.Add($"/Views/Admin/{trimmed}/{{0}}.cshtml");
                var plural = trimmed.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? trimmed : trimmed + "s";
                if (!string.Equals(plural, trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    candidates.Add($"/Views/Admin/{plural}/{{0}}.cshtml");
                }
            }
            candidates.Add("/Views/Admin/Shared/{0}.cshtml");

            // Prepend admin-specific locations
            return candidates.Concat(viewLocations);
        }
    }
}


