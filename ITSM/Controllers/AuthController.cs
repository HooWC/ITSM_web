using Microsoft.AspNetCore.Mvc;
using ITSM_Insfrastruture.Repository.Api;
using System.Threading.Tasks;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Linq;

namespace ITSM.Controllers
{
    public class AuthController : Controller
    {
        private readonly Auth_api _authApi;
        private readonly TokenService _tokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _authApi = new Auth_api(httpContextAccessor);
            _tokenService = new TokenService(httpContextAccessor);
        }

        public IActionResult Login()
        {
            try
            {
                Console.WriteLine("----- 开始自动登录检查 -----");
                Console.WriteLine($"当前URL: {Request.Path}");
                
                // 输出请求中的所有Cookie用于调试
                if (Request.Cookies != null && Request.Cookies.Count > 0)
                {
                    Console.WriteLine("请求中的Cookie:");
                    foreach (var cookie in Request.Cookies)
                    {
                        Console.WriteLine($"  {cookie.Key}: {(cookie.Key.Contains("Token") ? "[隐藏内容]" : cookie.Value)}");
                    }
                }
                else
                {
                    Console.WriteLine("请求中没有Cookie");
                }
                
                // 检查是否有有效令牌，如果有则自动登录
                Console.WriteLine("检查令牌有效性...");
                if (_tokenService.IsTokenValid())
                {
                    var token = _tokenService.GetToken();
                    if (token != null)
                    {
                        Console.WriteLine($"找到有效令牌，用户: {token.Username}，自动重定向到首页");
                        // 添加一个ViewBag消息，便于在页面上观察
                        ViewBag.AutoLoginMessage = $"自动登录成功，用户: {token.Username}";
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    // 检查为什么令牌无效
                    var sessionValue = "未能读取";
                    if (_httpContextAccessor.HttpContext?.Session != null)
                    {
                        // 使用完全限定名称解决SessionExtensions命名冲突
                        sessionValue = ITSM_Insfrastruture.Repository.Token.SessionExtensions.GetString(
                            _httpContextAccessor.HttpContext.Session, "UserToken") ?? "null";
                    }
                    Console.WriteLine($"令牌无效或不存在，Session值: {sessionValue}");
                }
                
                Console.WriteLine("----- 自动登录检查完成 -----");
            }
            catch (Exception ex)
            {
                // 记录任何异常
                Console.WriteLine($"检查令牌时发生错误: {ex.Message}");
                Console.WriteLine($"异常堆栈: {ex.StackTrace}");
            }
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string emp_id, string username, string password)
        {
            if (string.IsNullOrEmpty(emp_id) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "请填写所有必填字段";
                return View();
            }

            try
            {
                Console.WriteLine($"----- 开始手动登录 (用户: {username}) -----");
                bool loginResult = await _authApi.LoginAsync(emp_id, username, password);

                if (loginResult)
                {
                    // 登录成功，输出调试信息
                    var token = _tokenService.GetToken();
                    Console.WriteLine($"登录成功，用户: {token?.Username}");
                    
                    // 输出所有已设置的Cookie
                    Console.WriteLine("登录后响应中的Cookie:");
                    foreach (var cookie in Response.Headers.Where(h => h.Key == "Set-Cookie"))
                    {
                        Console.WriteLine($"  Set-Cookie: {cookie.Value}");
                    }
                    
                    Console.WriteLine("----- 手动登录完成，重定向到首页 -----");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Console.WriteLine("登录失败，用户名或密码错误");
                    ViewBag.ErrorMessage = "用户名或密码错误，请重试";
                    return View();
                }
            }
            catch (Exception ex)
            {
                // 记录登录异常
                Console.WriteLine($"登录过程中发生错误: {ex.Message}");
                Console.WriteLine($"异常堆栈: {ex.StackTrace}");
                ViewBag.ErrorMessage = "登录过程中发生错误，请稍后重试";
                return View();
            }
        }

        public IActionResult ForgotPasswrd()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }
    }
}
