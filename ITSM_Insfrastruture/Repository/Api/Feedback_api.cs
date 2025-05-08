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
    public class Feedback_api
    {
        private readonly string _allFeedbackUrl = Api_Link.FeedbackLink;
        private readonly string _sudFeedbackUrl = Api_Link.FeedbackSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Feedback_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Feedback>> GetAllFeedback_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Feedback>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allFeedbackUrl);
                return JsonConvert.DeserializeObject<List<Feedback>>(jsonStr) ?? new List<Feedback>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllFeedback_API: {ex.Message}");
                return new List<Feedback>();
            }
        }

        public async Task<Feedback> FindByIDFeedback_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudFeedbackUrl}{id}");
                var feedbackList = JsonConvert.DeserializeObject<List<Feedback>>(jsonStr);
                return feedbackList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDFeedback_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateFeedback_API(Feedback feedback)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(feedback), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_sudFeedbackUrl}{feedback.id}", jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateFeedback_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateFeedback_API(Feedback feedback)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(feedback), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_allFeedbackUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateFeedback_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteFeedback_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudFeedbackUrl}{id}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteFeedback_API: {ex.Message}");
                return false;
            }
        }
    }
}
