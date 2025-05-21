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
    public class CMDB_api
    {
        private readonly string _allCMDBUrl = Api_Link.CMDBLink;
        private readonly string _sudCMDBUrl = Api_Link.CMDBSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public CMDB_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<CMDB>> GetAllCMDB_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<CMDB>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allCMDBUrl);
                return JsonConvert.DeserializeObject<List<CMDB>>(jsonStr) ?? new List<CMDB>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllCMDB_API: {ex.Message}");
                return new List<CMDB>();
            }
        }

        public async Task<CMDB> FindByIDCMDB_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudCMDBUrl}{id}");

                var CMDBList = JsonConvert.DeserializeObject<List<CMDB>>(jsonStr);
                return CMDBList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDCMDB_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateCMDB_API(CMDB CMDB)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(CMDB), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudCMDBUrl}{CMDB.id}", jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateCMDB_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateCMDB_API(CMDB CMDB)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(CMDB), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allCMDBUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateCMDB_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCMDB_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudCMDBUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteCMDB_API: {ex.Message}");
                return false;
            }
        }
    }
}
