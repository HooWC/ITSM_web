using Microsoft.AspNetCore.Mvc;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Http;

namespace ITSM.Controllers
{
    public class AjaxController : Controller
    {
        private readonly TokenService _tokenService;

        public AjaxController(IHttpContextAccessor httpContextAccessor)
        {
            _tokenService = new TokenService(httpContextAccessor);
        }

        public IActionResult _Logout()
        {
            // 清除令牌，这样用户下次访问时必须重新登录
            _tokenService.ClearToken();
            
            // 返回JSON结果，表示退出成功
            return Json(new { success = true, message = "退出登录成功" });
        }
    }
}
