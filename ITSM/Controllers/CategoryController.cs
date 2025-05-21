using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class CategoryController : Controller
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
        private readonly Category_api _categoryApi;

        public CategoryController(IHttpContextAccessor httpContextAccessor)
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
            _categoryApi = new Category_api(httpContextAccessor);
        }

        public async Task<IActionResult> Category_List()
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var categoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(categoryTask);

            var allCategory = await categoryTask;

            var categoryList = allCategory.OrderByDescending(y => y.id).ToList();

            var model =  new AllModelVM
            {
                user = currentUser,
                CategoryList = categoryList
            };

            return View(model);
        }

        public async Task<IActionResult> Category_Create()
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
        public async Task<IActionResult> Category_Create(Category category)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            ViewBag.Photo = currentUser.photo;
            ViewBag.PhotoType = currentUser.photo_type;

            if (category.title == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View();
            }

            // Create New Category
            Category new_category = new Category()
            {
                title = category.title,
                description = category.description
            };

            // API requests
            bool result = await _categoryApi.CreateCategory_API(new_category);

            if (result)
                return RedirectToAction("Category_List", "Category");
            else
            {
                ViewBag.Error = "Create Category Error";
                return View();
            }
        }

        public async Task<IActionResult> Category_Info(int id)
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            ViewBag.Photo = currentUser.photo;
            ViewBag.PhotoType = currentUser.photo_type;

            // Get Category
            var category = await _categoryApi.FindByIDCategory_API(id);

            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Category_Info(Category c)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            ViewBag.Photo = currentUser.photo;
            ViewBag.PhotoType = currentUser.photo_type;

            // Making concurrent API requests
            var categoryTask = await _categoryApi.FindByIDCategory_API(c.id);

            if (categoryTask.title == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(categoryTask);
            }

            // Update New Category
            categoryTask.title = c.title;
            categoryTask.description = c.description;

            // API requests
            bool result = await _categoryApi.UpdateCategory_API(categoryTask);

            if (result)
                return RedirectToAction("Category_List", "Category");
            else
            {
                ViewBag.Error = "Update Category Error";
                return View(categoryTask);
            }
        }


    }
}
