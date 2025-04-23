using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class IncidentManagementController : Controller
    {
        public IActionResult All()
        {
            return View();
        }

        public IActionResult Create_New()
        {
            return View();
        }
    }
}
