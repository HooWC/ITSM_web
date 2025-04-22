using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult ForgotPasswrd()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
    }
}
