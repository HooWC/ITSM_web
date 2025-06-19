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
    public class Req_Function_api
    {
        private readonly string _allReq_FunctionUrl = Api_Link.ReqFunctionLink;
        private readonly string _sudReq_FunctionUrl = Api_Link.ReqFunctionSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Req_Function_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Req_Function>> GetAllReq_Function_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Req_Function>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allReq_FunctionUrl);
                return JsonConvert.DeserializeObject<List<Req_Function>>(jsonStr) ?? new List<Req_Function>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllReq_Function_API: {ex.Message}");
                return new List<Req_Function>();
            }
        }

        public async Task<Req_Function> FindByIDReq_Function_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync($"{_sudReq_FunctionUrl}{id}");

                var Req_FunctionList = JsonConvert.DeserializeObject<List<Req_Function>>(jsonStr);
                return Req_FunctionList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDReq_Function_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateReq_Function_API(Req_Function Req_Function)
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
                    JsonConvert.SerializeObject(Req_Function, jsonSettings),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PutAsync($"{_sudReq_FunctionUrl}{Req_Function.id}", jsonStr);
                //var responseStr = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"RESPONSE UPDATE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateReq_Function_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateReq_Function_API(Req_Function Req_Function)
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
                    JsonConvert.SerializeObject(Req_Function, jsonSettings),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PostAsync(_allReq_FunctionUrl, jsonStr);

                // Testing Date Error
                // var responseStr = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateReq_Function_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteReq_Function_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudReq_FunctionUrl}{id}");

                // Testing Debug
                // var responseStr = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteReq_Function_API: {ex.Message}");
                return false;
            }
        }
    }
}
