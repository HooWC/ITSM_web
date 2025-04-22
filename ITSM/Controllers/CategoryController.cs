using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
