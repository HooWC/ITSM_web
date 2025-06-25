using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class OtherPageController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Auth_api _authApi;
        private readonly User_api _userApi;
        private readonly Todo_api _todoApi;
        private readonly Feedback_api _feedbackApi;
        private readonly Incident_api _incApi;
        private readonly Knowledge_api _knowledgeApi;
        private readonly Request_api _reqApi;
        private readonly Department_api _depApi;
        private readonly Role_api _roleApi;
        private readonly Category_api _categoryApi;
        private readonly Product_api _productApi;
        private readonly Department_api _departmentApi;
        private readonly UserService _userService;

        public OtherPageController(IHttpContextAccessor httpContextAccessor, UserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authApi = new Auth_api(httpContextAccessor);
            _userApi = new User_api(httpContextAccessor);
            _todoApi = new Todo_api(httpContextAccessor);
            _feedbackApi = new Feedback_api(httpContextAccessor);
            _incApi = new Incident_api(httpContextAccessor);
            _knowledgeApi = new Knowledge_api(httpContextAccessor);
            _reqApi = new Request_api(httpContextAccessor);
            _depApi = new Department_api(httpContextAccessor);
            _roleApi = new Role_api(httpContextAccessor);
            _categoryApi = new Category_api(httpContextAccessor);
            _productApi = new Product_api(httpContextAccessor);
            _departmentApi = new Department_api(httpContextAccessor);
            _userService = userService;
        }

        public async Task<AllModelVM> get_data()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var model = new AllModelVM()
            {
                user = currentUser
            };

            return model;
        }

        public async Task<IActionResult> Show_Session()
        {
            var model = await get_data();

            return View(model);
        }

        public async Task<IActionResult> About_ITSM()
        {
            var model = await get_data();

            return View(model);
        }

        public async Task<IActionResult> FAQ()
        {
            var model = await get_data();

            return View(model);
        }
    }
}
