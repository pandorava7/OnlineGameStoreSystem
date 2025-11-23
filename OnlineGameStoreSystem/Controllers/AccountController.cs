using Microsoft.AspNetCore.Mvc;

namespace OnlineGameStoreSystem.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult register()
        {
            return View();
        }
    }
}
