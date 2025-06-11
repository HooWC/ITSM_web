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
    public class Subcategory_api
    {
        private readonly string _allSubcategoryUrl = Api_Link.SubcategoryLink;
        private readonly string _sudSubcategoryUrl = Api_Link.SubcategorySUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Subcategory_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Subcategory>> GetAllSubcategory_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Subcategory>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allSubcategoryUrl);
                return JsonConvert.DeserializeObject<List<Subcategory>>(jsonStr) ?? new List<Subcategory>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllSubcategory_API: {ex.Message}");
                return new List<Subcategory>();
            }
        }

        public async Task<Subcategory> FindByIDSubcategory_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudSubcategoryUrl}{id}");
                var SubcategoryList = JsonConvert.DeserializeObject<List<Subcategory>>(jsonStr);
                return SubcategoryList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDSubcategory_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateSubcategory_API(Subcategory Subcategory)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Subcategory), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudSubcategoryUrl}{Subcategory.id}", jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateSubcategory_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateSubcategory_API(Subcategory Subcategory)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Subcategory), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allSubcategoryUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateSubcategory_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteSubcategory_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudSubcategoryUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteSubcategory_API: {ex.Message}");
                return false;
            }
        }
    }
}
