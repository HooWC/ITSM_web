using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserService _userService;
        private readonly User_api _userApi;
        private readonly Todo_api _todoApi;
        private readonly Feedback_api _feedbackApi;
        private readonly Incident_api _incApi;
        private readonly Knowledge_api _knowledgeApi;
        private readonly Request_api _reqApi;
        private readonly Department_api _depApi;
        private readonly Role_api _roleApi;
        private readonly Category_api _categoryApi;

        public DepartmentController(IHttpContextAccessor httpContextAccessor, UserService userService)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _userApi = new User_api(httpContextAccessor);
            _todoApi = new Todo_api(httpContextAccessor);
            _feedbackApi = new Feedback_api(httpContextAccessor);
            _incApi = new Incident_api(httpContextAccessor);
            _knowledgeApi = new Knowledge_api(httpContextAccessor);
            _reqApi = new Request_api(httpContextAccessor);
            _depApi = new Department_api(httpContextAccessor);
            _roleApi = new Role_api(httpContextAccessor);
            _categoryApi = new Category_api(httpContextAccessor);
        }

        public async Task<IActionResult> Department_List()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var depTask = _depApi.GetAllDepartment_API();
            await Task.WhenAll(depTask);

            var allDepartment = depTask.Result;

            var departmentList = allDepartment.OrderByDescending(y => y.id).ToList();

            var model = new AllModelVM
            {
                user = currentUser,
                DepartmentList = departmentList,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        public async Task<IActionResult> Department_Create()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var model = new AllModelVM
            {
                user = currentUser,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Department_Create(Department dep)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var model = new AllModelVM
            {
                user = currentUser,
                noteMessageCount = noteMessageCount
            };

            if (dep.name == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            Department new_department = new Department()
            {
                name = dep.name,
                description = dep.description
            };

            bool result = await _depApi.CreateDepartment_API(new_department);

            if (result)
                return RedirectToAction("Department_List", "Department");
            else
            {
                ViewBag.Error = "Create Department Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Department_Info(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var department = await _depApi.FindByIDDepartment_API(id);

            var model = new AllModelVM
            {
                user = currentUser,
                department = department,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Department_Info(Department dep)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var departmentTask = await _depApi.FindByIDDepartment_API(dep.id);

            var model = new AllModelVM
            {
                user = currentUser,
                department = departmentTask,
                noteMessageCount = noteMessageCount
            };

            if (departmentTask.name == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(departmentTask);
            }

            departmentTask.name = dep.name;
            departmentTask.description = dep.description;

            bool result = await _depApi.UpdateDepartment_API(departmentTask);

            if (result)
                return RedirectToAction("Department_List", "Department");
            else
            {
                ViewBag.Error = "Update Department Error";
                return View(departmentTask);
            }
        }
    }
}
