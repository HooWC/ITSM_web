using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class OtherPageController : Controller
    {
        public IActionResult Show_Session()
        {
            return View();
        }
    }
}
