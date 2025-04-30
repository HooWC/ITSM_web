using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class AnnouncementController : Controller
    {
        public IActionResult Ann_List()
        {
            return View();
        }

        public IActionResult Ann_Create()
        {
            return View();
        }
    }
}
