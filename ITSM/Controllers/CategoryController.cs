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
        private readonly Sucategory_api _sucategoryApi;
        private readonly Incident_Category_api _incidentcategoryApi;

        public CategoryController(IHttpContextAccessor httpContextAccessor, UserService userService)
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
            _sucategoryApi = new Sucategory_api(httpContextAccessor);
            _incidentcategoryApi = new Incident_Category_api(httpContextAccessor);
            _userService = userService;
        }

        public async Task<IActionResult> Category_List()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var categoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(categoryTask);

            var allCategory = categoryTask.Result;

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
            var currentUser = await _userService.GetCurrentUserAsync();

            var model = new AllModelVM
            {
                user = currentUser
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Category_Create(Category category)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var model = new AllModelVM
            {
                user = currentUser
            };

            if (category.title == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
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
                return View(model);
            }
        }

        public async Task<IActionResult> Category_Info(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            // Get Category
            var category = await _categoryApi.FindByIDCategory_API(id);

            var model = new AllModelVM
            {
                user = currentUser,
                category = category
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Category_Info(Category c)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            // Making concurrent API requests
            var categoryTask = await _categoryApi.FindByIDCategory_API(c.id);

            var model = new AllModelVM
            {
                user = currentUser,
                category = categoryTask
            };

            if (categoryTask.title == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
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
                return View(model);
            }
        }

        public async Task<IActionResult> Inc_Category_List()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var allIncidentCategory = await _incidentcategoryApi.GetAllIncidentcategory_API();

            var inccategoryList = allIncidentCategory.OrderByDescending(y => y.id).ToList();

            var model = new AllModelVM
            {
                user = currentUser,
                Incident_Category_List = inccategoryList
            };

            return View(model);
        }

        public async Task<IActionResult> Inc_Category_Create()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var model = new AllModelVM
            {
                user = currentUser
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Inc_Category_Create(Incidentcategory incCategory)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var model = new AllModelVM
            {
                user = currentUser
            };

            if (incCategory.name == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var allIncidentCategory = await _incidentcategoryApi.GetAllIncidentcategory_API();
            var sameData = allIncidentCategory.Where(x => x.name.ToLower() == incCategory.name.ToLower()).ToList();
            if(sameData.Count > 0)
            {
                ViewBag.Error = "This name already exist.";
                return View(model);
            }

            Incidentcategory new_inc_category = new Incidentcategory()
            {
                name = incCategory.name
            };

            bool result = await _incidentcategoryApi.CreateIncidentcategory_API(new_inc_category);

            if (result)
                return RedirectToAction("Inc_Category_List", "Category");
            else
            {
                ViewBag.Error = "Create Incident Category Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Inc_Category_Info(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var inccategory = await _incidentcategoryApi.FindByIDIncidentcategory_API(id);

            var model = new AllModelVM
            {
                user = currentUser,
                Incident_Category = inccategory
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Inc_Category_Info(Incidentcategory incCategory)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var inccategoryTask = await _incidentcategoryApi.FindByIDIncidentcategory_API(incCategory.id);

            var model = new AllModelVM
            {
                user = currentUser,
                Incident_Category = inccategoryTask
            };

            if (incCategory.name == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var allIncidentCategory = await _incidentcategoryApi.GetAllIncidentcategory_API();
            var sameData = allIncidentCategory.Where(x => x.name.ToLower() == incCategory.name.ToLower() && x.id != incCategory.id).ToList();
            if (sameData.Count > 0)
            {
                ViewBag.Error = "This name already exist.";
                return View(model);
            }

            inccategoryTask.name = incCategory.name;

            bool result = await _incidentcategoryApi.UpdateIncidentcategory_API(inccategoryTask);

            if (result)
                return RedirectToAction("Inc_Category_List", "Category");
            else
            {
                ViewBag.Error = "Update Incident Category Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Sucategory_List()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var categoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(categoryTask);

            var allCategory = categoryTask.Result;

            var categoryList = allCategory.OrderByDescending(y => y.id).ToList();

            var model = new AllModelVM
            {
                user = currentUser,
                CategoryList = categoryList
            };

            return View(model);
        }
    }
}
