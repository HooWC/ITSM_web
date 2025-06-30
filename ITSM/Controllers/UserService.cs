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
        private readonly Note_api _noteApi;
        private readonly Incident_api _incApi;
        private readonly Request_api _reqApi;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _userApi = new User_api(httpContextAccessor);
            _roleApi = new Role_api(httpContextAccessor);
            _noteApi = new Note_api(httpContextAccessor);
            _incApi = new Incident_api(httpContextAccessor);
            _reqApi = new Request_api(httpContextAccessor);
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

        public async Task<int> GetIncidentTeamCount()
        {
            var currentUser = await GetCurrentUserAsync();

            var incidentListCount = await _incApi.GetAllIncident_API();
            var count = incidentListCount.Where(x => x.assignment_group == currentUser.department_id && x.state != "Resolved" && x.state != "Closed").Count();
            return count;
        }

        public async Task<int> GetRequestToMeCount()
        {
            var currentUser = await GetCurrentUserAsync();

            var requestListCount = await _reqApi.GetAllRequest_API();
            var count = requestListCount.Where(x => x.assigned_to == currentUser.id && x.state != "Completed" && x.state != "Rejected").Count();
            return count;
        }

        public async Task<IActionResult> checkIsAdmin()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser.Role?.role?.ToLower() != "admin")
                return RedirectToAction("Index", "Home");

            return Ok();
        }

    }
}
