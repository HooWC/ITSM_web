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
    public class Req_Subcategory_api
    {
        private readonly string _allReq_SubcategoryUrl = Api_Link.ReqSubcategoryLink;
        private readonly string _sudReq_SubcategoryUrl = Api_Link.ReqSubcategorySUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Req_Subcategory_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Req_Subcategory>> GetAllReq_Subcategory_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Req_Subcategory>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allReq_SubcategoryUrl);
                return JsonConvert.DeserializeObject<List<Req_Subcategory>>(jsonStr) ?? new List<Req_Subcategory>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllReq_Subcategory_API: {ex.Message}");
                return new List<Req_Subcategory>();
            }
        }

        public async Task<Req_Subcategory> FindByIDReq_Subcategory_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync($"{_sudReq_SubcategoryUrl}{id}");

                var Req_SubcategoryList = JsonConvert.DeserializeObject<List<Req_Subcategory>>(jsonStr);
                return Req_SubcategoryList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDReq_Subcategory_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateReq_Subcategory_API(Req_Subcategory Req_Subcategory)
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
                    JsonConvert.SerializeObject(Req_Subcategory, jsonSettings),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PutAsync($"{_sudReq_SubcategoryUrl}{Req_Subcategory.id}", jsonStr);
                //var responseStr = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"RESPONSE UPDATE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateReq_Subcategory_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateReq_Subcategory_API(Req_Subcategory Req_Subcategory)
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
                    JsonConvert.SerializeObject(Req_Subcategory, jsonSettings),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PostAsync(_allReq_SubcategoryUrl, jsonStr);

                // Testing Date Error
                // var responseStr = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateReq_Subcategory_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteReq_Subcategory_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudReq_SubcategoryUrl}{id}");

                // Testing Debug
                // var responseStr = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteReq_Subcategory_API: {ex.Message}");
                return false;
            }
        }
    }
}
