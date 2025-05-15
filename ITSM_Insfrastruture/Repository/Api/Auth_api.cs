using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;
using ITSM_Insfrastruture.Repository.Config;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ITSM_Insfrastruture.Repository.Api
{
    public class Auth_api
    {
        private readonly string _authUrl = Api_Link.AuthLink;
        private readonly string _currentUserUrl = Api_Link.UserCurrentLink;
        private readonly string _F_U_UserUrl = Api_Link.User_F_U_Link;
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
                if (_tokenService.IsTokenValid())
                {
                    var userInfo = _tokenService.GetUserInfo();
                    if (userInfo == null)
                    {
                        await FetchAndSaveCurrentUserInfo();
                    }
                    return true;
                }

                var loginData = new { emp_id, username, password };
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_authUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var authResult = System.Text.Json.JsonSerializer.Deserialize<AuthResponse>(jsonResponse);

                    if (authResult != null && !string.IsNullOrEmpty(authResult.token))
                    {
                        var tokenModel = new TokenModel
                        {
                            Token = authResult.token,
                            UserId = authResult.user.id,
                            Username = authResult.user.username,
                            EmpId = authResult.user.emp_id
                        };

                        _tokenService.SaveToken(tokenModel);
                        await FetchAndSaveCurrentUserInfo();
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> FetchAndSaveCurrentUserInfo()
        {
            try
            {
                var token = _tokenService.GetToken();
                if (token == null || string.IsNullOrEmpty(token.Token))
                    return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                // Try to get user info by ID first
                var userFindUrl = $"{_F_U_UserUrl}{token.UserId}";
                var response = await _client.GetAsync(userFindUrl);

                if (!response.IsSuccessStatusCode)
                {
                    // Fallback to current user endpoint
                    response = await _client.GetAsync(_currentUserUrl);
                    if (!response.IsSuccessStatusCode)
                        return false;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Handle both array and single object responses
                if (jsonResponse.TrimStart().StartsWith("["))
                {
                    var apiUsers = JsonConvert.DeserializeObject<dynamic[]>(jsonResponse);
                    if (apiUsers != null && apiUsers.Length > 0)
                    {
                        var user = ConvertApiResponseToUser(apiUsers[0]);
                        _tokenService.SaveUserInfo(user);
                        return true;
                    }
                }
                else
                {
                    var apiUser = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                    if (apiUser != null)
                    {
                        var user = ConvertApiResponseToUser(apiUser);
                        _tokenService.SaveUserInfo(user);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching user info: {ex.Message}");
                return false;
            }
        }

        // Convert API response to User model
        private User ConvertApiResponseToUser(dynamic apiUser)
        {
            var user = new User
            {
                id = apiUser.id,
                emp_id = apiUser.emp_id,
                prefix = apiUser.prefix,
                fullname = apiUser.fullname,
                email = apiUser.email,
                gender = apiUser.gender,
                department_id = apiUser.department_id,
                title = apiUser.title,
                business_phone = apiUser.business_phone,
                mobile_phone = apiUser.mobile_phone,
                role_id = apiUser.role_id,
                username = apiUser.username,
                password = apiUser.password,
                race = apiUser.race,
                update_date = apiUser.update_date,
                create_date = apiUser.create_date ?? DateTime.Now,
                active = apiUser.active,
                photo = apiUser.photo,
                photo_type = apiUser.photo_type
            };

            return user;
        }

        public string GetToken()
        {
            var tokenModel = _tokenService.GetToken();
            return tokenModel?.Token;
        }

        public User GetCurrentUser()
        {
            return _tokenService.GetUserInfo();
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
