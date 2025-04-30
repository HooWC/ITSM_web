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
    public class User_api
    {
        private readonly string _registerUrl = Api_Link.RegisterLink;
        private readonly HttpClient _client;
        private readonly TokenService _tokenService;

        // == Register ==
        public User_api(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _tokenService = new TokenService(httpContextAccessor);
        }

        public async Task<RegisterResult> Register_User(User user)
        {
            try
            {
                var jsonStr = new StringContent(JsonConvert.SerializeObject(user),Encoding.UTF8,"application/json");

                // Output request content for debugging
                // Console.WriteLine($"Json Content: {await jsonStr.ReadAsStringAsync()}");

                var response = await _client.PostAsync(_registerUrl, jsonStr);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var registerResult = JsonConvert.DeserializeObject<AuthResponse>(responseContent);

                        if (registerResult != null && !string.IsNullOrEmpty(registerResult.token))
                        {
                            // Save Token
                            var tokenModel = new TokenModel
                            {
                                Token = registerResult.token,
                                UserId = registerResult.user.id,
                                Username = registerResult.user.username,
                                EmpId = registerResult.user.emp_id
                            };

                            _tokenService.SaveToken(tokenModel);
                            return new RegisterResult { Success = true };
                        }
                        else
                        {
                            return new RegisterResult 
                            { 
                                Success = false, 
                                Message = "Registration was successful but no valid token was received"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ex Error: {ex.Message}");
                        return new RegisterResult 
                        { 
                            Success = false, 
                            Message = "Unable to parse server response"
                        };
                    }
                }

                // Handling registration failures
                string errorMessage = "Registration failed";
                
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                        errorMessage = errorResponse?.message ?? "Invalid request";
                    }
                    catch
                    {
                        errorMessage = $"Invalid request: {responseContent}";
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    errorMessage = "Username or employee ID already exists";
                }
                
                return new RegisterResult 
                { 
                    Success = false, 
                    Message = errorMessage 
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ex Message: {ex.Message}");
                return new RegisterResult 
                { 
                    Success = false, 
                    Message = $"Register Ex Error: {ex.Message}" 
                };
            }
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

        private class ErrorResponse
        {
            public string message { get; set; }
        }

        // == Register ==
    }

    public class RegisterResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
