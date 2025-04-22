using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class AnnouncementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
