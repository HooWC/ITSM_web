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
    public class Incident_Photos_api
    {
        private readonly string _allIncidentPhotosUrl = Api_Link.IncPhotosLink;
        private readonly string _sudIncidentPhotosUrl = Api_Link.IncPhotosSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Incident_Photos_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<IncidentPhotos>> GetAllIncidentPhotos_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<IncidentPhotos>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allIncidentPhotosUrl);
                return JsonConvert.DeserializeObject<List<IncidentPhotos>>(jsonStr) ?? new List<IncidentPhotos>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllIncidentPhotos_API: {ex.Message}");
                return new List<IncidentPhotos>();
            }
        }

        public async Task<IncidentPhotos> FindByIDIncidentPhotos_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudIncidentPhotosUrl}{id}");

                var IncidentPhotosList = JsonConvert.DeserializeObject<List<IncidentPhotos>>(jsonStr);
                return IncidentPhotosList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDIncidentPhotos_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateIncidentPhotos_API(IncidentPhotos IncidentPhotos)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(IncidentPhotos), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudIncidentPhotosUrl}{IncidentPhotos.id}", jsonStr);

                //var responseStr = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateIncidentPhotos_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateIncidentPhotos_API(IncidentPhotos IncidentPhotos)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(IncidentPhotos), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allIncidentPhotosUrl, jsonStr);

                // var responseStr = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateIncidentPhotos_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteIncidentPhotos_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudIncidentPhotosUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteIncidentPhotos_API: {ex.Message}");
                return false;
            }
        }
    }
}
