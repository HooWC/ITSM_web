using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ITSM.Models;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;

namespace ITSM.Controllers;

public class HomeController : Controller
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

    public HomeController(IHttpContextAccessor httpContextAccessor)
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
    }

    public async Task<IActionResult> Index()
    {
        var tokenService = new TokenService(_httpContextAccessor);
        var currentUser_token = tokenService.GetUserInfo();

        var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

        ViewBag.Photo = currentUser.photo;
        ViewBag.PhotoType = currentUser.photo_type;

        return View();
    }  
}
