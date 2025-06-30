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
        private readonly Req_Category_api _reqcategoryApi;
        private readonly Req_Subcategory_api _reqsubcategoryApi;
        private readonly Req_Function_api _reqfunctionApi;
        private readonly RequestPhotos_api _reqphoneApi;

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
            _reqcategoryApi = new Req_Category_api(httpContextAccessor);
            _reqsubcategoryApi = new Req_Subcategory_api(httpContextAccessor);
            _reqfunctionApi = new Req_Function_api(httpContextAccessor);
            _reqphoneApi = new RequestPhotos_api(httpContextAccessor);
        }

        public async Task<AllModelVM> get_req_data(string type)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

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
            if(type == "All")
            {
                if(currentUser.Role?.role.ToLower() != "admin" && currentUser.r_manager == true)
                {
                    Reqs = allRequest.Where(x => x.assignment_group == currentUser.department_id).OrderByDescending(x => x.id).ToList();
                }
                else
                {
                    Reqs = allRequest.OrderByDescending(x => x.id).ToList();
                }
            }
            else if(type == "User_All")
                Reqs = allRequest.Where(x => x.sender == currentUser.id).OrderByDescending(x => x.id).ToList();
            else if(type == "Assigned_To_Us")
                Reqs = allRequest.Where(x => x.assignment_group == currentUser.department_id).OrderByDescending(x => x.id).ToList();
            else if (type == "Manager_Assign_Work")
                Reqs = allRequest.Where(x => x.assignment_group == currentUser.department_id && x?.assigned_to == null && x.state != "Rejected" && x.state != "Completed").OrderByDescending(x => x.id).ToList();
            else if (type == "Assigned_To_Me")
                Reqs = allRequest.Where(x => x.assigned_to == currentUser.id).OrderByDescending(y => y.id).ToList();

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
                incCount = incCount,
                reqCount = reqCount
            };

            return model;
        }

        public async Task<IActionResult> All()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var model = await get_req_data("All");

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
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

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
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        public async Task<IActionResult> Create_Form(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

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
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create_Form(int pro_id, Request req)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

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
                product = info_pro
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

            info_pro.quantity = info_pro.quantity - (req.quantity ?? 0);
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
                state = "Pending",
                description = req.description,
                assignment_group = info_pro.responsible,
                quantity = req.quantity,
                updated_by = currentUser.id,
                req_type = false,
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

        public async Task<AllModelVM> get_Req_Info_Data(int id,string type)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

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
            Req.Sender = allUser.FirstOrDefault(x => x.id == Req.sender);
            Req.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == Req.assignment_group);
            Req.UpdatedBy = allUser.FirstOrDefault(x => x.id == Req.updated_by);
            Req.AssignedTo = allUser.FirstOrDefault(x => x.id == Req.assigned_to);

            var req_category_list = new List<Req_Category>();
            var ReqPhotoDataList = new List<RequestPhoto>();

            if (Req.req_type == true)
            {
                var reqcategoryTask = _reqcategoryApi.GetAllReq_Category_API();
                var reqsubcategoryTask = _reqsubcategoryApi.GetAllReq_Subcategory_API();
                var reqfunctionTask = _reqfunctionApi.GetAllReq_Function_API();
                var reqphotoTask = _reqphoneApi.GetAllRequestPhoto_API();
                await Task.WhenAll(reqcategoryTask, reqsubcategoryTask, reqfunctionTask, reqphotoTask);

                var allReqCategory = reqcategoryTask.Result;
                var allReqSubcategory = reqsubcategoryTask.Result;
                var allReqFunction = reqfunctionTask.Result;
                var allReqPhoto = reqphotoTask.Result;

                if (Req.req_type == true && Req.erp_version.ToLower() == "erp 8")
                    req_category_list = allReqCategory.Where(x => x.erp_version?.ToLower() == "erp 8").ToList();
                else if (Req.req_type == true && Req.erp_version.ToLower() == "erp 9")
                    req_category_list = allReqCategory.Where(x => x.erp_version?.ToLower() == "erp 9").ToList();
                else
                    req_category_list = new List<Req_Category>();

                Req.ERPCategory = allReqCategory.FirstOrDefault(x => x.id == Req.erp_category);
                Req.ERPSubcategory = allReqSubcategory.FirstOrDefault(x => x.id == Req.erp_subcategory);
                Req.ERPFunction = allReqFunction.FirstOrDefault(x => x.id == Req.erp_function);

                ReqPhotoDataList = allReqPhoto.Where(x => x.request_id == Req.id).ToList();

                if (ReqPhotoDataList.Count == 0 || ReqPhotoDataList == null)
                    ReqPhotoDataList = new List<RequestPhoto>();
            }
            else
            {
                var info_pro = await _productApi.FindByIDProduct_API(Req.pro_id != null ? (int)Req.pro_id : 0);
                Req.Product = allProduct.FirstOrDefault(x => x.id == Req.pro_id);
                if (Req.Product != null)
                    Req.Product.ResponsibleDepartment = allDepartment.FirstOrDefault(x => x.id == info_pro.responsible);
            }

            var model = new AllModelVM()
            {
                user = currentUser,
                request = Req,
                roleBack = type,
                reqPhotoList = ReqPhotoDataList.Count > 0 ? ReqPhotoDataList : null,
                reqCategoryList = req_category_list.Count > 0 ? req_category_list : null,
                incCount = incCount,
                reqCount = reqCount
            };

            return model;
        }

        public async Task<IActionResult> Req_Info(int id, string type)
        {
            var model = await get_Req_Info_Data(id, type);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Req_Info(List<IFormFile> files, Request req, string roleBack)
        {
            var model = await get_Req_Info_Data(req.id, roleBack);

            var reqinfo = await _reqApi.FindByIDRequest_API(req.id);

            if (req.description == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            if(reqinfo.req_type == false)
            {
                var info_pro = await _productApi.FindByIDProduct_API(req.pro_id != null ? (int)req.pro_id : 0);

                if (info_pro != null)
                {
                    if(req.quantity != reqinfo.quantity)
                    {
                        int root = info_pro.quantity + (reqinfo.quantity != null ? (int)reqinfo.quantity : 0);

                        if (req.quantity > root || req.quantity <= 0)
                        {
                            ViewBag.Error = "Error Quantity";
                            return View(model);
                        }

                        info_pro.quantity = root - (reqinfo.quantity != null ? (int)reqinfo.quantity : 0);
                        if (info_pro.quantity <= 0)
                            info_pro.active = false;
                        else
                            info_pro.active = true;

                        await _productApi.UpdateProduct_API(info_pro);

                        reqinfo.quantity = req.quantity;
                    }
                }
                else
                {
                    ViewBag.Error = "Cannot Fint Product Information";
                    return View(model);
                }
            }
            else
            {
                List<byte[]> fileBytesList = new List<byte[]>();

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 50_000_000) // 50MB
                        {
                            ViewBag.Error = "One of the files exceeds the 50MB limit.";
                            return View(model);
                        }

                        if (file.Length > 0)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await file.CopyToAsync(memoryStream);
                                fileBytesList.Add(memoryStream.ToArray());
                            }
                        }
                    }
                }

                if (fileBytesList != null && fileBytesList.Count > 0)
                {
                    foreach (var fileBytes in fileBytesList)
                    {
                        var reqPhone = new RequestPhoto
                        {
                            request_id = reqinfo.id,
                            photo = fileBytes,
                            photo_type = GetMimeTypeFromFileSignature(fileBytes)
                        };
                        await _reqphoneApi.CreateRequestPhoto_API(reqPhone);
                    }
                }

                reqinfo.erp_category = req.erp_category;
                reqinfo.erp_subcategory = req.erp_subcategory;
                reqinfo.erp_function = req.erp_function;
                reqinfo.erp_module = req.erp_module;
                reqinfo.erp_user_account = req.erp_user_account;
                reqinfo.erp_report = req.erp_report;
            }

            reqinfo.description = req.description;
            reqinfo.state = req.state;
            reqinfo.updated_by = model.user.id;
            reqinfo.assigned_to = req.assigned_to;

            bool result = await _reqApi.UpdateRequest_API(reqinfo);

            if (result)
            {
                if (roleBack == "Admin")
                    return RedirectToAction("All", "Request");
                else if (roleBack == "Group")
                    return RedirectToAction("Assigned_To_Us", "Request");
                else if (roleBack == "Tome")
                    return RedirectToAction("Assigned_To_Me", "Request");
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
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

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
            req_info.Sender = allUser.FirstOrDefault(x => x.id == req_info.sender);
            req_info.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == req_info.assignment_group);
            req_info.UpdatedBy = allUser.FirstOrDefault(x => x.id == req_info.updated_by);
            req_info.AssignedTo = allUser.FirstOrDefault(x => x.id == req_info.assigned_to);

            var ReqPhotoDataList = new List<RequestPhoto>();   
            if(req_info.req_type == true)
            {
                var reqcategoryTask = _reqcategoryApi.GetAllReq_Category_API();
                var reqsubcategoryTask = _reqsubcategoryApi.GetAllReq_Subcategory_API();
                var reqfunctionTask = _reqfunctionApi.GetAllReq_Function_API();
                var reqphotoTask = _reqphoneApi.GetAllRequestPhoto_API();
                await Task.WhenAll(reqcategoryTask, reqsubcategoryTask, reqfunctionTask, reqphotoTask);

                var allReqCategory = reqcategoryTask.Result;
                var allReqSubcategory = reqsubcategoryTask.Result;
                var allReqFunction = reqfunctionTask.Result;
                var allReqPhoto = reqphotoTask.Result;

                req_info.ERPCategory = allReqCategory.FirstOrDefault(x => x.id == req_info.erp_category);
                req_info.ERPSubcategory = allReqSubcategory.FirstOrDefault(x => x.id == req_info.erp_subcategory);
                req_info.ERPFunction = allReqFunction.FirstOrDefault(x => x.id == req_info.erp_function);

                ReqPhotoDataList = allReqPhoto.Where(x => x.request_id == req_info.id).ToList();

                if (ReqPhotoDataList.Count == 0 || ReqPhotoDataList == null)
                    ReqPhotoDataList = new List<RequestPhoto>();
            }
            else
            {
                var info_pro = await _productApi.FindByIDProduct_API(req_info.pro_id != null ? (int)req_info.pro_id : 0);
                req_info.Product = allProduct.FirstOrDefault(x => x.id == req_info.pro_id);
                if(req_info.Product != null)
                    req_info.Product.ResponsibleDepartment = allDepartment.FirstOrDefault(x => x.id == info_pro.responsible);
            }

            var model = new AllModelVM()
            {
                user = currentUser,
                request = req_info,
                reqPhotoList = ReqPhotoDataList.Count > 0 ? ReqPhotoDataList : null,
                incCount = incCount,
                reqCount = reqCount
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

            if (req.description != null && req.assigned_to != null)
            {
                if (reqData != null)
                {
                    reqData.assigned_to = req.assigned_to;

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

        public async Task<IActionResult> Application()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM()
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        public async Task<AllModelVM> get_req_application_data(string type)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var reqcategoryTask = await _reqcategoryApi.GetAllReq_Category_API();

            var ReqCategorys = new List<Req_Category>();
            if (type == "Application_Account_form")
                ReqCategorys = reqcategoryTask.Where(x => x.erp_version.ToLower() == "erp 8").ToList();
            else if (type == "Application_ERP_form")
                ReqCategorys = reqcategoryTask.Where(x => x.erp_version.ToLower() == "erp 9").ToList();

            var model = new AllModelVM()
            {
                user = currentUser,
                reqCategoryList = ReqCategorys,
                incCount = incCount,
                reqCount = reqCount
            };

            return model;
        }

        public async Task<IActionResult> Application_Account_form()
        {
            var model = await get_req_application_data("Application_Account_form");

            return View(model);
        }

        public async Task<IActionResult> Application_ERP_form()
        {
            var model = await get_req_application_data("Application_ERP_form");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Application_form(List<IFormFile> files, Request req, string version_type)
        {
            var model = await get_req_application_data("Application_Account_form");

            if (!string.IsNullOrEmpty(req.description))
            {
                var allRequest = await _reqApi.GetAllRequest_API();

                List<byte[]> fileBytesList = new List<byte[]>();

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 50_000_000) // 50MB
                        {
                            ViewBag.Error = "One of the files exceeds the 50MB limit.";
                            return View(model);
                        }

                        if (file.Length > 0)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await file.CopyToAsync(memoryStream);
                                fileBytesList.Add(memoryStream.ToArray());
                            }
                        }
                    }
                }

                string newId = "";
                if (allRequest.Count > 0)
                {
                    var incidentLast = allRequest.Last();
                    string r_id_up = incidentLast.req_id;
                    string prefix = new string(r_id_up.TakeWhile(char.IsLetter).ToArray());
                    string numberPart = new string(r_id_up.SkipWhile(char.IsLetter).ToArray());
                    int number = int.Parse(numberPart);
                    newId = prefix + (number + 1);
                }
                else
                    newId = "REQ1";

                var alldepartment = await _depApi.GetAllDepartment_API();

                var new_rep = new Request()
                {
                    req_id = newId,
                    description = req.description,
                    erp_user_account = req.erp_user_account == null ? null : req.erp_user_account,
                    state = "Pending",
                    erp_report = req.erp_report,
                    erp_category = req.erp_category,
                    erp_subcategory = req.erp_subcategory,
                    erp_function = req.erp_function == null ? null : req.erp_function,
                    erp_module = req.erp_module,
                    req_type = true,
                    erp_version = version_type,
                    sender = model.user.id,
                    assignment_group = alldepartment.FirstOrDefault(x => x.name.ToLower() == "it")?.id
                };

                bool result = await _reqApi.CreateRequest_API(new_rep);

                if (result)
                {
                    var createdIRequest = await _reqApi.FindByReqIDIncident_API(newId);

                    if (createdIRequest != null && fileBytesList != null && fileBytesList.Count > 0)
                    {
                        foreach (var fileBytes in fileBytesList)
                        {
                            var reqPhone = new RequestPhoto
                            {
                                request_id = createdIRequest.id,
                                photo = fileBytes,
                                photo_type = GetMimeTypeFromFileSignature(fileBytes)
                            };
                            await _reqphoneApi.CreateRequestPhoto_API(reqPhone);
                        }
                    }

                    return RedirectToAction("User_All", "Request");
                }
                else
                {
                    ViewBag.Error = "Create Request Error";
                    return View(model);
                }
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }
        }

        private string GetMimeTypeFromFileSignature(byte[] fileBytes)
        {
            if (fileBytes.Length < 4) return "application/octet-stream";

            // PNG
            if (fileBytes[0] == 0x89 && fileBytes[1] == 0x50 &&
                fileBytes[2] == 0x4E && fileBytes[3] == 0x47)
                return "image/png";

            // JPEG/JPG
            if (fileBytes[0] == 0xFF && fileBytes[1] == 0xD8 && fileBytes[2] == 0xFF)
                return "image/jpeg";

            // GIF
            if (fileBytes[0] == 0x47 && fileBytes[1] == 0x49 && fileBytes[2] == 0x46)
                return "image/gif";

            // WebP
            if (fileBytes.Length >= 12 &&
                fileBytes[0] == 0x52 && fileBytes[1] == 0x49 &&
                fileBytes[2] == 0x46 && fileBytes[3] == 0x46 &&
                fileBytes[8] == 0x57 && fileBytes[9] == 0x45 &&
                fileBytes[10] == 0x42 && fileBytes[11] == 0x50)
                return "image/webp";

            // BMP
            if (fileBytes[0] == 0x42 && fileBytes[1] == 0x4D)
                return "image/bmp";

            // TIFF (little endian)
            if (fileBytes[0] == 0x49 && fileBytes[1] == 0x49 &&
                fileBytes[2] == 0x2A && fileBytes[3] == 0x00)
                return "image/tiff";

            // TIFF (big endian)
            if (fileBytes[0] == 0x4D && fileBytes[1] == 0x4D &&
                fileBytes[2] == 0x00 && fileBytes[3] == 0x2A)
                return "image/tiff";

            // ICO
            if (fileBytes[0] == 0x00 && fileBytes[1] == 0x00 &&
                fileBytes[2] == 0x01 && fileBytes[3] == 0x00)
                return "image/x-icon";

            // HEIF
            if (fileBytes.Length >= 12 &&
                ((fileBytes[4] == 0x66 && fileBytes[5] == 0x74 &&
                  fileBytes[6] == 0x79 && fileBytes[7] == 0x70 &&
                  fileBytes[8] == 0x68 && fileBytes[9] == 0x65 &&
                  fileBytes[10] == 0x69 && fileBytes[11] == 0x63) || // heic
                 (fileBytes[4] == 0x66 && fileBytes[5] == 0x74 &&
                  fileBytes[6] == 0x79 && fileBytes[7] == 0x70 &&
                  fileBytes[8] == 0x6D && fileBytes[9] == 0x69 &&
                  fileBytes[10] == 0x66 && fileBytes[11] == 0x31)))   // heif
                return "image/heif";

            // AVIF
            if (fileBytes.Length >= 12 &&
                fileBytes[4] == 0x66 && fileBytes[5] == 0x74 &&
                fileBytes[6] == 0x79 && fileBytes[7] == 0x70 &&
                fileBytes[8] == 0x61 && fileBytes[9] == 0x76 &&
                fileBytes[10] == 0x69 && fileBytes[11] == 0x66)
                return "image/avif";

            // Basic
            return "application/octet-stream";
        }
    }
}
