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
    public class Category_api
    {
        private readonly string _allCategoryUrl = Api_Link.CategoryLink;
        private readonly string _sudCategoryUrl = Api_Link.CategorySUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Category_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Category>> GetAllCategory_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Category>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allCategoryUrl);
                return JsonConvert.DeserializeObject<List<Category>>(jsonStr) ?? new List<Category>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllCategory_API: {ex.Message}");
                return new List<Category>();
            }
        }

        public async Task<Category> FindByIDCategory_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudCategoryUrl}{id}");
                var CategoryList = JsonConvert.DeserializeObject<List<Category>>(jsonStr);
                return CategoryList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDCategory_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateCategory_API(Category Category)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Category), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudCategoryUrl}{Category.id}", jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateCategory_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateCategory_API(Category Category)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Category), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allCategoryUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateCategory_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCategory_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudCategoryUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteCategory_API: {ex.Message}");
                return false;
            }
        }
    }
}
