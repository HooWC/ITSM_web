using Humanizer;
using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class PersonalController : Controller
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
    
        public PersonalController(IHttpContextAccessor httpContextAccessor)
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

        public async Task<IActionResult> Home()
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            // Making concurrent API requests
            var todoTask = _todoApi.GetAllTodo_API();
            var incidentTask = _incApi.GetAllIncident_API();
            var userTask = _userApi.GetAllUser_API();
            var reqTask = _reqApi.GetAllRequest_API();
            var knowledgeTask = _knowledgeApi.GetAllKnowledge_API();
            var feedbackTask = _feedbackApi.GetAllFeedback_API();
            var departmentTask = _depApi.GetAllDepartment_API();
            var roleTask = _roleApi.GetAllRole_API();

            // Wait for all tasks to complete
            await Task.WhenAll(todoTask, incidentTask, userTask, reqTask, knowledgeTask, feedbackTask, departmentTask, roleTask);

            // Linq get api data
            var allTodo = await todoTask;
            var todo = allTodo.Where(x => x.user_id == currentUser.id).ToList();
            var todo_c_count = todo.Count(x => x.active);
            var todo_td_count = todo.Count(x => !x.active);
            var todo_all_count = todo.Count;

            var allIncident = await incidentTask;
            var incident = allIncident.Where(x => x.sender == currentUser.id).ToList();
            var incident_list = incident.Take(9).ToList();
            var incident_r_count = incident.Count(x => x.state == "Resolved");
            var incident_p_count = incident.Count(x => x.state == "Pending");
            var incident_all_count = incident.Count;

            var allUser = await userTask;
            var sameDepartment = allUser.Where(x => x.department_id == currentUser.department_id).Take(9).ToList();

            var allReq = await reqTask;
            var req = allReq.Where(x => x.sender == currentUser.id).ToList();
            var req_r_count = req.Count(x => x.state == "Resolved");
            var req_p_count = req.Count(x => x.state == "Pending");
            var req_all_count = req.Count;

            var allKnowledge = await knowledgeTask;
            var knowledge_count = allKnowledge.Count(x => x.author == currentUser.id);

            var allFeedback = await feedbackTask;
            var feedback_count = allFeedback.Count(x => x.user_id == currentUser.id);

            var allDepartment = await departmentTask;
            var getDepartmentName = allDepartment.Where(x => x.id == currentUser.department_id).FirstOrDefault()?.name;

            var allRole = await roleTask;
            var getRoleName = allRole.Where(x => x.id == currentUser.role_id).FirstOrDefault()?.role;

            // return to view data
            var model = new PersonalHomeVM
            {
                User = currentUser,
                CompletedTodo = todo_c_count,
                TodoCount = todo_td_count,
                AllTodo = todo_all_count,
                AllInc = incident_all_count,
                ApplyInc = incident_p_count,
                CompletedInc = incident_r_count,
                AllReq = req_all_count,
                ApplyReq = req_p_count,
                CompletedReq = req_r_count,
                AllKnowledge = knowledge_count,
                AllFeedback = feedback_count,
                DepartmentName = getDepartmentName,
                RoleName = getRoleName,
                Team = sameDepartment,
                IncidentsHistory = incident_list,
            };

            return View(model);
        }

        public async Task<IActionResult> Todo_List()
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            // Making concurrent API requests
            var todoTask = _todoApi.GetAllTodo_API();

            // get todo list data
            var allTodo = await todoTask;
            var todo = allTodo.Where(x => x.user_id == currentUser.id).ToList();

            // return to view data
            var model = new TodoVM
            {
                User = currentUser,
                Todo = todo,
            };

            return View(model);
        }

        public async Task<IActionResult> Todo_Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Todo_Create(Todo todo, string active_word)
        {
            if (todo.title == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View();
            }

            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            // Making concurrent API requests
            var todoTask = _todoApi.GetAllTodo_API();

            // get todo new id
            var allTodo = await todoTask;
            
            string newId = "";
            if (allTodo.Count > 0)
            {
                var last_todo = allTodo.Last();
                string t_id_up = last_todo.todo_id;
                string prefix = new string(t_id_up.TakeWhile(char.IsLetter).ToArray());
                string numberPart = new string(t_id_up.SkipWhile(char.IsLetter).ToArray());
                int number = int.Parse(numberPart);
                newId = prefix + (number + 1);
            }
            else
                newId = "TOD1";

            // Create New Todo
            Todo new_todo = new Todo()
            {
                user_id = currentUser.id,
                title = todo.title,
                create_date = DateTime.Now,
                update_date = DateTime.Now,
                todo_id = newId,
                active = active_word == "Doing" ? false : true
            };

            // API requests
            bool result = await _todoApi.CreateTodo_API(new_todo);
 
            if (result)
                return RedirectToAction("Todo_List", "Personal");
            else
            {
                ViewBag.Error = "Create Todo Error";
                return View();
            }
        }

        public async Task<IActionResult> Todo_Edit(int id)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            // Making concurrent API requests
            var todoTask = _todoApi.GetAllTodo_API();

            // Get Todo
            var allTodo = await todoTask;
            var todo = allTodo.Where(x => x.user_id == currentUser.id && x.id == id).FirstOrDefault();

            return View(todo);
        }

        [HttpPost]
        public async Task<IActionResult> Todo_Edit(Todo todo, string active_word)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser = tokenService.GetUserInfo();

            // Making concurrent API requests
            var todoTask = _todoApi.GetAllTodo_API();

            // Get Todo
            var allTodo = await todoTask;
            var edit_todo = allTodo.Where(x => x.user_id == currentUser.id && x.id == todo.id).FirstOrDefault();
            if(edit_todo != null)
            {
                // Update Todo Data
                edit_todo.active = active_word == "Doing" ? false : true;
                edit_todo.title = todo.title;

                bool result = await _todoApi.UpdateTodo_API(edit_todo);

                if (result)
                    return RedirectToAction("Todo_List", "Personal");
                else
                    return View();
            }

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
