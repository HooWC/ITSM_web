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

        public RequestController(IHttpContextAccessor httpContextAccessor)
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

        public async Task<IActionResult> All()
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var productTask = _productApi.GetAllProduct_API();
            var userTask = _userApi.GetAllUser_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            var requestTask = _reqApi.GetAllRequest_API();
            await Task.WhenAll(productTask, userTask, departmentTask, requestTask);

            var allProduct = await productTask;
            var allUser = await userTask;
            var allDepartment = await departmentTask;
            var allRequest = await requestTask;

            var Reqs = allRequest.OrderByDescending(x => x.id).ToList();
            foreach(var i in Reqs)
            {
                i.Product = allProduct.FirstOrDefault(x => x.id == i.pro_id);
                i.Sender = allUser.FirstOrDefault(x => x.id == i.sender);
                i.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == i.assignment_group);
                i.UpdatedBy = allUser.FirstOrDefault(x => x.id == i.updated_by);
            }

            var model = new AllModelVM()
            {
                user = currentUser,
                RequestList = Reqs,
            };

            return View(model);
        }

        public async Task<IActionResult> User_All()
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var productTask = _productApi.GetAllProduct_API();
            var userTask = _userApi.GetAllUser_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            var requestTask = _reqApi.GetAllRequest_API();
            await Task.WhenAll(productTask, userTask, departmentTask, requestTask);

            var allProduct = await productTask;
            var allUser = await userTask;
            var allDepartment = await departmentTask;
            var allRequest = await requestTask;

            var Reqs = allRequest.Where(x => x.sender == currentUser.id).OrderByDescending(x => x.id).ToList();
            foreach (var i in Reqs)
            {
                i.Product = allProduct.FirstOrDefault(x => x.id == i.pro_id);
                i.Sender = allUser.FirstOrDefault(x => x.id == i.sender);
                i.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == i.assignment_group);
                i.UpdatedBy = allUser.FirstOrDefault(x => x.id == i.updated_by);
            }

            var model = new AllModelVM()
            {
                user = currentUser,
                RequestList = Reqs,
            };

            return View(model);
        }

        public async Task<IActionResult> Service_Catalog()
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var depTask = _depApi.GetAllDepartment_API();
            var categoryTask = _categoryApi.GetAllCategory_API();
            var productTask = _productApi.GetAllProduct_API();
            await Task.WhenAll(depTask, categoryTask, productTask);

            var allDep = await depTask;
            var allCategory = await categoryTask;
            var allProduct = await productTask;

            foreach(var i in allProduct)
            {
                i.Category = allCategory.FirstOrDefault(x => x.id == i.category_id);
                i.ResponsibleDepartment = allDep.FirstOrDefault(x => x.id == i.responsible);
            }

            var model = new AllModelVM()
            {
                user = currentUser,
                ProductList = allProduct,
                CategoryList = allCategory
            };

            return View(model);
        }

        public async Task<IActionResult> Create_Form(int id)
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var depTask = _depApi.GetAllDepartment_API();
            var categoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(depTask, categoryTask);

            var allDep = await depTask;
            var allCategory = await categoryTask;

            var info_pro = await _productApi.FindByIDProduct_API(id);
            info_pro.Category = allCategory.Where(x => x.id ==  info_pro.category_id).FirstOrDefault();
            info_pro.ResponsibleDepartment = allDep.Where(x => x.id == info_pro.responsible).FirstOrDefault();

            var model = new AllModelVM()
            {
                user = currentUser,
                product = info_pro
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create_Form(int pro_id, Request req)
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var depTask = _depApi.GetAllDepartment_API();
            var categoryTask = _categoryApi.GetAllCategory_API();
            var reqTask = _reqApi.GetAllRequest_API();
            var roleTask = _roleApi.GetAllRole_API();
            await Task.WhenAll(depTask, categoryTask, reqTask, roleTask);

            var allDep = await depTask;
            var allCategory = await categoryTask;
            var allReq = await reqTask;
            var allRole = await roleTask;

            currentUser.Role = allRole.Where(x => x.id == currentUser.role_id).FirstOrDefault();

            var info_pro = await _productApi.FindByIDProduct_API(pro_id);

            info_pro.Category = allCategory.Where(x => x.id == info_pro.category_id).FirstOrDefault();
            info_pro.ResponsibleDepartment = allDep.Where(x => x.id == info_pro.responsible).FirstOrDefault();

            var model = new AllModelVM()
            {
                user = currentUser,
                product = info_pro
            };

            if (info_pro.product_type == "Product")
            {
                if(req.quantity > info_pro.quantity || req.quantity <= 0)
                {
                    ViewBag.Error = "Error Quantity";
                    return View(model);
                }

                info_pro.quantity = info_pro.quantity - req.quantity;
                if (info_pro.quantity <= 0)
                    info_pro.active = false;

                await _productApi.UpdateProduct_API(info_pro);
            }

            if (req.short_description == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

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
                short_description = req.short_description,
                description = req.short_description,
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
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var productTask = _productApi.GetAllProduct_API();
            var userTask = _userApi.GetAllUser_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            var requestTask = _reqApi.GetAllRequest_API();
            await Task.WhenAll(productTask, userTask, departmentTask, requestTask);

            var allProduct = await productTask;
            var allUser = await userTask;
            var allDepartment = await departmentTask;
            var allRequest = await requestTask;

            var Reqs = allRequest.Where(x => x.assignment_group == currentUser.department_id).OrderByDescending(x => x.id).ToList();
            foreach (var i in Reqs)
            {
                i.Product = allProduct.FirstOrDefault(x => x.id == i.pro_id);
                i.Sender = allUser.FirstOrDefault(x => x.id == i.sender);
                i.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == i.assignment_group);
                i.UpdatedBy = allUser.FirstOrDefault(x => x.id == i.updated_by);
            }

            var model = new AllModelVM()
            {
                user = currentUser,
                RequestList = Reqs,
            };

            return View(model);
        }

        public async Task<IActionResult> Req_Info(int id, string role)
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            ViewBag.roleBack = role;

            var productTask = _productApi.GetAllProduct_API();
            var userTask = _userApi.GetAllUser_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            var categoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(productTask, userTask, departmentTask, categoryTask);

            var allProduct = await productTask;
            var allUser = await userTask;
            var allDepartment = await departmentTask;
            var allCategory = await categoryTask;

            var Req = await _reqApi.FindByIDRequest_API(id);
            Req.Product = allProduct.FirstOrDefault(x => x.id == Req.pro_id);
            Req.Sender = allUser.FirstOrDefault(x => x.id == Req.sender);
            Req.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == Req.assignment_group);
            Req.UpdatedBy = allUser.FirstOrDefault(x => x.id == Req.updated_by);
            Req.Product.Category = allCategory.FirstOrDefault(x => x.id == Req.Product.category_id);
            Req.Product.ResponsibleDepartment = allDepartment.FirstOrDefault(x => x.id == Req.Product.responsible);

            var model = new AllModelVM()
            {
                user = currentUser,
                request = Req,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Req_Info(Request req, string roleBack)
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var productTask = _productApi.GetAllProduct_API();
            var userTask = _userApi.GetAllUser_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            var categoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(productTask, userTask, departmentTask, categoryTask);

            var allProduct = await productTask;
            var allUser = await userTask;
            var allDepartment = await departmentTask;
            var allCategory = await categoryTask;

            var Req = await _reqApi.FindByIDRequest_API(req.id);
            Req.Product = allProduct.FirstOrDefault(x => x.id == Req.pro_id);
            Req.Sender = allUser.FirstOrDefault(x => x.id == Req.sender);
            Req.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == Req.assignment_group);
            Req.UpdatedBy = allUser.FirstOrDefault(x => x.id == Req.updated_by);
            Req.Product.Category = allCategory.FirstOrDefault(x => x.id == Req.Product.category_id);
            Req.Product.ResponsibleDepartment = allDepartment.FirstOrDefault(x => x.id == Req.Product.responsible);

            var model = new AllModelVM()
            {
                user = currentUser,
                request = Req,
            };

            if (req.short_description == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var info_pro = await _productApi.FindByIDProduct_API(req.pro_id);

            int root = info_pro.quantity + Req.quantity;

            if (info_pro.product_type == "Product")
            {
                if (req.quantity > root || req.quantity <= 0)
                {
                    ViewBag.Error = "Error Quantity";
                    return View(model);
                }

                info_pro.quantity = root - req.quantity;
                if (info_pro.quantity <= 0)
                    info_pro.active = false;

                await _productApi.UpdateProduct_API(info_pro);
            }

            Req.short_description = req.short_description;
            Req.description = req.short_description;
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
    }
}
