using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class CMDBController : Controller
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
        private readonly Announcement_api _announApi;
        private readonly CMDB_api _cmdbApi;

        public CMDBController(IHttpContextAccessor httpContextAccessor)
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
            _announApi = new Announcement_api(httpContextAccessor);
            _cmdbApi = new CMDB_api(httpContextAccessor);
        }

        public async Task<IActionResult> CMDB_List()
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var CMDBTask = _cmdbApi.GetAllCMDB_API();
            var DepartmentTask = _depApi.GetAllDepartment_API();
            await Task.WhenAll(CMDBTask, DepartmentTask);

            var allCMDB = await CMDBTask;
            var allDep = await DepartmentTask;

            foreach (var i in allCMDB)
                i.Department = allDep.FirstOrDefault(x => x.id == i.department_id);

            var model = new AllModelVM
            {
                user = currentUser,
                CMDBList = allCMDB.OrderByDescending(X => X.id).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> CMDB_Create()
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var DepartmentTask = _depApi.GetAllDepartment_API();
            await Task.WhenAll(DepartmentTask);

            var allDep = await DepartmentTask;

            var model = new AllModelVM()
            {
                user = currentUser,
                DepartmentList = allDep
            };

            return View(model);
        }

        public IActionResult CMDB_Info()
        {
            return View();
        }
    }
}
