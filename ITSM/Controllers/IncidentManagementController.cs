using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class IncidentManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
