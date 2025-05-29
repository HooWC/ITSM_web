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
    public class Myversion_api
    {
        private readonly string _allMyversionUrl = Api_Link.VersionLink;
        private readonly string _sudMyversionUrl = Api_Link.VersionSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Myversion_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Myversion>> GetAllMyversion_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Myversion>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allMyversionUrl);
                return JsonConvert.DeserializeObject<List<Myversion>>(jsonStr) ?? new List<Myversion>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllMyversion_API: {ex.Message}");
                return new List<Myversion>();
            }
        }

        public async Task<Myversion> FindByIDMyversion_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudMyversionUrl}{id}");

                var MyversionList = JsonConvert.DeserializeObject<List<Myversion>>(jsonStr);
                return MyversionList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDMyversion_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateMyversion_API(Myversion Myversion)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Myversion), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudMyversionUrl}{Myversion.id}", jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateMyversion_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateMyversion_API(Myversion Myversion)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Myversion), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allMyversionUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateMyversion_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteMyversion_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudMyversionUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteMyversion_API: {ex.Message}");
                return false;
            }
        }
    }
}
