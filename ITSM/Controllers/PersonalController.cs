using Humanizer;
using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Config;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class PersonalController : Controller
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

        public PersonalController(IHttpContextAccessor httpContextAccessor, UserService userService)
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

        public async Task<IActionResult> Home()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var todoTask = _todoApi.GetAllTodo_API();
            var incidentTask = _incApi.GetAllIncident_API();
            var userTask = _userApi.GetAllUser_API();
            var reqTask = _reqApi.GetAllRequest_API();
            var knowledgeTask = _knowledgeApi.GetAllKnowledge_API();
            var feedbackTask = _feedbackApi.GetAllFeedback_API();
            var departmentTask = _depApi.GetAllDepartment_API();
            var roleTask = _roleApi.GetAllRole_API();

            await Task.WhenAll(todoTask, incidentTask, userTask, reqTask, knowledgeTask, feedbackTask, departmentTask, roleTask);

            var allTodo = todoTask.Result;
            var todo = allTodo.Where(x => x.user_id == currentUser.id).ToList();
            var todo_c_count = todo.Count(x => x.active);
            var todo_td_count = todo.Count(x => !x.active);
            var todo_all_count = todo.Count;

            var allIncident = incidentTask.Result;
            var incident = allIncident.Where(x => x.sender == currentUser.id).OrderByDescending(y => y.id).ToList();
            var incident_list = incident.Take(9).ToList();
            var incident_r_count = incident.Count(x => x.state == "Resolved");
            var incident_p_count = incident.Count(x => x.state == "Pending");
            var incident_all_count = incident.Count;

            var allUser = userTask.Result;
            var sameDepartment = allUser.Where(x => x.department_id == currentUser.department_id && x.active == true && x.approve == true).Take(9).ToList();

            var allReq = reqTask.Result;
            var req = allReq.Where(x => x.sender == currentUser.id).ToList();
            var req_c_count = req.Count(x => x.state == "Completed");
            var req_p_count = req.Count(x => x.state == "Pending");
            var req_all_count = req.Count;

            var allKnowledge = knowledgeTask.Result;
            var knowledge_count = allKnowledge.Count(x => x.author == currentUser.id);

            var allFeedback = feedbackTask.Result;
            var feedback_count = allFeedback.Count(x => x.user_id == currentUser.id);

            var allDepartment = departmentTask.Result;
            var getDepartmentName = allDepartment.Where(x => x.id == currentUser.department_id).FirstOrDefault()?.name;

            var allRole = roleTask.Result;
            var getRoleName = allRole.Where(x => x.id == currentUser.role_id).FirstOrDefault()?.role;

            var model = new AllModelVM()
            {
                user = currentUser,
                User = currentUser,
                CompletedTodo = todo_c_count,
                TodoCount = todo_td_count,
                AllTodo = todo_all_count,
                AllInc = incident_all_count,
                ApplyInc = incident_p_count,
                CompletedInc = incident_r_count,
                AllReq = req_all_count,
                ApplyReq = req_p_count,
                CompletedReq = req_c_count,
                AllKnowledge = knowledge_count,
                AllFeedback = feedback_count,
                DepartmentName = getDepartmentName,
                RoleName = getRoleName,
                Team = sameDepartment,
                IncidentsHistory = incident_list
            };

            return View(model);
        }

        public async Task<IActionResult> Todo_List()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var todoTask = _todoApi.GetAllTodo_API();

            var allTodo = todoTask.Result;
            var todo = allTodo.Where(x => x.user_id == currentUser.id).OrderByDescending(y => y.id).ToList();

            var model = new AllModelVM
            {
                user = currentUser,
                TodoList = todo
            };

            return View(model);
        }

        public async Task<IActionResult> Todo_Create()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var model = new AllModelVM
            {
                user = currentUser
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Todo_Create(Todo todo, string active_word)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var model = new AllModelVM
            {
                user = currentUser
            };

            if (todo.title == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var todoTask = _todoApi.GetAllTodo_API();

            var allTodo = todoTask.Result;
            
            string newId = "";
            if (allTodo.Count > 0)
            {
                var last_todo = allTodo.Last();
                string t_id_up = last_todo.todo_id;
                string prefix = new string(t_id_up.TakeWhile(char.IsLetter).ToArray());
                string numberPart = new string(t_id_up.SkipWhile(char.IsLetter).ToArray());
                int number = int.Parse(numberPart);
                newId = prefix + (number + 1);
            }
            else
                newId = "TOD1";

            Todo new_todo = new Todo()
            {
                user_id = currentUser.id,
                title = todo.title,
                create_date = DateTime.Now,
                update_date = DateTime.Now,
                todo_id = newId,
                active = active_word == "Doing" ? false : true
            };

            bool result = await _todoApi.CreateTodo_API(new_todo);
 
            if (result)
                return RedirectToAction("Todo_List", "Personal");
            else
            {
                ViewBag.Error = "Create Todo Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Todo_Edit(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var todo = await _todoApi.FindByIDTodo_API(id);

            var model = new AllModelVM
            {
                user = currentUser,
                todo = todo
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Todo_Edit(Todo todo, string active_word)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var edit_todo = await _todoApi.FindByIDTodo_API(todo.id);

            var model = new AllModelVM
            {
                user = currentUser,
                todo = edit_todo
            };

            if (todo.title == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            if(edit_todo != null)
            {
                edit_todo.active = active_word == "Doing" ? false : true;
                edit_todo.title = todo.title;

                bool result = await _todoApi.UpdateTodo_API(edit_todo);

                if (result)
                    return RedirectToAction("Todo_List", "Personal");
                else
                {
                    ViewBag.Error = "Update Todo Error";
                    return View(model);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> User_Info()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var departmentTask = _departmentApi.GetAllDepartment_API();
            var roleTask = _roleApi.GetAllRole_API();
            await Task.WhenAll(departmentTask, roleTask);

            var allDepartment = departmentTask.Result;
            var allRole = roleTask.Result;

            currentUser.Department = allDepartment.Where(x => x.id == currentUser.department_id).FirstOrDefault();
            currentUser.Role = allRole.Where(x => x.id == currentUser.role_id).FirstOrDefault();

            var model = new AllModelVM()
            {
                user = currentUser,
                RoleList = allRole,
                DepartmentList = allDepartment
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> User_Info(IFormFile file, User user)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var departmentTask = _departmentApi.GetAllDepartment_API();
            var roleTask = _roleApi.GetAllRole_API();
            var userTask = _userApi.GetAllUser_API();
            await Task.WhenAll(departmentTask, roleTask, userTask);

            var allDepartment = departmentTask.Result;
            var allRole = roleTask.Result;
            var allUser = userTask.Result;

            var info_user = await _userApi.FindByIDUser_API(user.id);

            currentUser.Department = allDepartment.Where(x => x.id == currentUser.department_id).FirstOrDefault();
            currentUser.Role = allRole.Where(x => x.id == currentUser.role_id).FirstOrDefault();

            var model = new AllModelVM()
            {
                user = currentUser,
                RoleList = allRole,
                DepartmentList = allDepartment
            };

            if (!string.IsNullOrEmpty(user.emp_id) &&
                !string.IsNullOrEmpty(user.fullname) &&
                !string.IsNullOrEmpty(user.email) &&
                !string.IsNullOrEmpty(user.title) &&
                !string.IsNullOrEmpty(user.mobile_phone))
            {
                byte[] fileBytes = null;

                if (file != null && file.Length > 20_000_000)
                {
                    ViewBag.Error = "File size exceeds 20MB limit";
                    return View(model);
                }

                if (file != null && file.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                }

                //if (user.password != null && new_password != null)
                //{
                //    if (user.password == new_password)
                //    {
                //        ViewBag.Error = "The old and new passwords cannot be the same. Please try again.";
                //        return View(model);
                //    }
                //    else if (new_password.Length <= 6)
                //    {
                //        ViewBag.Error = "The mew password must be at least 6 word. Please try again.";
                //        return View(model);
                //    }
                //    else
                //    {
                //        try
                //        {
                //            bool loginResult = await _authApi.LoginAsync(user.emp_id, user.password);

                //            if (loginResult)
                //                info_user.password = new_password;
                //            else
                //            {
                //                ViewBag.Error = "Wrong employee id or password. Try again.";
                //                return View(model);
                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //            Console.WriteLine($"Ex Message: {ex.Message}");
                //            Console.WriteLine($"Ex StackTrace: {ex.StackTrace}");
                //            ViewBag.Error = "An error occurred during login, please try again later";
                //            return View(model);
                //        }
                //    }
                //}

                bool emailEmpid = allUser.Any(u => u.emp_id.ToLower() == user.emp_id.ToLower() && u.id != user.id);
                bool emailExists = allUser.Any(u => u.email == user.email && u.id != user.id);
                bool mobilephoneExists = allUser.Any(u => u.mobile_phone == user.mobile_phone && u.id != user.id);
                bool businessphoneExists = !string.IsNullOrWhiteSpace(user.business_phone) &&
                                            allUser.Any(u => u.business_phone == user.business_phone && u.id != user.id);

                if (emailEmpid)
                {
                    ViewBag.Error = "This Emp Id is already in use.";
                    return View(model);
                }

                if (emailExists)
                {
                    ViewBag.Error = "This email is already in use.";
                    return View(model);
                }

                if (mobilephoneExists)
                {
                    ViewBag.Error = "This mobile phone is already in use.";
                    return View(model);
                }

                if (businessphoneExists)
                {
                    ViewBag.Error = "This business phone is already in use.";
                    return View(model);
                }

                //if (info_user.department_id != user.department_id)
                //{
                //    var userManager = allUser.FirstOrDefault(t => t.department_id == user.department_id && t.r_manager == true);
                //    if (userManager != null)
                //        info_user.Manager = userManager.id;
                //    else info_user.Manager = null;
                //}

                info_user.emp_id = user.emp_id;
                info_user.email = user.email;
                info_user.gender = user.gender;
                info_user.fullname = user.fullname;
                info_user.title = user.title;
                info_user.race = user.race;
                info_user.business_phone = user.business_phone;
                info_user.mobile_phone = user.mobile_phone;

                // Prefix
                if (user.gender == "Male")
                    info_user.prefix = "Mr.";
                else if (user.gender == "Female")
                    info_user.prefix = "Ms.";
                else
                    info_user.prefix = "-";

                if (fileBytes != null)
                {
                    info_user.photo = fileBytes;
                    info_user.photo_type = GetMimeTypeFromFileSignature(fileBytes);
                }

                bool result = await _userApi.UpdateUser_API(info_user);

                if (result)
                {
                    var tokenService = new TokenService(_httpContextAccessor);
                    tokenService.SaveUserInfo(info_user);
                    return RedirectToAction("Home", "Personal");
                }
                else
                {
                    ViewBag.Error = "Update User Info Error";
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
