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
    public class Request_api
    {
        private readonly string _allReqUrl = Api_Link.ReqLink;
        private readonly string _sudReqUrl = Api_Link.ReqSUDLink;
        private readonly string _ReqIDUrl = Api_Link.ReqIDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Request_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Request>> GetAllRequest_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Request>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allReqUrl);
                return JsonConvert.DeserializeObject<List<Request>>(jsonStr) ?? new List<Request>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllRequest_API: {ex.Message}");
                return new List<Request>();
            }
        }

        public async Task<Request> FindByIDRequest_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudReqUrl}{id}");
                var requestList = JsonConvert.DeserializeObject<List<Request>>(jsonStr);
                return requestList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDRequest_API: {ex.Message}");
                return null;
            }
        }

        public async Task<Request> FindByReqIDIncident_API(string req_id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_ReqIDUrl}{req_id}");
                var RequestList = JsonConvert.DeserializeObject<List<Request>>(jsonStr);
                return RequestList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDReqident_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateRequest_API(Request request)
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

                var jsonStr = new StringContent(JsonConvert.SerializeObject(request, jsonSettings), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudReqUrl}{request.id}", jsonStr);

                //var responseStr = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"RESPONSE UPDATE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateRequest_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateRequest_API(Request request)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allReqUrl, jsonStr);

                var responseStr = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateRequest_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteRequest_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudReqUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteRequest_API: {ex.Message}");
                return false;
            }
        }
    }
}
