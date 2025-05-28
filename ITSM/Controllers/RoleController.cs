using ITSM_DomainModelEntity.Models;
using System.Runtime.Intrinsics.Arm;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ITSM.Controllers
{
    public class RoleController : Controller
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

        public RoleController(IHttpContextAccessor httpContextAccessor, UserService userService)
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
        }

        public async Task<IActionResult> Role_List()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var roleTask = _roleApi.GetAllRole_API();
            await Task.WhenAll(roleTask);

            var allRole = roleTask.Result;

            var model = new AllModelVM
            {
                user = currentUser,
                RoleList = allRole
            };

            return View(model);
        }

        public async Task<IActionResult> Role_Create()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var model = new AllModelVM
            {
                user = currentUser
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Role_Create(Role roleName)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var model = new AllModelVM
            {
                user = currentUser
            };

            if (roleName.role == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var allRole = await _roleApi.GetAllRole_API();
            bool b = allRole.Any(x => x.role.ToLower() == roleName.role.ToLower());

            if (b)
            {
                ViewBag.Error = "This role already exist.";
                return View(model);
            }

            // Create New Role
            Role new_role = new Role()
            {
                role = roleName.role
            };

            // API requests
            bool result = await _roleApi.CreateRole_API(new_role);

            if (result)
                return RedirectToAction("Role_List", "Role");
            else
            {
                ViewBag.Error = "Create Role Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Role_Info(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            // Get Role
            var role = await _roleApi.FindByIDRole_API(id);

            var model = new AllModelVM
            {
                user = currentUser,
                role = role
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Role_Info(Role roleInfo)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            // Making concurrent API requests
            var roleTask = await _roleApi.FindByIDRole_API(roleInfo.id);

            var model = new AllModelVM
            {
                user = currentUser,
                role = roleTask
            };

            if (roleTask.role == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(roleTask);
            }

            var allRole = await _roleApi.GetAllRole_API();
            bool b = allRole.Any(x => x.role.ToLower() == roleInfo.role.ToLower() && x.id != roleInfo.id);

            if (b)
            {
                ViewBag.Error = "This role already exist.";
                return View(model);
            }

            // Update New ROle
            roleTask.role = roleInfo.role;

            // API requests
            bool result = await _roleApi.UpdateRole_API(roleTask);

            if (result)
                return RedirectToAction("Role_List", "Role");
            else
            {
                ViewBag.Error = "Update Role Error";
                return View(model);
            }
        }
    }
}
