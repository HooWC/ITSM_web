using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;
using ITSM_Insfrastruture.Repository.Config;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ITSM_Insfrastruture.Repository.Api
{
    public class Knowledge_api
    {
        private readonly string _allKnowledgeUrl = Api_Link.KnowledgeLink;
        private readonly string _sudKnowledgeUrl = Api_Link.KnowledgeSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Knowledge_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Knowledge>> GetAllKnowledge_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Knowledge>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allKnowledgeUrl);
                return JsonConvert.DeserializeObject<List<Knowledge>>(jsonStr) ?? new List<Knowledge>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllKnowledge_API: {ex.Message}");
                return new List<Knowledge>();
            }
        }

        public async Task<Knowledge> FindByIDKnowledge_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudKnowledgeUrl}{id}");
                var knowledgeList = JsonConvert.DeserializeObject<List<Knowledge>>(jsonStr);
                return knowledgeList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDKnowledge_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateKnowledge_API(Knowledge knowledge)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(knowledge), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudKnowledgeUrl}{knowledge.id}", jsonStr);

                //var responseStr = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateKnowledge_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateKnowledge_API(Knowledge knowledge)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(knowledge), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allKnowledgeUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateKnowledge_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteKnowledge_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudKnowledgeUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteKnowledge_API: {ex.Message}");
                return false;
            }
        }
    }
}
