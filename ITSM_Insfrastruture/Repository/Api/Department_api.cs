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
    public class Department_api
    {
        private readonly string _allDepartmentUrl = Api_Link.DepartmentLink;
        private readonly string _sudDepartmentUrl = Api_Link.DepartmentSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Department_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Department>> GetAllDepartment_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Department>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allDepartmentUrl);
                return JsonConvert.DeserializeObject<List<Department>>(jsonStr) ?? new List<Department>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllDepartment_API: {ex.Message}");
                return new List<Department>();
            }
        }

        public async Task<Department> FindByIDDepartment_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudDepartmentUrl}{id}");
                return JsonConvert.DeserializeObject<Department>(jsonStr);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDDepartment_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateDepartment_API(Department department)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(department), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudDepartmentUrl}{department.id}", jsonStr);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateDepartment_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateDepartment_API(Department department)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(department), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allDepartmentUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateDepartment_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteDepartment_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudDepartmentUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteDepartment_API: {ex.Message}");
                return false;
            }
        }
    }
}
