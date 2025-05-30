using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class InfoController : Controller
    {
        public IActionResult Info_Personal_Todo()
        {
            return View();
        }

        public IActionResult Analysis_For_Today_Info()
        {
            return View();
        }

        public IActionResult All_List_Info()
        {
            return View();
        }

        public IActionResult Incident_Priority_Info()
        {
            return View();
        }


    }
}
