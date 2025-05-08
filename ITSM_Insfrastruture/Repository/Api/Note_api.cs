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
    public class Note_api
    {
        private readonly string _allNoteUrl = Api_Link.NoteLink;
        private readonly string _sudNoteUrl = Api_Link.NoteSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Note_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Note>> GetAllNote_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Note>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allNoteUrl);
                return JsonConvert.DeserializeObject<List<Note>>(jsonStr) ?? new List<Note>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllNote_API: {ex.Message}");
                return new List<Note>();
            }
        }

        public async Task<Note> FindByIDNote_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudNoteUrl}{id}");
                var noteList = JsonConvert.DeserializeObject<List<Note>>(jsonStr);
                return noteList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDNote_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateNote_API(Note note)
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
                    JsonConvert.SerializeObject(note, jsonSettings),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PutAsync($"{_sudNoteUrl}{note.id}", jsonStr);
                //var responseStr = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"RESPONSE UPDATE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateNote_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateNote_API(Note note)
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
                    JsonConvert.SerializeObject(note, jsonSettings),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _client.PostAsync(_allNoteUrl, jsonStr);

                // Testing Date Error
                // var responseStr = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateNote_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteNote_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudNoteUrl}{id}");

                // Testing Debug
                // var responseStr = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteNote_API: {ex.Message}");
                return false;
            }
        }
    }
}
