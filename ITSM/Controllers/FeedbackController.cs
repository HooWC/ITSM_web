using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class FeedbackController : Controller
    {
        public IActionResult Feedback_List()
        {
            return View();
        }

        public IActionResult Feedback_Create()
        {
            return View();
        }
    }
}
