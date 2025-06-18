using System.Runtime.Intrinsics.Arm;
using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class RequestController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserService _userService;
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

        public RequestController(IHttpContextAccessor httpContextAccessor, UserService userService)
        {
            _userService = userService;
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

        public async Task<AllModelVM> get_req_data(string type)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var productTask = _productApi.GetAllProduct_API();
            var userTask = _userApi.GetAllUser_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            var requestTask = _reqApi.GetAllRequest_API();
            await Task.WhenAll(productTask, userTask, departmentTask, requestTask);

            var allProduct = productTask.Result;
            var allUser = userTask.Result;
            var allDepartment = departmentTask.Result;
            var allRequest = requestTask.Result;

            var Reqs = new List<Request>();
            if(type.Contains("All"))
                Reqs = allRequest.OrderByDescending(x => x.id).ToList();
            else if(type.Contains("User_All"))
                Reqs = allRequest.Where(x => x.sender == currentUser.id).OrderByDescending(x => x.id).ToList();
            else if(type.Contains("Assigned_To_Us"))
                Reqs = allRequest.Where(x => x.assignment_group == currentUser.department_id).OrderByDescending(x => x.id).ToList();
            else if (type.Contains("Manager_Assign_Work"))
                Reqs = allRequest.Where(x => x.assignment_group == currentUser.department_id && x?.assigned_to == null && x.state != "Rejected" && x.state != "Completed").OrderByDescending(x => x.id).ToList();
            else if (type == "Assigned_To_Me")
                Reqs = allRequest.Where(x => x.assigned_to == currentUser.id && x.state != "Resolved" && x.state != "Closed").OrderByDescending(y => y.id).ToList();

            foreach (var i in Reqs)
            {
                i.Product = allProduct.FirstOrDefault(x => x.id == i.pro_id);
                i.Sender = allUser.FirstOrDefault(x => x.id == i.sender);
                i.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == i.assignment_group);
                i.UpdatedBy = allUser.FirstOrDefault(x => x.id == i.updated_by);
                i.AssignedTo = allUser.FirstOrDefault(x => x.id == i.assigned_to);
            }

            var model = new AllModelVM()
            {
                user = currentUser,
                RequestList = Reqs,
                noteMessageCount = noteMessageCount
            };

            return model;
        }

        public async Task<IActionResult> All()
        {
            var model = await get_req_data("User_All");

            return View(model);
        }

        public async Task<IActionResult> User_All()
        {
            var model = await get_req_data("User_All");

            return View(model);
        }

        public async Task<IActionResult> Service_Catalog()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var depTask = _depApi.GetAllDepartment_API();
            var categoryTask = _categoryApi.GetAllCategory_API();
            var productTask = _productApi.GetAllProduct_API();
            await Task.WhenAll(depTask, categoryTask, productTask);

            var allDep = depTask.Result;
            var allCategory = categoryTask.Result;
            var allProduct = productTask.Result;

            foreach(var i in allProduct)
            {
                i.ResponsibleDepartment = allDep.FirstOrDefault(x => x.id == i.responsible);
            }

            var model = new AllModelVM()
            {
                user = currentUser,
                ProductList = allProduct,
                CategoryList = allCategory,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        public async Task<IActionResult> Create_Form(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var depTask = _depApi.GetAllDepartment_API();
            var categoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(depTask, categoryTask);

            var allDep = depTask.Result;
            var allCategory = categoryTask.Result;

            var info_pro = await _productApi.FindByIDProduct_API(id);
            info_pro.ResponsibleDepartment = allDep.Where(x => x.id == info_pro.responsible).FirstOrDefault();

            var model = new AllModelVM()
            {
                user = currentUser,
                product = info_pro,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create_Form(int pro_id, Request req)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var depTask = _depApi.GetAllDepartment_API();
            var categoryTask = _categoryApi.GetAllCategory_API();
            var reqTask = _reqApi.GetAllRequest_API();
            var roleTask = _roleApi.GetAllRole_API();
            await Task.WhenAll(depTask, categoryTask, reqTask, roleTask);

            var allDep = depTask.Result;
            var allCategory = categoryTask.Result;
            var allReq = reqTask.Result;
            var allRole = roleTask.Result;

            currentUser.Role = allRole.Where(x => x.id == currentUser.role_id).FirstOrDefault();

            var info_pro = await _productApi.FindByIDProduct_API(pro_id);

            info_pro.ResponsibleDepartment = allDep.Where(x => x.id == info_pro.responsible).FirstOrDefault();

            var model = new AllModelVM()
            {
                user = currentUser,
                product = info_pro,
                noteMessageCount = noteMessageCount
            };

            if (req.description == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            if (req.quantity > info_pro.quantity || req.quantity <= 0)
            {
                ViewBag.Error = "Error Quantity";
                return View(model);
            }

            info_pro.quantity = info_pro.quantity - req.quantity;
            if (info_pro.quantity <= 0)
                info_pro.active = false;

            await _productApi.UpdateProduct_API(info_pro);

            string newId = "";
            if (allReq.Count > 0)
            {
                var last_pro = allReq.Last();
                string p_id_up = last_pro.req_id;
                string prefix = new string(p_id_up.TakeWhile(char.IsLetter).ToArray());
                string numberPart = new string(p_id_up.SkipWhile(char.IsLetter).ToArray());
                int number = int.Parse(numberPart);
                newId = prefix + (number + 1);
            }
            else
                newId = "REQ1";

            var new_req = new Request()
            {
                req_id = newId,
                pro_id = pro_id,
                sender = currentUser.id,
                state = "Pedding",
                description = req.description,
                assignment_group = info_pro.responsible,
                quantity = req.quantity,
                updated_by = currentUser.id
            };

            bool result = await _reqApi.CreateRequest_API(new_req);

            if (result)
            {
                return RedirectToAction("User_All", "Request");
            }
            else
            {
                ViewBag.Error = "Create Request Error";
                return View(model);
            }
            
        }

        public async Task<IActionResult> Assigned_To_Us()
        {
            var model = await get_req_data("User_All");

            return View(model);
        }

        public async Task<IActionResult> Req_Info(int id, string type)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var productTask = _productApi.GetAllProduct_API();
            var userTask = _userApi.GetAllUser_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            var categoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(productTask, userTask, departmentTask, categoryTask);

            var allProduct = productTask.Result;
            var allUser = userTask.Result;
            var allDepartment = departmentTask.Result;
            var allCategory = categoryTask.Result;

            var Req = await _reqApi.FindByIDRequest_API(id);
            Req.Product = allProduct.FirstOrDefault(x => x.id == Req.pro_id);
            Req.Sender = allUser.FirstOrDefault(x => x.id == Req.sender);
            Req.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == Req.assignment_group);
            Req.UpdatedBy = allUser.FirstOrDefault(x => x.id == Req.updated_by);
            Req.Product.ResponsibleDepartment = allDepartment.FirstOrDefault(x => x.id == Req.Product.responsible);

            var model = new AllModelVM()
            {
                user = currentUser,
                request = Req,
                roleBack = type,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Req_Info(Request req, string roleBack)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var productTask = _productApi.GetAllProduct_API();
            var userTask = _userApi.GetAllUser_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            var categoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(productTask, userTask, departmentTask, categoryTask);

            var allProduct = productTask.Result;
            var allUser = userTask.Result;
            var allDepartment = departmentTask.Result;
            var allCategory = categoryTask.Result;

            var Req = await _reqApi.FindByIDRequest_API(req.id);
            Req.Product = allProduct.FirstOrDefault(x => x.id == Req.pro_id);
            Req.Sender = allUser.FirstOrDefault(x => x.id == Req.sender);
            Req.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == Req.assignment_group);
            Req.UpdatedBy = allUser.FirstOrDefault(x => x.id == Req.updated_by);
            Req.Product.ResponsibleDepartment = allDepartment.FirstOrDefault(x => x.id == Req.Product.responsible);

            var model = new AllModelVM()
            {
                user = currentUser,
                request = Req,
                roleBack = roleBack,
                noteMessageCount = noteMessageCount
            };

            if (req.description == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var info_pro = await _productApi.FindByIDProduct_API(req.pro_id);

            int root = info_pro.quantity + Req.quantity;

            if (req.quantity > root || req.quantity <= 0)
            {
                ViewBag.Error = "Error Quantity";
                return View(model);
            }

            info_pro.quantity = root - req.quantity;
            if (info_pro.quantity <= 0)
                info_pro.active = false;
            else
                info_pro.active = true;

            await _productApi.UpdateProduct_API(info_pro);
            
            Req.description = req.description;
            Req.state = req.state;
            Req.updated_by = currentUser.id;
            Req.quantity = req.quantity;

            bool result = await _reqApi.UpdateRequest_API(Req);

            if (result)
            {
                if(roleBack == "Admin")
                    return RedirectToAction("All", "Request");
                else if(roleBack == "Group")
                    return RedirectToAction("Assigned_To_Us", "Request");
                else
                    return RedirectToAction("User_All", "Request");
            }
            else
            {
                ViewBag.Error = "Update Request Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Manager_Assign_Work()
        {
            var model = await get_req_data("Manager_Assign_Work");

            return View(model);
        }

        public async Task<AllModelVM> get_Manager_Assign_Work_Info(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var productTask = _productApi.GetAllProduct_API();
            var userTask = _userApi.GetAllUser_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            var requestTask = _reqApi.GetAllRequest_API();
            await Task.WhenAll(productTask, userTask, departmentTask, requestTask);

            var allProduct = productTask.Result;
            var allUser = userTask.Result;
            var allDepartment = departmentTask.Result;
            var allRequest = requestTask.Result;

            var req_info = await _reqApi.FindByIDRequest_API(id);
            req_info.Product = allProduct.FirstOrDefault(x => x.id == req_info.pro_id);
            req_info.Sender = allUser.FirstOrDefault(x => x.id == req_info.sender);
            req_info.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == req_info.assignment_group);
            req_info.UpdatedBy = allUser.FirstOrDefault(x => x.id == req_info.updated_by);
            req_info.AssignedTo = allUser.FirstOrDefault(x => x.id == req_info.assigned_to);

            var info_pro = await _productApi.FindByIDProduct_API(req_info.pro_id);
            req_info.Product.ResponsibleDepartment = allDepartment.FirstOrDefault(x => x.id == info_pro.responsible);

            var model = new AllModelVM()
            {
                user = currentUser,
                request = req_info,
                noteMessageCount = noteMessageCount
            };

            return model;
        }

        public async Task<IActionResult> Manager_Assign_Work_Info(int id)
        {
            var model = await get_Manager_Assign_Work_Info(id);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Manager_Assign_Work_Info(Request req)
        {
            var model = await get_Manager_Assign_Work_Info(req.id);

            var reqData = await _reqApi.FindByIDRequest_API(req.id);

            if (req.description != null)
            {
                if (reqData != null)
                {
                    reqData.assigned_to = reqData.assigned_to;

                    bool result = await _reqApi.UpdateRequest_API(reqData);

                    if (result)
                        return RedirectToAction("Manager_Assign_Work", "Request");
                    else
                    {
                        ViewBag.Error = "Update event failed";
                        return View(model);
                    }
                }
                else
                {
                    ViewBag.Error = "Update Request Error";
                    return View(model);
                }
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }
        }

        public async Task<IActionResult> Assigned_To_Me()
        {
            var model = await get_req_data("Assigned_To_Me");

            return View(model);
        }
    }
}
