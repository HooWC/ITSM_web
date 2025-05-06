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
using Newtonsoft.Json.Serialization;

namespace ITSM_Insfrastruture.Repository.Api
{
    public class Todo_api
    {
        private readonly string _allTodoUrl = Api_Link.TodoLink;
        private readonly string _sudTodoUrl = Api_Link.TodoSUDLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Todo_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<List<Todo>> GetAllTodo_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<Todo>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allTodoUrl);
                return JsonConvert.DeserializeObject<List<Todo>>(jsonStr) ?? new List<Todo>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllTodo_API: {ex.Message}");
                return new List<Todo>();
            }
        }

        public async Task<Todo> FindByIDTodo_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_sudTodoUrl}{id}");
                return JsonConvert.DeserializeObject<Todo>(jsonStr);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDTodo_API: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateTodo_API(Todo todo)
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
                    JsonConvert.SerializeObject(todo, jsonSettings), 
                    Encoding.UTF8, 
                    "application/json"
                );
                
                var response = await _client.PutAsync($"{_sudTodoUrl}{todo.id}", jsonStr);
                //var responseStr = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"RESPONSE UPDATE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateTodo_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CreateTodo_API(Todo todo)
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
                    JsonConvert.SerializeObject(todo, jsonSettings), 
                    Encoding.UTF8, 
                    "application/json"
                );
                
                var response = await _client.PostAsync(_allTodoUrl, jsonStr);

                // Testing Date Error
                // var responseStr = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX CreateTodo_API: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTodo_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var response = await _client.DeleteAsync($"{_sudTodoUrl}{id}");

                // Testing Debug
                // var responseStr = await response.Content.ReadAsStringAsync();
                // Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX DeleteTodo_API: {ex.Message}");
                return false;
            }
        }
    }
}
