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
        private readonly string _allUserUrl = Api_Link.AllUserLink;
        private readonly string _F_U_UserUrl = Api_Link.User_F_U_Link;
        private readonly string _UserForgotPasswordUrl = Api_Link.UserForgotPasswordLink;
        private readonly string _UserNoTokenUrl = Api_Link.UserNoTokenLink;
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
                var jsonStr = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

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

                            // Register success then save user infomation
                            await FetchAndSaveUserInfo(registerResult.user.id);

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
            public string? message { get; set; }
        }
        // == Register ==

        public async Task<List<User>> GetAllUser_API()
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return new List<User>();

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                string jsonStr = await _client.GetStringAsync(_allUserUrl);
                return JsonConvert.DeserializeObject<List<User>>(jsonStr) ?? new List<User>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX GetAllUser_API: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<User> FindByIDUser_API(int id)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return null;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = await _client.GetStringAsync($"{_F_U_UserUrl}{id}");
                var userList = JsonConvert.DeserializeObject<List<User>>(jsonStr);
                return userList?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX FindByIDUser_API: {ex.Message}");
                return null;
            }
        }

        //public async Task<bool> UpdateUser_API(User user)
        //{
        //    try
        //    {
        //        var tokenModel = _tokenService.GetToken();
        //        if (tokenModel == null) return false;

        //        var originalUser = await FindByIDUser_API(user.id);
        //        if (originalUser == null) return false;

        //        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);

        //        bool excludePassword = user.password == null;
        //        bool excludeUsername = user.username == originalUser.username;

        //        object userToSend;
        //        if (excludePassword || excludeUsername)
        //        {
        //            var userDict = new Dictionary<string, object>
        //            {
        //                { "id", user.id },
        //                { "emp_id", user.emp_id },
        //                { "prefix", user.prefix },
        //                { "fullname", user.fullname },
        //                { "email", user.email },
        //                { "gender", user.gender },
        //                { "department_id", user.department_id },
        //                { "title", user.title },
        //                { "mobile_phone", user.mobile_phone },
        //                { "role_id", user.role_id },
        //                { "race", user.race },
        //                { "active", user.active },
        //                { "photo_type", user.photo_type ?? "" }
        //            };

        //            if (user.business_phone != null)
        //                userDict["business_phone"] = user.business_phone;

        //            if (user.update_date.HasValue)
        //                userDict["update_date"] = user.update_date;

        //            if (user.create_date != default(DateTime))
        //                userDict["create_date"] = user.create_date;

        //            if (!excludeUsername)
        //                userDict["username"] = user.username;

        //            if (!excludePassword && user.password != null)
        //                userDict["password"] = user.password;

        //            if (user.photo != null)
        //            {
        //                // 转换photo为Base64字符串
        //                userDict["photo"] = Convert.ToBase64String(user.photo);
        //            }

        //            userToSend = userDict;
        //        }
        //        else
        //        {
        //            // 创建一个新对象并转换photo为Base64字符串
        //            var userDict = new Dictionary<string, object>
        //            {
        //                { "id", user.id },
        //                { "emp_id", user.emp_id },
        //                { "username", user.username },
        //                { "prefix", user.prefix },
        //                { "fullname", user.fullname },
        //                { "email", user.email },
        //                { "gender", user.gender },
        //                { "department_id", user.department_id },
        //                { "title", user.title },
        //                { "mobile_phone", user.mobile_phone },
        //                { "role_id", user.role_id },
        //                { "race", user.race },
        //                { "active", user.active },
        //                { "photo_type", user.photo_type ?? "" }
        //            };

        //            if (user.business_phone != null)
        //                userDict["business_phone"] = user.business_phone;

        //            if (user.update_date.HasValue)
        //                userDict["update_date"] = user.update_date;

        //            if (user.create_date != default(DateTime))
        //                userDict["create_date"] = user.create_date;

        //            if (user.password != null)
        //                userDict["password"] = user.password;

        //            if (user.photo != null)
        //            {
        //                userDict["photo"] = Convert.ToBase64String(user.photo);
        //            }

        //            userToSend = userDict;
        //        }

        //        Console.WriteLine($"Photo as Base64: {Convert.ToBase64String(user.photo).Substring(0, 50)}...");

        //        //{[photo_type, image/webp]}
        //        var jsonStr = new StringContent(JsonConvert.SerializeObject(userToSend), Encoding.UTF8, "application/json");
        //        var response = await _client.PutAsync($"{_F_U_UserUrl}{user.id}", jsonStr);

        //        var responseStr = await response.Content.ReadAsStringAsync();
        //        Console.WriteLine($"RESPONSE: {responseStr}");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            // Update User Session Infomation
        //            if (tokenModel.UserId == user.id)
        //            {
        //                // 获取最新的用户信息，包括photo字段
        //                var updatedUser = await FindByIDUser_API(user.id);
        //                if (updatedUser != null)
        //                {
        //                    _tokenService.SaveUserInfo(updatedUser);
        //                }
        //                else
        //                {
        //                    _tokenService.SaveUserInfo(user);
        //                }
        //            }
        //            return true;
        //        }

        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"EX UpdateUser_API: {ex.Message}");
        //        return false;
        //    }
        //}


        public async Task<bool> UpdateUser_API(User user)
        {
            try
            {
                var tokenModel = _tokenService.GetToken();
                if (tokenModel == null) return false;

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenModel.Token);
                var jsonStr = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                var response = await _client.PutAsync($"{_F_U_UserUrl}{user.id}", jsonStr);

                //var responseStr = await response.Content.ReadAsStringAsync();
                //Console.WriteLine($"RESPONSE: {responseStr}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX UpdateUser_API: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> FetchAndSaveUserInfo(int userId)
        {
            try
            {
                var token = _tokenService.GetToken();
                if (token == null || string.IsNullOrEmpty(token.Token))
                {
                    Console.WriteLine("Failed to get user information: No valid token");
                    return false;
                }

                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);

                var user = await FindByIDUser_API(userId);

                if (user != null)
                {
                    // Save User Information Session && Cookie
                    _tokenService.SaveUserInfo(user);
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to obtain information of user ID={userId}");
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX Save Info Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VerifyUserWithoutToken(User user)
        {
            try
            {
                var userVerificationData = new
                {
                    emp_id = user.emp_id,
                    username = user.username
                };

                var jsonStr = new StringContent(JsonConvert.SerializeObject(userVerificationData), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_UserNoTokenUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX VerifyUserWithoutToken: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResetForgotPassword(User user)
        {
            try
            {
                var passwordResetData = new
                {
                    emp_id = user.emp_id,
                    username = user.username,
                    password = user.password
                };

                var jsonStr = new StringContent(JsonConvert.SerializeObject(passwordResetData), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(_UserForgotPasswordUrl, jsonStr);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EX ResetForgotPassword: {ex.Message}");
                return false;
            }
        }

        public User GetCurrentUser()
        {
            // Get User Current Data
            return _tokenService.GetUserInfo();
        }
    }

    public class RegisterResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
