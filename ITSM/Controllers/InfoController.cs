using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class InfoController : Controller
    {
        public IActionResult Info_Personal_Todo()
        {
            return View();
        }
    }
}
