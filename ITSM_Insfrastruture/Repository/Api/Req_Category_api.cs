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
    public class Req_Category_api
    {
        private readonly string _allReq_CategoryUrl = Api_Link.ReqCategoryLink;
        private readonly string _sudReq_CategoryUrl = Api_Link.ReqCategorySUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Req_Category_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Req_Category>> GetAllReq_Category_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Req_Category>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allReq_CategoryUrl);
                return JsonConvert.DeserializeObject<List<Req_Category>>(jsonStr) ?? new List<Req_Category>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllReq_Category_API: {ex.Message}");
                return new List<Req_Category>();
            }
        }

        public async Task<Req_Category> FindByIDReq_Category_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync($"{_sudReq_CategoryUrl}{id}");

                var Req_CategoryList = JsonConvert.DeserializeObject<List<Req_Category>>(jsonStr);
                return Req_CategoryList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDReq_Category_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateReq_Category_API(Req_Category Req_Category)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);

                var jsonSettings = new JsonSerializerSettings
                {
                    DateFormatString = "yyyy-MM-dd HH:mm:ss",
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                };

                var jsonStr = new StringContent(
                    JsonConvert.SerializeObject(Req_Category, jsonSettings),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PutAsync($"{_sudReq_CategoryUrl}{Req_Category.id}", jsonStr);
                //var responseStr = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"RESPONSE UPDATE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateReq_Category_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateReq_Category_API(Req_Category Req_Category)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);

                var jsonSettings = new JsonSerializerSettings
                {
                    DateFormatString = "yyyy-MM-dd HH:mm:ss",
                    DateTimeZoneHandling = DateTimeZoneHandling.Local
                };

                var jsonStr = new StringContent(
                    JsonConvert.SerializeObject(Req_Category, jsonSettings),
                    Encoding.UTF8,
                    "application/json"
                );

                Console.WriteLine("Sending JSON: " + JsonConvert.SerializeObject(Req_Category, jsonSettings));

                var response = await _client.PostAsync(_allReq_CategoryUrl, jsonStr);

                // Testing Date Error
                //var responseStr = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateReq_Category_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteReq_Category_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudReq_CategoryUrl}{id}");

                // Testing Debug
                // var responseStr = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteReq_Category_API: {ex.Message}");
                return false;
            }
        }
    }
}
