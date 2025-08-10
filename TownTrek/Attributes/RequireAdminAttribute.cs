using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace TownTrek.Attributes
{
    public class RequireAdminAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Check if user is authenticated
            if (!user.Identity?.IsAuthenticated == true)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Check if user has Admin role
            if (!user.IsInRole("Admin"))
            {
                // Redirect to access denied or appropriate page
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}