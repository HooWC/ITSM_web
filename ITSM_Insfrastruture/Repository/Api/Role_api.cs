using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;
using ITSM_Insfrastruture.Repository.Config;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ITSM_Insfrastruture.Repository.Api
{
    public class Role_api
    {
        private readonly string _allRoleUrl = Api_Link.RoleLink;
        private readonly string _sudRoleUrl = Api_Link.RoleSUDLink;
        private readonly string _getallRoleUrl = Api_Link.RoleGetAllLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Role_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Role>> GetAllRole_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Role>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allRoleUrl);
                return JsonConvert.DeserializeObject<List<Role>>(jsonStr) ?? new List<Role>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllRole_API: {ex.Message}");
                return new List<Role>();
            }
        }

        public async Task<List<Role>> GetAll_With_No_Token_Role_API()
        {
            try
            {
                var response = await _client.GetAsync(_getallRoleUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return new List<Role>();
                }

                string jsonStr = await response.Content.ReadAsStringAsync();
                var roles = JsonConvert.DeserializeObject<List<Role>>(jsonStr);
                return roles ?? new List<Role>();
            }
            catch (Exception)
            {
                return new List<Role>();
            }
        }

        public async Task<Role> FindByIDRole_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudRoleUrl}{id}");
                var roleList = JsonConvert.DeserializeObject<List<Role>>(jsonStr);
                return roleList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDRole_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateRole_API(Role role)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(role), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudRoleUrl}{role.id}", jsonStr);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateRole_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateRole_API(Role role)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(role), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allRoleUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateRole_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteRole_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudRoleUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteRole_API: {ex.Message}");
                return false;
            }
        }
    }
}
