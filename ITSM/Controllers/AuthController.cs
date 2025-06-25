using Microsoft.AspNetCore.Mvc;
using ITSM_Insfrastruture.Repository.Api;
using System.Threading.Tasks;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Linq;
using ITSM_DomainModelEntity.Models;
using ITSM_Insfrastruture.Repository.Config;
using System.Text;
using System.Security.Cryptography;
using ITSM_DomainModelEntity.ViewModels;
using System.Collections.Generic;

namespace ITSM.Controllers
{
    public class AuthController : Controller
    {
        private readonly Auth_api _authApi;
        private readonly UserService _userService;
        private readonly TokenService _tokenService;
        private readonly User_api _userApi;
        private readonly Role_api _roleApi;
        private readonly Department_api _departmentApi;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IHttpContextAccessor httpContextAccessor, UserService userService)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _authApi = new Auth_api(httpContextAccessor);
            _tokenService = new TokenService(httpContextAccessor);
            _userApi = new User_api(httpContextAccessor);
            _roleApi = new Role_api(httpContextAccessor);
            _departmentApi = new Department_api(httpContextAccessor);
            _userService = userService;
        }

        public async Task<IActionResult> Login()
        {
            try
            {
                if (_tokenService.IsTokenValid())
                {
                    var token = _tokenService.GetToken();
                    if (token != null)
                        return RedirectToAction("Index", "Home");
                }

                var allDepartment = await _departmentApi.GetAll_With_No_Token_Department_API();
                var allRole = await _roleApi.GetAll_With_No_Token_Role_API();

                var model = new AllModelVM()
                {
                    RoleList = allRole ?? new List<Role>(),
                    DepartmentList = allDepartment ?? new List<Department>()
                };

                if ((allDepartment == null || !allDepartment.Any()) && (allRole == null || !allRole.Any()))
                {
                    ViewBag.ErrorMessage = "Unable to load departments and roles. Please try again.";
                }
                
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while loading data.";
                return View(new AllModelVM 
                { 
                    RoleList = new List<Role>(),
                    DepartmentList = new List<Department>()
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(string emp_id, string password)
        {
            var RoleTask = _roleApi.GetAll_With_No_Token_Role_API();
            var DepartmentTask = _departmentApi.GetAll_With_No_Token_Department_API();
            await Task.WhenAll(RoleTask, DepartmentTask);

            var allRole = RoleTask.Result;
            var allDepartment = DepartmentTask.Result;

            var model = new AllModelVM()
            {
                RoleList = allRole,
                DepartmentList = allDepartment
            };
            
            if (string.IsNullOrEmpty(emp_id) ||  string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Please fill in all required fields";
                return View(model);
            }

            try
            {
                bool loginResult = await _authApi.LoginAsync(emp_id, password);

                if (loginResult)
                {
                    var currentUser = await _userService.GetCurrentUserAsync();

                    if (!currentUser.active)
                    {
                        ViewBag.ErrorMessage = "Your account has been blocked, please contact your supervisor.";
                        _tokenService.ClearToken();
                        return View(model);
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.ErrorMessage = "Wrong employee id or password. Try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex Message: {ex.Message}");
                Console.WriteLine($"Ex StackTrace: {ex.StackTrace}");
                ViewBag.ErrorMessage = "An error occurred during login, please try again later";
                return View(model);
            }
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost] // No use this function , already remove
        public async Task<IActionResult> ForgotPassword(User user)
        {
            bool res = await _userApi.VerifyUserWithoutToken(user);

            if (res)
            {
                bool reset = await _userApi.ResetForgotPassword(user);
                if(reset)
                    return RedirectToAction("Login", "Auth");
                else
                {
                    @ViewBag.ErrorMessage = "Reset Password Error";
                    return View();
                }
            }

            @ViewBag.ErrorMessage = "Employee No and User Name Error.";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            var RoleTask = _roleApi.GetAll_With_No_Token_Role_API();
            var DepartmentTask = _departmentApi.GetAll_With_No_Token_Department_API();
            await Task.WhenAll(RoleTask, DepartmentTask);

            var allRole = RoleTask.Result;
            var allDepartment = DepartmentTask.Result;

            var model = new AllModelVM()
            {
                RoleList = allRole,
                DepartmentList = allDepartment
            };

            try
            {
                if (string.IsNullOrEmpty(user.emp_id) || 
                    string.IsNullOrEmpty(user.fullname) || 
                    string.IsNullOrEmpty(user.email) ||
                    string.IsNullOrEmpty(user.password) ||
                    string.IsNullOrEmpty(user.mobile_phone))
                {
                    ViewBag.ErrorMessage = "Please fill in all required fields";
                    return View("Login");
                }

                // Prefix
                if (user.gender == "Male")
                    user.prefix = "Mr.";
                else if (user.gender == "Female")
                    user.prefix = "Ms.";
                else
                    user.prefix = "-";

                var newuser = new User()
                {
                    emp_id = user.emp_id,
                    photo = (byte[]?)null,
                    prefix = user.prefix,
                    fullname = user.fullname,
                    email = user.email,
                    gender = user.gender,
                    department_id = user.department_id,
                    title = user.title,
                    business_phone = user.business_phone == null ? null : user.business_phone,
                    mobile_phone = user.mobile_phone,
                    role_id = allRole.FirstOrDefault(x => x.role.ToLower() == "user").id,
                    password = user.password,
                    race = user.race,
                    r_manager = false
                };
                
                // Register Api
                bool resule = await _userApi.Register_User(newuser);
                
                if (!resule)
                {
                    ViewBag.ErrorMessage = "Registration failed, please try again";
                    return View("Login", model);
                }

                return View("Login", model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "EX: " + ex.Message;
                Console.WriteLine($"Register Error: {ex.Message}");
                Console.WriteLine($"Ex Error: {ex.StackTrace}");
                return View("Login", model);
            }
        }
    }
}
