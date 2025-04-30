using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ITSM_Insfrastruture.Repository.Config;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Http;

namespace ITSM_Insfrastruture.Repository.Api
{
    public class Auth_api
    {
        private readonly string _authUrl = Api_Link.AuthLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        public Auth_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<bool> LoginAsync(string emp_id, string username, string password)
        {
            try
            {
                // Check if there is a valid token
                if (_tokenService.IsTokenValid())
                {
                    return true;
                }

                // Api Value
                var loginData = new
                {
                    emp_id,
                    username,
                    password 
                };

                var content = new StringContent(JsonSerializer.Serialize(loginData),Encoding.UTF8,"application/json");

                var response = await _client.PostAsync(_authUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var authResult = JsonSerializer.Deserialize<AuthResponse>(jsonResponse);

                    if (authResult != null && !string.IsNullOrEmpty(authResult.token))
                    {
                        // Save Token
                        var tokenModel = new TokenModel
                        {
                            Token = authResult.token,
                            UserId = authResult.user.id,
                            Username = authResult.user.username,
                            EmpId = authResult.user.emp_id
                        };

                        _tokenService.SaveToken(tokenModel);
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Ex Error: {ex.Message}");
                return false;
            }
        }

        public string GetToken()
        {
            var tokenModel = _tokenService.GetToken();
            return tokenModel?.Token;
        }

        private class AuthResponse
        {
            public UserInfo user { get; set; }
            public string token { get; set; }

            public class UserInfo
            {
                public int id { get; set; }
                public string username { get; set; }
                public string emp_id { get; set; }
            }
        }
    }
}
