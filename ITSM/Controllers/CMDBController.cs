using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class CMDBController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
