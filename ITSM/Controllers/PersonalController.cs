using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class PersonalController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
    
        public PersonalController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Home()
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            // todo info

            // team member

            // incident count

            // incident later 9 data

            // req count

            // knowledge count

            // feedback count
            
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

        public IActionResult Knowledge_List()
        {
            return View();
        }

        public IActionResult Feedback_List()
        {
            return View();
        }
    }
}
