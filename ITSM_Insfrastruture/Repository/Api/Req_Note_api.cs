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
    public class Req_Note_api
    {
        private readonly string _allReq_NoteUrl = Api_Link.ReqNotesLink;
        private readonly string _sudReq_NoteUrl = Api_Link.ReqNotesSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Req_Note_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Req_Note>> GetAllReq_Note_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Req_Note>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allReq_NoteUrl);
                return JsonConvert.DeserializeObject<List<Req_Note>>(jsonStr) ?? new List<Req_Note>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllReq_Note_API: {ex.Message}");
                return new List<Req_Note>();
            }
        }

        public async Task<Req_Note> FindByIDReq_Note_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync($"{_sudReq_NoteUrl}{id}");

                var Req_NoteList = JsonConvert.DeserializeObject<List<Req_Note>>(jsonStr);
                return Req_NoteList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDReq_Note_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateReq_Note_API(Req_Note Req_Note)
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
                    JsonConvert.SerializeObject(Req_Note, jsonSettings),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PutAsync($"{_sudReq_NoteUrl}{Req_Note.id}", jsonStr);
                //var responseStr = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"RESPONSE UPDATE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateReq_Note_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateReq_Note_API(Req_Note Req_Note)
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
                    JsonConvert.SerializeObject(Req_Note, jsonSettings),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PostAsync(_allReq_NoteUrl, jsonStr);

                // Testing Date Error
                // var responseStr = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateReq_Note_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteReq_Note_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudReq_NoteUrl}{id}");

                // Testing Debug
                // var responseStr = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteReq_Note_API: {ex.Message}");
                return false;
            }
        }
    }
}
