using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TownTrek.Controllers.Client
{
    [Authorize]
    [Route("Client/Documentation/[action]")]
    public class DocumentationController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}


