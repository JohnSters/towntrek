using Microsoft.AspNetCore.Mvc.Razor;

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
            // Add additional search locations so controllers can
            // discover views located under Views/Client/**
            // Include plural fallbacks to support folders like "Businesses"
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


