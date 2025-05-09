using Humanizer;
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

        // ????
        //public async Task<IActionResult> Admin_All()
        //{

        //}

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
            var incList = allInc.Where(x => x.sender == currentUser.id).OrderByDescending(y => y.id).ToList();

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

        public IActionResult Create_Form()
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            ViewBag.CurrentUser = currentUser.fullname;
            ViewBag.Photo = currentUser.photo;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create_Form(Incident inc)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            ViewBag.CurrentUser = currentUser.fullname;
            ViewBag.Photo = currentUser.photo;

            if (inc.short_description == null && inc.AssignmentGroup == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View();
            }

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
                assigned_to = inc.assigned_to == 0 ? null : inc.assigned_to,
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

        public async Task<IActionResult> Inc_Info_Form(int id)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            // Making concurrent API requests
            var departmentTask = _depApi.GetAllDepartment_API();
            var userTask = _userApi.GetAllUser_API();

            // Wait for all tasks to complete
            await Task.WhenAll(departmentTask, userTask);

            // Ready Api Data
            var allDepartment = await departmentTask;
            var allUser = await userTask;

            // Get Inc Data
            var incData = await _incApi.FindByIDIncident_API(id);

            // Get data
            incData.AssignmentGroup = allDepartment.Where(x => x.id == incData.assignment_group).FirstOrDefault();
            incData.AssignedTo = incData.assigned_to == null ? null : allUser.FirstOrDefault(x => x.id == incData.assigned_to);

            var model = new IncidentInfoVM()
            {
                User = currentUser,
                Inc = incData
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Inc_Info_Form(Incident inc)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            // Making concurrent API requests
            var departmentTask = _depApi.GetAllDepartment_API();
            var userTask = _userApi.GetAllUser_API();

            // Wait for all tasks to complete
            await Task.WhenAll(departmentTask, userTask);

            // Ready Api Data
            var allDepartment = await departmentTask;
            var allUser = await userTask;

            // Get Inc Data
            var incData = await _incApi.FindByIDIncident_API(inc.id);

            if (inc.short_description == null && inc.AssignmentGroup == null)
            {
                ViewBag.Error = "Please fill in all required fields";

                incData.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == incData.assignment_group);
                incData.AssignedTo = incData.assigned_to == null ? null : allUser.FirstOrDefault(x => x.id == incData.assigned_to);

                var model = new IncidentInfoVM()
                {
                    User = currentUser,
                    Inc = incData
                };

                return View(model);
            }

            if (incData != null)
            {
                incData.short_description = inc.short_description;
                incData.describe = inc.describe;
                incData.impact = inc.impact;
                incData.urgency = inc.urgency;
                incData.priority = inc.priority;
                incData.state = inc.state;
                incData.category = inc.category;
                incData.subcategory = inc.subcategory;
                incData.assignment_group = inc.assignment_group;
                incData.assigned_to = inc.assigned_to == 0 ? null : inc.assigned_to;
                incData.updated_by = currentUser.id;

                bool result = await _incApi.UpdateIncident_API(incData);

                if (result)
                    return RedirectToAction("All", "IncidentManagement");
                else
                {
                    ViewBag.Error = "Update event failed";

                    incData.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == incData.assignment_group);
                    incData.AssignedTo = incData.assigned_to == null ? null : allUser.FirstOrDefault(x => x.id == incData.assigned_to);

                    var model = new IncidentInfoVM()
                    {
                        User = currentUser,
                        Inc = incData
                    };

                    return View(model);
                }
            }

            return RedirectToAction("All", "IncidentManagement");
        }

        public async Task<IActionResult> Assigned_To_Me()
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
            var incList = allInc.Where(x => x.assigned_to == currentUser.id).OrderByDescending(y => y.id).ToList();

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

        public async Task<IActionResult> Assigned_To_Group()
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
            var incList = allInc.Where(x => x.assignment_group == currentUser.department_id).OrderByDescending(y => y.id).ToList();

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

        public async Task<IActionResult> Resolved_Assigned_To_Me()
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
            var incList = allInc.Where(x => x.assigned_to == currentUser.id || x.updated_by == currentUser.id && x.state == "Resolved").OrderByDescending(y => y.id).ToList();

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

        public async Task<IActionResult> Closed_Assigned_To_Me()
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
            var incList = allInc.Where(x => (x.assigned_to == currentUser.id || x.updated_by == currentUser.id) && x.state == "Closed").OrderByDescending(y => y.id).ToList();

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

    }
}
