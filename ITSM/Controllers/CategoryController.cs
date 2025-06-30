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
        private readonly Req_Category_api _reqcategoryApi;
        private readonly Req_Subcategory_api _reqsubcategoryApi;
        private readonly Req_Function_api _reqfunctionApi;

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
            _reqcategoryApi = new Req_Category_api(httpContextAccessor);
            _reqsubcategoryApi = new Req_Subcategory_api(httpContextAccessor);
            _reqfunctionApi = new Req_Function_api(httpContextAccessor);
            _userService = userService;
        }

        public async Task<IActionResult> Category_List()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var categoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(categoryTask);

            var allCategory = categoryTask.Result;

            var categoryList = allCategory.OrderByDescending(y => y.id).ToList();

            var model =  new AllModelVM
            {
                user = currentUser,
                CategoryList = categoryList,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        public async Task<IActionResult> Category_Create()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Category_Create(Category category)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
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
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var category = await _categoryApi.FindByIDCategory_API(id);

            var model = new AllModelVM
            {
                user = currentUser,
                category = category,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Category_Info(Category c)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var categoryTask = await _categoryApi.FindByIDCategory_API(c.id);

            var model = new AllModelVM
            {
                user = currentUser,
                category = categoryTask,
                incCount = incCount,
                reqCount = reqCount
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
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var allIncidentCategory = await _incidentcategoryApi.GetAllIncidentcategory_API();

            var inccategoryList = allIncidentCategory.OrderByDescending(y => y.id).ToList();

            var model = new AllModelVM
            {
                user = currentUser,
                Incident_Category_List = inccategoryList,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        public async Task<IActionResult> Inc_Category_Create()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Inc_Category_Create(Incidentcategory incCategory)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
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
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var inccategory = await _incidentcategoryApi.FindByIDIncidentcategory_API(id);

            var model = new AllModelVM
            {
                user = currentUser,
                Incident_Category = inccategory,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Inc_Category_Info(Incidentcategory incCategory)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var inccategoryTask = await _incidentcategoryApi.FindByIDIncidentcategory_API(incCategory.id);

            var model = new AllModelVM
            {
                user = currentUser,
                Incident_Category = inccategoryTask,
                incCount = incCount,
                reqCount = reqCount
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
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

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
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        public async Task<IActionResult> Subcategory_Create()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

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
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Subcategory_Create(Subcategory sub)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

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
                incCount = incCount,
                reqCount = reqCount
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
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

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
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Subcategory_Info(Subcategory sub)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

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
                incCount = incCount,
                reqCount = reqCount
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

        public async Task<IActionResult> Req_Category_List()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var allReqCategory = await _reqcategoryApi.GetAllReq_Category_API();

            var reqcategoryList = allReqCategory.OrderByDescending(y => y.id).ToList();

            var model = new AllModelVM
            {
                user = currentUser,
                reqCategoryList = reqcategoryList,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        public async Task<IActionResult> Req_Category_Create()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Req_Category_Create(Req_Category reqcategory)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
            };

            if (reqcategory.name == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var allRequestCategory = await _reqcategoryApi.GetAllReq_Category_API();
            var sameData = allRequestCategory.Where(x => x.name.ToLower() == reqcategory.name.ToLower() && x.erp_version == reqcategory.erp_version).ToList();
            if (sameData.Count > 0)
            {
                ViewBag.Error = "This category name already exist.";
                return View(model);
            }

            Req_Category new_req_category = new Req_Category()
            {
                name = reqcategory.name,
                erp_version = reqcategory.erp_version
            };

            bool result = await _reqcategoryApi.CreateReq_Category_API(new_req_category);

            if (result)
                return RedirectToAction("Req_Category_List", "Category");
            else
            {
                ViewBag.Error = "Create Request Category Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Req_Category_Info(int id)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var reqcategory = await _reqcategoryApi.FindByIDReq_Category_API(id);

            var model = new AllModelVM
            {
                user = currentUser,
                req_category = reqcategory,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Req_Category_Info(Req_Category reqcategory)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var reqcategoryTask = await _reqcategoryApi.FindByIDReq_Category_API(reqcategory.id);

            var model = new AllModelVM
            {
                user = currentUser,
                req_category = reqcategoryTask,
                incCount = incCount,
                reqCount = reqCount
            };

            if (reqcategory.name == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var allRequestCategory = await _reqcategoryApi.GetAllReq_Category_API();
            var sameData = allRequestCategory.Where(x => x.name?.ToLower() == reqcategory.name.ToLower() && x.id != reqcategory.id).ToList();
            if (sameData.Count > 0)
            {
                ViewBag.Error = "This name already exist.";
                return View(model);
            }

            reqcategoryTask.name = reqcategory.name;
            reqcategoryTask.erp_version = reqcategory.erp_version;

            bool result = await _reqcategoryApi.UpdateReq_Category_API(reqcategoryTask);

            if (result)
                return RedirectToAction("Req_Category_List", "Category");
            else
            {
                ViewBag.Error = "Update Request Category Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Req_Subcategory_List()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var allReqSubcategory = await _reqsubcategoryApi.GetAllReq_Subcategory_API();

            var reqSubcategoryList = allReqSubcategory.OrderByDescending(y => y.id).ToList();

            var reqcategory = await _reqcategoryApi.GetAllReq_Category_API();

            if(reqSubcategoryList.Count > 0)
            {
                foreach (var i in reqSubcategoryList)
                    i.Req_Category = reqcategory.FirstOrDefault(x => x.id == i.req_category_id);
            }

            var model = new AllModelVM
            {
                user = currentUser,
                reqSubCategoryList = reqSubcategoryList,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        public async Task<IActionResult> Req_Subcategory_Create()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var req_category = await _reqcategoryApi.GetAllReq_Category_API();

            var model = new AllModelVM
            {
                user = currentUser,
                reqCategoryList = req_category,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Req_Subcategory_Create(Req_Subcategory reqsubcategory)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
            };

            if (reqsubcategory.name == null && reqsubcategory.req_category_id != null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var allRequestSubcategory = await _reqsubcategoryApi.GetAllReq_Subcategory_API();
            var sameData = allRequestSubcategory.Where(x => x.name?.ToLower() == reqsubcategory.name?.ToLower() && x.req_category_id == reqsubcategory.req_category_id).ToList();
            if (sameData.Count > 0)
            {
                ViewBag.Error = "This category name already exist.";
                return View(model);
            }

            Req_Subcategory new_req_subcategory = new Req_Subcategory()
            {
                name = reqsubcategory.name,
                req_category_id = reqsubcategory.req_category_id
            };

            bool result = await _reqsubcategoryApi.CreateReq_Subcategory_API(new_req_subcategory);

            if (result)
                return RedirectToAction("Req_Subcategory_List", "Category");
            else
            {
                ViewBag.Error = "Create Request Subcategory Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Req_Subcategory_Info(int id)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var reqsubcategory = await _reqsubcategoryApi.FindByIDReq_Subcategory_API(id);

            var req_category = await _reqcategoryApi.GetAllReq_Category_API();

            reqsubcategory.Req_Category = req_category.FirstOrDefault(x => x.id == reqsubcategory.req_category_id);

            var model = new AllModelVM
            {
                user = currentUser,
                req_subcategory = reqsubcategory,
                reqCategoryList = req_category,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Req_Subcategory_Info(Req_Subcategory reqsubcategory)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var reqsubcategoryTask = await _reqsubcategoryApi.FindByIDReq_Subcategory_API(reqsubcategory.id);

            var req_category = await _reqcategoryApi.GetAllReq_Category_API();

            reqsubcategory.Req_Category = req_category.FirstOrDefault(x => x.id == reqsubcategory.req_category_id);

            var model = new AllModelVM
            {
                user = currentUser,
                req_subcategory = reqsubcategoryTask,
                reqCategoryList = req_category,
                incCount = incCount,
                reqCount = reqCount
            };

            if (reqsubcategory.name == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var allRequestSubcategory = await _reqsubcategoryApi.GetAllReq_Subcategory_API();
            var sameData = allRequestSubcategory.Where(x => x.name?.ToLower() == reqsubcategory.name.ToLower() && x.id != reqsubcategory.id).ToList();
            if (sameData.Count > 0)
            {
                ViewBag.Error = "This name already exist.";
                return View(model);
            }

            reqsubcategoryTask.name = reqsubcategory.name;
            reqsubcategoryTask.req_category_id = reqsubcategory.req_category_id;

            bool result = await _reqsubcategoryApi.UpdateReq_Subcategory_API(reqsubcategoryTask);

            if (result)
                return RedirectToAction("Req_Subcategory_List", "Category");
            else
            {
                ViewBag.Error = "Update Request Subcaategory Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Req_Function_List()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var allReqFunction = await _reqfunctionApi.GetAllReq_Function_API();

            var reqFunctionList = allReqFunction.OrderByDescending(y => y.id).ToList();

            var allReqSubcategoryTask = await _reqsubcategoryApi.GetAllReq_Subcategory_API();
            var allReqCategoryTask = await _reqcategoryApi.GetAllReq_Category_API();

            if(allReqFunction.Count > 0)
            {
                foreach (var i in allReqFunction)
                {
                    i.Req_Subcategory = allReqSubcategoryTask.FirstOrDefault(x => x.id == i.req_subcategory_id);
                    if (i.Req_Subcategory != null)
                        i.Req_Subcategory.Req_Category = allReqCategoryTask.FirstOrDefault(x => x.id == i.Req_Subcategory.req_category_id);
                }
            }

            var model = new AllModelVM
            {
                user = currentUser,
                reqFunctionList = reqFunctionList,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        public async Task<IActionResult> Req_Function_Create()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var req_subcategory = await _reqsubcategoryApi.GetAllReq_Subcategory_API();
            var allReqCategory = await _reqcategoryApi.GetAllReq_Category_API();

            if(req_subcategory.Count > 0)
            {
                foreach (var i in req_subcategory)
                    i.Req_Category = allReqCategory.FirstOrDefault(x => x.id == i.req_category_id);
            }

            var model = new AllModelVM
            {
                user = currentUser,
                reqSubCategoryList = req_subcategory,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Req_Function_Create(Req_Function reqfunction)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var req_subcategory = await _reqsubcategoryApi.GetAllReq_Subcategory_API();
            var allReqCategory = await _reqcategoryApi.GetAllReq_Category_API();

            if (req_subcategory.Count > 0)
            {
                foreach (var i in req_subcategory)
                    i.Req_Category = allReqCategory.FirstOrDefault(x => x.id == i.req_category_id);
            }

            var model = new AllModelVM
            {
                user = currentUser,
                reqSubCategoryList = req_subcategory,
                incCount = incCount,
                reqCount = reqCount
            };

            if (reqfunction.name == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var allReqFunction = await _reqfunctionApi.GetAllReq_Function_API();
            var sameData = allReqFunction.Where(x => x.name?.ToLower() == reqfunction.name?.ToLower() && x.req_subcategory_id == reqfunction.req_subcategory_id).ToList();
            if (sameData.Count > 0)
            {
                ViewBag.Error = "This category name already exist.";
                return View(model);
            }

            Req_Function new_req_function = new Req_Function()
            {
                name = reqfunction.name,
                req_subcategory_id = reqfunction.req_subcategory_id
            };

            bool result = await _reqfunctionApi.CreateReq_Function_API(new_req_function);

            if (result)
                return RedirectToAction("Req_Function_List", "Category");
            else
            {
                ViewBag.Error = "Create Request Function Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Req_Function_Info(int id)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var reqfunction = await _reqfunctionApi.FindByIDReq_Function_API(id);

            var req_subcategory = await _reqsubcategoryApi.GetAllReq_Subcategory_API();

            var req_category = await _reqcategoryApi.GetAllReq_Category_API();

            reqfunction.Req_Subcategory = req_subcategory.FirstOrDefault(x => x.id == reqfunction.req_subcategory_id);
            if(reqfunction.Req_Subcategory != null)
            {
                reqfunction.Req_Subcategory.Req_Category = req_category.FirstOrDefault(x => x.id == reqfunction.Req_Subcategory.req_category_id);
            }

            var model = new AllModelVM
            {
                user = currentUser,
                req_function = reqfunction,
                reqSubCategoryList = req_subcategory,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Req_Function_Info(Req_Function reqfunctionpost)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var reqfunction = await _reqfunctionApi.FindByIDReq_Function_API(reqfunctionpost.id);

            var req_subcategory = await _reqsubcategoryApi.GetAllReq_Subcategory_API();

            var req_category = await _reqcategoryApi.GetAllReq_Category_API();

            reqfunction.Req_Subcategory = req_subcategory.FirstOrDefault(x => x.id == reqfunction.req_subcategory_id);
            if (reqfunction.Req_Subcategory != null)
            {
                reqfunction.Req_Subcategory.Req_Category = req_category.FirstOrDefault(x => x.id == reqfunction.Req_Subcategory.req_category_id);
            }

            var model = new AllModelVM
            {
                user = currentUser,
                req_function = reqfunction,
                reqSubCategoryList = req_subcategory,
                incCount = incCount,
                reqCount = reqCount
            };

            if (reqfunctionpost.name == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var allRequestFunction = await _reqfunctionApi.GetAllReq_Function_API();
            var sameData = allRequestFunction.Where(x => x.name?.ToLower() == reqfunctionpost.name.ToLower() && x.id != reqfunctionpost.id).ToList();
            if (sameData.Count > 0)
            {
                ViewBag.Error = "This name already exist.";
                return View(model);
            }

            reqfunction.name = reqfunctionpost.name;
            reqfunction.req_subcategory_id = reqfunctionpost.req_subcategory_id;

            bool result = await _reqfunctionApi.UpdateReq_Function_API(reqfunction);

            if (result)
                return RedirectToAction("Req_Function_List", "Category");
            else
            {
                ViewBag.Error = "Update Request Function Error";
                return View(model);
            }
        }
    }
}
