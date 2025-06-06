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

namespace ITSM.Controllers
{
    public class AuthController : Controller
    {
        private readonly Auth_api _authApi;
        private readonly TokenService _tokenService;
        private readonly User_api _userApi;
        private readonly Role_api _roleApi;
        private readonly Department_api _departmentApi;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _authApi = new Auth_api(httpContextAccessor);
            _tokenService = new TokenService(httpContextAccessor);
            _userApi = new User_api(httpContextAccessor);
            _roleApi = new Role_api(httpContextAccessor);
            _departmentApi = new Department_api(httpContextAccessor);
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
                else
                {
                    // Check why the token is invalid
                    var sessionValue = "Failed to read";
                    if (_httpContextAccessor.HttpContext?.Session != null)
                    {
                        sessionValue = ITSM_Insfrastruture.Repository.Token.SessionExtensions.GetString(
                            _httpContextAccessor.HttpContext.Session, "UserToken") ?? "null";
                    }
                    Console.WriteLine($"The token is invalid or does not exist，Session value: {sessionValue}");
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions
                Console.WriteLine($"An error occurred while checking the token: {ex.Message}");
                Console.WriteLine($"Exception stack: {ex.StackTrace}");
            }

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
            
            return View(model);
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
                    return RedirectToAction("Index", "Home");
                else
                {
                    ViewBag.ErrorMessage = "Wrong employee id or password. Try again.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions
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

        [HttpPost]
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
        public async Task<IActionResult> Register(User user, string role_code, string register_code)
        {
            try
            {
                // Check If Null
                if (string.IsNullOrEmpty(user.emp_id) || 
                    string.IsNullOrEmpty(user.fullname) || 
                    string.IsNullOrEmpty(user.email) ||
                    string.IsNullOrEmpty(user.password) ||
                    string.IsNullOrEmpty(user.mobile_phone) ||
                    string.IsNullOrEmpty(role_code) ||
                    string.IsNullOrEmpty(register_code))
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

                // Register Code
                if (register_code != Info.RegisterCode)
                {
                    ViewBag.ErrorMessage = "Register Code Error";
                    return View("Login");
                }

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
                        ViewBag.ErrorMessage = "Role Error";
                        return View("Login");
                }

                if (role_code != expectedRoleCode)
                {
                    ViewBag.ErrorMessage = "Role Code Error";
                    return View("Login");
                }

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
                    role_id = user.role_id,
                    password = user.password,
                    race = user.race,
                    approve = false,
                    Manager = null
                };
                
                // Register Api
                var userApi = new User_api(_httpContextAccessor);
                var registerResult = await userApi.Register_User(newuser);
                
                if (!registerResult.Success)
                {
                    ViewBag.ErrorMessage = registerResult.Message ?? "Registration failed";
                    return View("Login");
                }

                // Automatically log in after successful registration
                bool loginResult = await _authApi.LoginAsync(user.emp_id, user.password);
                
                if (loginResult)
                    return RedirectToAction("Index", "Home");
                else
                {
                    // If creation succeeds but login fails
                    ViewBag.ErrorMessage = "Registration was successful, but automatic login failed.";
                    return View("Login");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "EX: " + ex.Message;
                Console.WriteLine($"Register Error: {ex.Message}");
                Console.WriteLine($"Ex Error: {ex.StackTrace}");
                return View("Login");
            }
        }
    }
}
