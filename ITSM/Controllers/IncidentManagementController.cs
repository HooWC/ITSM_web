using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class IncidentManagementController : Controller
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

        public IncidentManagementController(IHttpContextAccessor httpContextAccessor)
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
        public async Task<IActionResult> All()
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            // Making concurrent API requests
            var inc = _incApi.GetAllIncident_API();
            var dep = _depApi.GetAllDepartment_API();
            var user = _userApi.GetAllUser_API();

            // Wait for all tasks to complete
            await Task.WhenAll(inc, dep, user);

            // get incident list data
            var allInc = await inc;
            var incList = allInc.Where(x => x.sender == currentUser.id).ToList();

            // get user and department data
            var allDepartments = await dep;
            var allUsers = await user;

            // 为每个事件手动设置分配组和分配人
            foreach (var incident in incList)
            {
                incident.AssignmentGroup = allDepartments.FirstOrDefault(d => d.id == incident.assignment_group);
                incident.AssignedTo = allUsers.FirstOrDefault(u => u.id == incident.assigned_to);
            }

            // 创建一个单一的IncidentVM对象
            var model = new IncidentVM
            {
                User = currentUser,
                Inc = incList
            };

            return View(model);
        }

        public IActionResult Create_Form()
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            ViewBag.CurrentUser = currentUser.fullname;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create_Form(Incident inc)
        {
            if(inc.short_description == null && inc.AssignmentGroup == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View();
            }

            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            // Making concurrent API requests
            var incidentTask = _incApi.GetAllIncident_API();

            // Wait for all tasks to complete
            await Task.WhenAll(incidentTask);

            // Up Number
            var allIncident = await incidentTask;
            string newId = "";
            if (allIncident.Count > 0)
            {
                var incidentLast = allIncident.Last();
                string i_id_up = incidentLast.inc_number;
                string prefix = new string(i_id_up.TakeWhile(char.IsLetter).ToArray());
                string numberPart = new string(i_id_up.SkipWhile(char.IsLetter).ToArray());
                int number = int.Parse(numberPart);
                newId = prefix + (number + 1);
            }
            else
                newId = "INC1";

            // Create New Incident
            var new_inc = new Incident()
            {
                inc_number = newId,
                short_description = inc.short_description,
                describe = inc.describe,
                sender = currentUser.id,
                impact = inc.impact,
                urgency = inc.urgency,
                priority = inc.priority,
                state = inc.state,
                category = inc.category,
                subcategory = inc.subcategory,
                assignment_group = inc.assignment_group,
                assigned_to = inc.assigned_to,
                updated_by = currentUser.id
            };

            // API requests
            bool result = await _incApi.CreateIncident_API(new_inc);

            if (result)
                return RedirectToAction("All", "IncidentManagement");
            else
            {
                ViewBag.Error = "Create Incident Error";
                return View();
            }       
        }

        public IActionResult Assigned_To_Me()
        {
            return View();
        }
    }
}
