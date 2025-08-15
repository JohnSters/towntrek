using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace TownTrek.Extensions
{
    public class ClientViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // No-op: we don't need per-request values
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            // Only apply to controllers in the Client namespace
            var actionDescriptor = context.ActionContext.ActionDescriptor as ControllerActionDescriptor;
            var controllerNamespace = actionDescriptor?.ControllerTypeInfo?.Namespace ?? string.Empty;
            var isClientController = controllerNamespace.Contains(".Controllers.Client", StringComparison.OrdinalIgnoreCase);

            if (!isClientController)
            {
                return viewLocations;
            }

            // Add additional search locations so controllers can discover views under Views/Client/**
            var clientLocations = new[]
            {
                "/Views/Client/{1}/{0}.cshtml",
                "/Views/Client/{1}s/{0}.cshtml",
                "/Views/Client/{1}es/{0}.cshtml",
                "/Views/Client/Shared/{0}.cshtml"
            };

            // Prepend our custom locations so they are checked first
            return clientLocations.Concat(viewLocations);
        }
    }
}


