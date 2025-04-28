using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class PersonalController : Controller
    {
        public IActionResult Home()
        {
            return View();
        }

        public IActionResult Todo_List()
        {
            return View();
        }

        public IActionResult Incident_List()
        {
            return View();
        }

        public IActionResult Request_List()
        {
            return View();
        }

        public IActionResult Feedback_List()
        {
            return View();
        }
    }
}
