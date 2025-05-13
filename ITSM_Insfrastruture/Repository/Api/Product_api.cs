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
    public class Product_api
    {
        private readonly string _allProductUrl = Api_Link.ProductLink;
        private readonly string _sudProductUrl = Api_Link.ProductSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Product_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Product>> GetAllProduct_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Product>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allProductUrl);
                return JsonConvert.DeserializeObject<List<Product>>(jsonStr) ?? new List<Product>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllProduct_API: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<Product> FindByIDProduct_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudProductUrl}{id}");

                var ProductList = JsonConvert.DeserializeObject<List<Product>>(jsonStr);
                return ProductList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDProduct_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateProduct_API(Product Product)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Product), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudProductUrl}{Product.id}", jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateProduct_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateProduct_API(Product Product)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Product), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allProductUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateProduct_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteProduct_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudProductUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteProduct_API: {ex.Message}");
                return false;
            }
        }
    }
}
