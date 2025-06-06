using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Config;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class UserController : Controller
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

        public UserController(IHttpContextAccessor httpContextAccessor, UserService userService)
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

        public async Task<IActionResult> User_List()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var userTask = _userApi.GetAllUser_API();
            var depTask = _depApi.GetAllDepartment_API();
            var roleTask = _roleApi.GetAllRole_API();
            await Task.WhenAll(userTask, depTask, roleTask);

            var allUser = userTask.Result;
            var allDep = depTask.Result;
            var allRole = roleTask.Result;

            foreach(var i in allUser)
            {
                i.Department = allDep.FirstOrDefault(x => x.id == i.department_id);
                i.Role = allRole.FirstOrDefault(x => x.id == i.role_id);
            }

            var model = new AllModelVM()
            {
                user = currentUser,
                UserList = allUser.Where(x => x.id != currentUser.id).OrderByDescending(y => y.id).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> UserCreate()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var departmentTask = _departmentApi.GetAllDepartment_API();
            var roleTask = _roleApi.GetAllRole_API();
            await Task.WhenAll(departmentTask, roleTask);

            var allDepartment = departmentTask.Result;
            var allRole = roleTask.Result;

            var model = new AllModelVM()
            {
                user = currentUser,
                RoleList = allRole.OrderByDescending(x => x.id).ToList(),
                DepartmentList = allDepartment
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UserCreate(IFormFile file, User user, string role_code)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var departmentTask = _departmentApi.GetAllDepartment_API();
            var roleTask = _roleApi.GetAllRole_API();
            var userTask = _userApi.GetAllUser_API();
            await Task.WhenAll(departmentTask, roleTask, userTask);

            var allDepartment = departmentTask.Result;
            var allRole = roleTask.Result;
            var allUser = userTask.Result;

            var model = new AllModelVM()
            {
                user = currentUser,
                RoleList = allRole.OrderByDescending(x => x.id).ToList(),
                DepartmentList = allDepartment
            };

            if (!string.IsNullOrEmpty(user.emp_id) &&
                !string.IsNullOrEmpty(role_code) &&
                !string.IsNullOrEmpty(user.fullname) &&
                !string.IsNullOrEmpty(user.password) &&
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

                if (user.password != null && user.password.Length <= 6)
                {
                    ViewBag.Error = "The password must be at least 6 word. Please try again.";
                    return View(model);
                }

                bool emailEmpid = allUser.Any(u => u.emp_id.ToLower() == user.emp_id.ToLower());
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

                var newuser = new User()
                {
                    emp_id = user.emp_id,
                    fullname = user.fullname,
                    email = user.email,
                    gender = user.gender,
                    department_id = user.department_id,
                    title = user.title,
                    business_phone = user.business_phone == null ? null : user.business_phone,
                    mobile_phone = user.mobile_phone,
                    password = user.password,
                    race = user.race,
                    approve = false,
                    Manager = null
                };

                if (fileBytes != null)
                {
                    newuser.photo = fileBytes;
                    newuser.photo_type = GetMimeTypeFromFileSignature(fileBytes);
                }

                // Prefix
                if (user.gender == "Male")
                    newuser.prefix = "Mr.";
                else if (user.gender == "Female")
                    newuser.prefix = "Ms.";
                else
                    newuser.prefix = "-";

                // role
                string expectedRoleCode;
                switch (user.role_id)
                {
                    case 1: // Admin
                        expectedRoleCode = Info.AdminCode;
                        break;
                    case 2: // ITIL
                        expectedRoleCode = Info.ITILCode;
                        break;
                    case 3: // User
                        expectedRoleCode = Info.UserCode;
                        break;
                    default:
                        ViewBag.Error = "Role Error";
                        return View(model);
                }

                if (role_code != expectedRoleCode)
                {
                    ViewBag.Error = "Role Code Error";
                    return View(model);
                }

                newuser.role_id = user.role_id;

                var registerResult = await _userApi.AdminCreateUser(newuser);

                if (!registerResult.Success)
                {
                    ViewBag.Error = registerResult.Message ?? "Api Error:Failed to create user";
                    return View(model);
                }
                else
                    return RedirectToAction("User_List", "User");
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }
        }

        public async Task<IActionResult> User_Info(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var departmentTask = _departmentApi.GetAllDepartment_API();
            var roleTask = _roleApi.GetAllRole_API();
            await Task.WhenAll(departmentTask, roleTask);

            var allDepartment = departmentTask.Result;
            var allRole = roleTask.Result;

            var info_user = await _userApi.FindByIDUser_API(id);
            info_user.Department = allDepartment.Where(x => x.id == currentUser.department_id).FirstOrDefault();
            info_user.Role = allRole.Where(x => x.id == currentUser.role_id).FirstOrDefault();

            var model = new AllModelVM()
            {
                user = currentUser,
                RoleList = allRole,
                DepartmentList = allDepartment,
                info_user = info_user
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> User_Info(User user, string role_code, string new_password)
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
            info_user.Department = allDepartment.Where(x => x.id == currentUser.department_id).FirstOrDefault();
            info_user.Role = allRole.Where(x => x.id == currentUser.role_id).FirstOrDefault();

            var model = new AllModelVM()
            {
                user = currentUser,
                RoleList = allRole,
                DepartmentList = allDepartment,
                info_user = info_user
            };

            if (!string.IsNullOrEmpty(user.emp_id) &&
                !string.IsNullOrEmpty(user.fullname) &&
                !string.IsNullOrEmpty(user.email) &&
                !string.IsNullOrEmpty(user.title) &&
                !string.IsNullOrEmpty(user.mobile_phone))
            {
               
                if (user.password != null && new_password != null)
                {
                    if (user.password == new_password)
                    {
                        ViewBag.Error = "The old and new passwords cannot be the same. Please try again.";
                        return View(model);
                    }
                    else if (new_password.Length <= 6)
                    {
                        ViewBag.Error = "The mew password must be at least 6 word. Please try again.";
                        return View(model);
                    }
                    else
                    {
                        try
                        {
                            bool loginResult = await _authApi.LoginAsync(user.emp_id, user.password);

                            if (loginResult)
                                info_user.password = new_password;
                            else
                            {
                                ViewBag.Error = "Wrong employee id or password. Try again.";
                                return View(model);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ex Message: {ex.Message}");
                            Console.WriteLine($"Ex StackTrace: {ex.StackTrace}");
                            ViewBag.Error = "An error occurred during login, please try again later";
                            return View(model);
                        }
                    }
                }


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

                info_user.emp_id = user.emp_id;
                info_user.email = user.email;
                info_user.gender = user.gender;
                info_user.fullname = user.fullname;
                info_user.department_id = user.department_id;
                info_user.title = user.title;
                info_user.race = user.race;
                info_user.business_phone = user.business_phone;
                info_user.mobile_phone = user.mobile_phone;
                info_user.active = user.active;

                // Prefix
                if (user.gender == "Male")
                    info_user.prefix = "Mr.";
                else if (user.gender == "Female")
                    info_user.prefix = "Ms.";
                else
                    info_user.prefix = "-";

                // role
                if (user.role_id != info_user.role_id)
                {
                    // Role Code
                    string expectedRoleCode;
                    switch (user.role_id)
                    {
                        case 1: // Admin
                            expectedRoleCode = Info.AdminCode;
                            break;
                        case 2: // ITIL
                            expectedRoleCode = Info.ITILCode;
                            break;
                        case 3: // User
                            expectedRoleCode = Info.UserCode;
                            break;
                        default:
                            ViewBag.Error = "Role Error";
                            return View(model);
                    }

                    if (role_code != expectedRoleCode)
                    {
                        ViewBag.Error = "Role Code Error";
                        return View(model);
                    }

                    info_user.role_id = user.role_id;
                }

                bool result = await _userApi.UpdateUser_API(info_user);

                if (result)
                    return RedirectToAction("User_List", "User");
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

            // HEIF (需要更多字节检查)
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

            // 默认
            return "application/octet-stream";
        }
    }
}
