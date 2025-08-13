using Microsoft.AspNetCore.Mvc;
using TownTrek.Services.Interfaces;
using System.Security.Claims;

namespace TownTrek.ViewComponents
{
    public class TrialStatusViewComponent : ViewComponent
    {
        private readonly ITrialService _trialService;

        public TrialStatusViewComponent(ITrialService trialService)
        {
            _trialService = trialService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Content("");
            }

            var trialStatus = await _trialService.GetTrialStatusAsync(userId);
            if (!trialStatus.IsTrialUser)
            {
                return Content("");
            }

            return View(trialStatus);
        }
    }
}