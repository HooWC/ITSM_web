using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class RequestController : Controller
    {
        public IActionResult All()
        {
            return View();
        }

        public IActionResult Service_Catalog()
        {
            return View();
        }

        public IActionResult Create_Form()
        {
            return View();
        }

        public IActionResult Assigned_To_Us()
        {
            return View();
        }
    }
}
