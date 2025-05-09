using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly User_api _userApi;
        private readonly Todo_api _todoApi;
        private readonly Feedback_api _feedbackApi;
        private readonly Incident_api _incApi;
        private readonly Knowledge_api _knowledgeApi;
        private readonly Request_api _reqApi;
        private readonly Department_api _depApi;
        private readonly Role_api _roleApi;

        public FeedbackController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _userApi = new User_api(httpContextAccessor);
            _todoApi = new Todo_api(httpContextAccessor);
            _feedbackApi = new Feedback_api(httpContextAccessor);
            _incApi = new Incident_api(httpContextAccessor);
            _knowledgeApi = new Knowledge_api(httpContextAccessor);
            _reqApi = new Request_api(httpContextAccessor);
            _depApi = new Department_api(httpContextAccessor);
            _roleApi = new Role_api(httpContextAccessor);
        }

        public async Task<IActionResult> Feedback_List()
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            // Making concurrent API requests
            var feed = _feedbackApi.GetAllFeedback_API();

            // Wait for all tasks to complete
            await Task.WhenAll(feed);

            // get incident list data
            var allFeed = await feed;
            var incList = allFeed.Where(x => x.user_id == currentUser.id).OrderByDescending(y => y.id).ToList();

            // get user and department data
            var allDepartments = await dep;
            var allUsers = await user;

            foreach (var incident in incList)
            {
                incident.AssignmentGroup = allDepartments.FirstOrDefault(d => d.id == incident.assignment_group);
                incident.AssignedTo = allUsers.FirstOrDefault(u => u.id == incident.assigned_to);
            }

            var model = new IncidentVM
            {
                User = currentUser,
                Inc = incList
            };

            return View(model);
        }

        public IActionResult Feedback_Create()
        {
            return View();
        }
    }
}
