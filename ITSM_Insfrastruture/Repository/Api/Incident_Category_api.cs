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
    public class Incident_Category_api
    {
        private readonly string _allIncidentcategoryUrl = Api_Link.IncidentCategoryLink;
        private readonly string _sudIncidentcategoryUrl = Api_Link.IncidentCategorySUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Incident_Category_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Incidentcategory>> GetAllIncidentcategory_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Incidentcategory>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allIncidentcategoryUrl);
                return JsonConvert.DeserializeObject<List<Incidentcategory>>(jsonStr) ?? new List<Incidentcategory>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllIncidentcategory_API: {ex.Message}");
                return new List<Incidentcategory>();
            }
        }

        //public async Task<Incidentcategory> FindByIDIncidentcategory_API(int id)
        //{
        //    try
        //    {
        //        var tokenModel = _tokenService.GetToken();
        //        if (tokenModel == null) return null;

        //        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
        //        var jsonStr = await _client.GetStringAsync($"{_sudIncidentcategoryUrl}{id}");
        //        var IncidentcategoryList = JsonConvert.DeserializeObject<List<Incidentcategory>>(jsonStr);

        //        //var responseStr = await response.Content.ReadAsStringAsync();
        //        //Console.WriteLine($"RESPONSE UPDATE: {responseStr}");

        //        return IncidentcategoryList?.FirstOrDefault();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"EX FindByIDIncidentcategory_API: {ex.Message}");
        //        return null;
        //    }
        //}

        public async Task<Incidentcategory> FindByIDIncidentcategory_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudIncidentcategoryUrl}{id}");

                var IncidentcategoryList = JsonConvert.DeserializeObject<List<Incidentcategory>>(jsonStr);
                return IncidentcategoryList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDIncidentcategory_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateIncidentcategory_API(Incidentcategory Incidentcategory)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Incidentcategory), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudIncidentcategoryUrl}{Incidentcategory.id}", jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateIncidentcategory_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateIncidentcategory_API(Incidentcategory Incidentcategory)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(Incidentcategory), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allIncidentcategoryUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateIncidentcategory_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteIncidentcategory_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudIncidentcategoryUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteIncidentcategory_API: {ex.Message}");
                return false;
            }
        }
    }
}
