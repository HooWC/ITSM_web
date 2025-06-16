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
        private readonly Subcategory_api _subcategoryApi;
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
            _subcategoryApi = new Subcategory_api(httpContextAccessor);
            _incidentcategoryApi = new Incident_Category_api(httpContextAccessor);
            _userService = userService;
        }

        public async Task<IActionResult> Category_List()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var categoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(categoryTask);

            var allCategory = categoryTask.Result;

            var categoryList = allCategory.OrderByDescending(y => y.id).ToList();

            var model =  new AllModelVM
            {
                user = currentUser,
                CategoryList = categoryList,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        public async Task<IActionResult> Category_Create()
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
        public async Task<IActionResult> Category_Create(Category category)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var model = new AllModelVM
            {
                user = currentUser,
                noteMessageCount = noteMessageCount
            };

            if (category.title == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            Category new_category = new Category()
            {
                title = category.title,
                description = category.description
            };

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
            var noteMessageCount = await _userService.GetNoteAsync();

            var category = await _categoryApi.FindByIDCategory_API(id);

            var model = new AllModelVM
            {
                user = currentUser,
                category = category,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Category_Info(Category c)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var categoryTask = await _categoryApi.FindByIDCategory_API(c.id);

            var model = new AllModelVM
            {
                user = currentUser,
                category = categoryTask,
                noteMessageCount = noteMessageCount
            };

            if (categoryTask.title == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            categoryTask.title = c.title;
            categoryTask.description = c.description;

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
            var noteMessageCount = await _userService.GetNoteAsync();

            var allIncidentCategory = await _incidentcategoryApi.GetAllIncidentcategory_API();

            var inccategoryList = allIncidentCategory.OrderByDescending(y => y.id).ToList();

            var model = new AllModelVM
            {
                user = currentUser,
                Incident_Category_List = inccategoryList,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        public async Task<IActionResult> Inc_Category_Create()
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
        public async Task<IActionResult> Inc_Category_Create(Incidentcategory incCategory)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var model = new AllModelVM
            {
                user = currentUser,
                noteMessageCount = noteMessageCount
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
                ViewBag.Error = "This category name already exist.";
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
            var noteMessageCount = await _userService.GetNoteAsync();

            var inccategory = await _incidentcategoryApi.FindByIDIncidentcategory_API(id);

            var model = new AllModelVM
            {
                user = currentUser,
                Incident_Category = inccategory,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Inc_Category_Info(Incidentcategory incCategory)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var inccategoryTask = await _incidentcategoryApi.FindByIDIncidentcategory_API(incCategory.id);

            var model = new AllModelVM
            {
                user = currentUser,
                Incident_Category = inccategoryTask,
                noteMessageCount = noteMessageCount
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

        public async Task<IActionResult> Subcategory_List()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var subcategoryTask = _subcategoryApi.GetAllSubcategory_API();
            var departmentTask = _depApi.GetAllDepartment_API();
            var inccategoryTask = _incidentcategoryApi.GetAllIncidentcategory_API();
            await Task.WhenAll(subcategoryTask, departmentTask, inccategoryTask);

            var allSubcategory = subcategoryTask.Result;
            var allDepartment = departmentTask.Result;
            var allIncCategory = inccategoryTask.Result;

            var subcategoryList = allSubcategory.OrderByDescending(y => y.id).ToList();

            foreach(var i in subcategoryList)
            {
                i.Department = allDepartment.FirstOrDefault(x => x.id == i.department_id);
                i.Incidentcategory = allIncCategory.FirstOrDefault(x => x.id == i.category);
            }

            var model = new AllModelVM
            {
                user = currentUser,
                Subcategory_List = subcategoryList,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        public async Task<IActionResult> Subcategory_Create()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var departmentTask = _depApi.GetAllDepartment_API();
            var inccategoryTask = _incidentcategoryApi.GetAllIncidentcategory_API();
            await Task.WhenAll(departmentTask, inccategoryTask);

            var allDepartment = departmentTask.Result;
            var allIncCategory = inccategoryTask.Result;

            var model = new AllModelVM
            {
                user = currentUser,
                DepartmentList = allDepartment,
                Incident_Category_List = allIncCategory,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Subcategory_Create(Subcategory sub)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var departmentTask = _depApi.GetAllDepartment_API();
            var inccategoryTask = _incidentcategoryApi.GetAllIncidentcategory_API();
            await Task.WhenAll(departmentTask, inccategoryTask);

            var allDepartment = departmentTask.Result;
            var allIncCategory = inccategoryTask.Result;

            var model = new AllModelVM
            {
                user = currentUser,
                DepartmentList = allDepartment,
                Incident_Category_List = allIncCategory,
                noteMessageCount = noteMessageCount
            };

            if (sub.subcategory == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var allSubcategory = await _subcategoryApi.GetAllSubcategory_API();
            var sameData = allSubcategory.Where(x => x.subcategory.ToLower() == sub.subcategory.ToLower() && x.category != sub.category).ToList();
            if (sameData.Count > 0)
            {
                ViewBag.Error = "This subcategory name already exist.";
                return View(model);
            }

            Subcategory new_subcategory = new Subcategory()
            {
                subcategory = sub.subcategory,
                category = sub.category,
                department_id = sub.department_id
            };

            bool result = await _subcategoryApi.CreateSubcategory_API(new_subcategory);

            if (result)
                return RedirectToAction("Subcategory_List", "Category");
            else
            {
                ViewBag.Error = "Create Subcategory Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Subcategory_Info(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var subcategory = await _subcategoryApi.FindByIDSubcategory_API(id);
            var departmentTask = _depApi.GetAllDepartment_API();
            var inccategoryTask = _incidentcategoryApi.GetAllIncidentcategory_API();
            await Task.WhenAll(departmentTask, inccategoryTask);

            var allDepartment = departmentTask.Result;
            var allIncCategory = inccategoryTask.Result;

            var model = new AllModelVM
            {
                user = currentUser,
                Sub_Category = subcategory,
                DepartmentList = allDepartment,
                Incident_Category_List = allIncCategory,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Subcategory_Info(Subcategory sub)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var subcategoryTask = await _subcategoryApi.FindByIDSubcategory_API(sub.id);
            var departmentTask = _depApi.GetAllDepartment_API();
            var inccategoryTask = _incidentcategoryApi.GetAllIncidentcategory_API();
            await Task.WhenAll(departmentTask, inccategoryTask);

            var allDepartment = departmentTask.Result;
            var allIncCategory = inccategoryTask.Result;

            var model = new AllModelVM
            {
                user = currentUser,
                Sub_Category = subcategoryTask,
                DepartmentList = allDepartment,
                Incident_Category_List = allIncCategory,
                noteMessageCount = noteMessageCount
            };

            if (sub.subcategory == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var allSubcategory = await _subcategoryApi.GetAllSubcategory_API();
            var sameData = allSubcategory.Where(x => x.subcategory.ToLower() == sub.subcategory.ToLower() && x.id != sub.id && x.category != sub.category).ToList();
            if (sameData.Count > 0)
            {
                ViewBag.Error = "This subcategory name already exist.";
                return View(model);
            }

            subcategoryTask.subcategory = sub.subcategory;
            subcategoryTask.category = sub.category;
            subcategoryTask.department_id = sub.department_id;

            bool result = await _subcategoryApi.UpdateSubcategory_API(subcategoryTask);

            if (result)
                return RedirectToAction("Subcategory_List", "Category");
            else
            {
                ViewBag.Error = "Update Subcategory Error";
                return View(model);
            }
        }
    }
}
