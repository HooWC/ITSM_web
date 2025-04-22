using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class DepartmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
