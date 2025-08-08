using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TownTrek.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/users")]
    public class AdminUsersController : Controller
    {
        // GET /admin/users
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Users.cshtml");
        }
    }
}


