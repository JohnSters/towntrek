using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Controllers;

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

            // Detect if the controller belongs to the Admin namespace
            var actionDescriptor = context.ActionContext.ActionDescriptor as ControllerActionDescriptor;
            var controllerNamespace = actionDescriptor?.ControllerTypeInfo?.Namespace ?? string.Empty;
            var isAdminController = controllerNamespace.Contains(".Controllers.Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdminController)
            {
                return viewLocations;
            }

            // If controller starts with "Admin" (e.g., AdminBusinesses), trim the prefix.
            // Otherwise, use the full controller name (e.g., Errors -> Views/Admin/Errors/*)
            var folderName = controller.StartsWith("Admin", StringComparison.OrdinalIgnoreCase)
                ? controller.Substring("Admin".Length)
                : controller;

            // Provide both singular and plural folder fallbacks
            var candidates = new List<string>();
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                candidates.Add($"/Views/Admin/{folderName}/{{0}}.cshtml");
                var plural = folderName.EndsWith("s", StringComparison.OrdinalIgnoreCase) ? folderName : folderName + "s";
                if (!string.Equals(plural, folderName, StringComparison.OrdinalIgnoreCase))
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


