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
    public class RequestPhotos_api
    {
        private readonly string _allRequestPhotoUrl = Api_Link.ReqPhotoLink;
        private readonly string _sudRequestPhotoUrl = Api_Link.ReqPhotoSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public RequestPhotos_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<RequestPhoto>> GetAllRequestPhoto_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<RequestPhoto>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allRequestPhotoUrl);
                return JsonConvert.DeserializeObject<List<RequestPhoto>>(jsonStr) ?? new List<RequestPhoto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllRequestPhoto_API: {ex.Message}");
                return new List<RequestPhoto>();
            }
        }

        public async Task<RequestPhoto> FindByIDRequestPhoto_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudRequestPhotoUrl}{id}");

                var RequestPhotoList = JsonConvert.DeserializeObject<List<RequestPhoto>>(jsonStr);
                return RequestPhotoList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDRequestPhoto_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateRequestPhoto_API(RequestPhoto RequestPhoto)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(RequestPhoto), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudRequestPhotoUrl}{RequestPhoto.id}", jsonStr);

                //var responseStr = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateRequestPhoto_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateRequestPhoto_API(RequestPhoto RequestPhoto)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(RequestPhoto), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allRequestPhotoUrl, jsonStr);

                // var responseStr = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateRequestPhoto_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteRequestPhoto_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudRequestPhotoUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteRequestPhoto_API: {ex.Message}");
                return false;
            }
        }
    }
}
