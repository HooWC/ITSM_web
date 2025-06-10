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
    public class Sucategory_api
    {
        private readonly string _allSucategoryUrl = Api_Link.SucategoryLink;
        private readonly string _sudSucategoryUrl = Api_Link.SucategorySUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Sucategory_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Sucategory>> GetAllSucategory_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Sucategory>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allSucategoryUrl);
                return JsonConvert.DeserializeObject<List<Sucategory>>(jsonStr) ?? new List<Sucategory>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllSucategory_API: {ex.Message}");
                return new List<Sucategory>();
            }
        }

        public async Task<Sucategory> FindByIDSucategory_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudSucategoryUrl}{id}");
                var SucategoryList = JsonConvert.DeserializeObject<List<Sucategory>>(jsonStr);
                return SucategoryList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDSucategory_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateSucategory_API(Sucategory Sucategory)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Sucategory), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudSucategoryUrl}{Sucategory.id}", jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateSucategory_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateSucategory_API(Sucategory Sucategory)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Sucategory), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allSucategoryUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateSucategory_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteSucategory_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudSucategoryUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteSucategory_API: {ex.Message}");
                return false;
            }
        }
    }
}
