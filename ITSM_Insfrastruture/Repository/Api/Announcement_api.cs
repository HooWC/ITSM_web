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
    public class Announcement_api
    {
        private readonly string _allAnnounUrl = Api_Link.AnnouncementLink;
        private readonly string _sudAnnounUrl = Api_Link.AnnouncementSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Announcement_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Announcement>> GetAllAnnouncement_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Announcement>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allAnnounUrl);
                return JsonConvert.DeserializeObject<List<Announcement>>(jsonStr) ?? new List<Announcement>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllAnnouncement_API: {ex.Message}");
                return new List<Announcement>();
            }
        }

        public async Task<Announcement> FindByIDAnnouncement_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudAnnounUrl}{id}");

                var AnnouncementList = JsonConvert.DeserializeObject<List<Announcement>>(jsonStr);
                return AnnouncementList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDAnnouncement_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateAnnouncement_API(Announcement Announcement)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Announcement), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudAnnounUrl}{Announcement.id}", jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateAnnouncement_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateAnnouncement_API(Announcement Announcement)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Announcement), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allAnnounUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateAnnouncement_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAnnouncement_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudAnnounUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteAnnouncement_API: {ex.Message}");
                return false;
            }
        }
    }
}
