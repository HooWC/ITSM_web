using ITSM_DomainModelEntity.Models;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class UserService : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly User_api _userApi;
        private readonly Role_api _roleApi;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _userApi = new User_api(httpContextAccessor);
            _roleApi = new Role_api(httpContextAccessor);
        }

        public async Task<User> GetCurrentUserAsync()
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);
            var allRole = await _roleApi.GetAllRole_API();

            currentUser.Role = allRole.FirstOrDefault(x => x.id == currentUser.role_id);
            return currentUser;
        }
    }
}
