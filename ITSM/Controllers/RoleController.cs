using ITSM_DomainModelEntity.Models;
using System.Runtime.Intrinsics.Arm;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class RoleController : Controller
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

        public RoleController(IHttpContextAccessor httpContextAccessor)
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

        public async Task<IActionResult> Role_List()
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

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
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            ViewBag.Photo = currentUser.photo;
            ViewBag.PhotoType = currentUser.photo_type;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Role_Create(Role roleName)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            ViewBag.Photo = currentUser.photo;
            ViewBag.PhotoType = currentUser.photo_type;

            if (roleName.role == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View();
            }

            var allRole = await _roleApi.GetAllRole_API();
            bool b = allRole.Any(x => x.role.ToLower() == roleName.role.ToLower());

            if (b)
            {
                ViewBag.Error = "This role already exist.";
                return View();
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
                return View();
            }
        }

        public async Task<IActionResult> Role_Info(int id)
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            ViewBag.Photo = currentUser.photo;
            ViewBag.PhotoType = currentUser.photo_type;

            // Get Role
            var role = await _roleApi.FindByIDRole_API(id);

            return View(role);
        }

        [HttpPost]
        public async Task<IActionResult> Role_Info(Role roleInfo)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            ViewBag.Photo = currentUser.photo;
            ViewBag.PhotoType = currentUser.photo_type;

            // Making concurrent API requests
            var roleTask = await _roleApi.FindByIDRole_API(roleInfo.id);

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
                return View(roleTask);
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
                return View(roleTask);
            }
        }
    }
}
