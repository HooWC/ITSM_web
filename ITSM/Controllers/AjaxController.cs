using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class AjaxController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
