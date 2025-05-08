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
    public class Incident_api
    {
        private readonly string _allIncUrl = Api_Link.IncidentLink;
        private readonly string _sudIncUrl = Api_Link.IncidentSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Incident_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Incident>> GetAllIncident_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Incident>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allIncUrl);
                return JsonConvert.DeserializeObject<List<Incident>>(jsonStr) ?? new List<Incident>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllIncident_API: {ex.Message}");
                return new List<Incident>();
            }
        }

        public async Task<Incident> FindByIDIncident_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudIncUrl}{id}");
                var incidentList = JsonConvert.DeserializeObject<List<Incident>>(jsonStr);
                return incidentList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDIncident_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateIncident_API(Incident incident)
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
                    JsonConvert.SerializeObject(incident, jsonSettings),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PutAsync($"{_sudIncUrl}{incident.id}", jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateIncident_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateIncident_API(Incident incident)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(incident), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allIncUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateIncident_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteIncident_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudIncUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteIncident_API: {ex.Message}");
                return false;
            }
        }
    }
}
