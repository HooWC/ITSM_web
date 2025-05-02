using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;

namespace ITSM_Insfrastruture.Repository.Token
{
    public class TokenModel
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string EmpId { get; set; }
    }

    public class TokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string TokenKey = "UserToken";
        private const string UserInfoKey = "UserInfo";
        private const string CookieTokenKey = "AuthTokenCookie";
        private const string CookieUserInfoKey = "UserInfoCookie";
        private const int CookieDaysValid = 36500; // 100-year validity period

        public TokenService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SaveToken(TokenModel tokenModel)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                try
                {
                    var tokenJson = JsonSerializer.Serialize(tokenModel);
                    // Console.WriteLine($"Token Json: {tokenJson}");
                    
                    // Save To Session
                    try
                    {
                        if (!_httpContextAccessor.HttpContext.Session.IsAvailable)
                        {
                            _httpContextAccessor.HttpContext.Session.Set("SessionTest", new byte[] { 1 });
                        }
                        _httpContextAccessor.HttpContext.Session.SetString(TokenKey, tokenJson);
                        // Console.WriteLine("Token Save To Session");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Token Save To Session Error: {ex.Message}");
                    }
                    
                    // Save To Cookie
                    try
                    {
                        _httpContextAccessor.HttpContext.Response.Cookies.Append(
                            CookieTokenKey,
                            tokenJson,
                            new CookieOptions
                            {
                                Expires = DateTime.Now.AddDays(CookieDaysValid),
                                HttpOnly = true,
                                Secure = _httpContextAccessor.HttpContext.Request.IsHttps,
                                SameSite = SameSiteMode.Lax,
                                IsEssential = true
                            });
                        // Console.WriteLine("Token Save To Cookie");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Token Save To Cookie Error: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while serializing token: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Cannot Save Token，HttpContext is null");
            }
        }

        public TokenModel GetToken()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                // Console.WriteLine("Cannot get Token，HttpContext is null");
                return null;
            }
            
            // Try Session get
            try
            {
                var sessionToken = GetTokenFromSession();
                if (sessionToken != null)
                {
                    // Console.WriteLine("Session get Token");
                    return sessionToken;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Session get Token Error: {ex.Message}");
            }
            
            // Try Cookie get
            try
            {
                var cookieToken = GetTokenFromCookie();
                if (cookieToken != null)
                {
                    // Console.WriteLine("Cookie get Token");
                    try
                    {
                        if (_httpContextAccessor.HttpContext.Session != null)
                        {
                            var tokenJson = JsonSerializer.Serialize(cookieToken);
                            _httpContextAccessor.HttpContext.Session.SetString(TokenKey, tokenJson);
                            // Console.WriteLine("The cookie token has been synchronized to the Session");
                        }
                    }
                    catch { /* Ignore sync errors */ }
                    
                    return cookieToken;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting token from cookie: {ex.Message}");
            }
            
            Console.WriteLine("No valid token found");
            return null;
        }

        private TokenModel GetTokenFromSession()
        {
            if (_httpContextAccessor.HttpContext?.Session == null || !_httpContextAccessor.HttpContext.Session.IsAvailable)
            {
                return null;
            }
            
            var tokenJson = ITSM_Insfrastruture.Repository.Token.SessionExtensions.GetString(
                _httpContextAccessor.HttpContext.Session, TokenKey);
                
            if (!string.IsNullOrEmpty(tokenJson))
            {
                try
                {
                    return JsonSerializer.Deserialize<TokenModel>(tokenJson);
                }
                catch
                {
                    return null;
                }
            }
            
            return null;
        }

        private TokenModel GetTokenFromCookie()
        {
            if (_httpContextAccessor.HttpContext?.Request?.Cookies == null)
            {
                return null;
            }
            
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(CookieTokenKey, out var tokenJson) && 
                !string.IsNullOrEmpty(tokenJson))
            {
                try
                {
                    return JsonSerializer.Deserialize<TokenModel>(tokenJson);
                }
                catch
                {
                    return null;
                }
            }
            
            return null;
        }

        public bool IsTokenValid()
        {
            try
            {
                var token = GetToken();
                
                if (token != null && !string.IsNullOrEmpty(token.Token))
                {
                    // Console.WriteLine($"Found a valid token, username: {token.Username}");
                    return true;
                }

                // Console.WriteLine("No valid token found");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while validating the token: {ex.Message}");
                return false;
            }
        }

        public void ClearToken()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                Console.WriteLine("Unable to clear token，HttpContext is null");
                return;
            }
            
            try
            {
                // Clear the token in the Session
                if (_httpContextAccessor.HttpContext.Session != null)
                {
                    _httpContextAccessor.HttpContext.Session.Remove(TokenKey);
                    _httpContextAccessor.HttpContext.Session.Remove(UserInfoKey);
                    // Console.WriteLine("The token in the session has been cleared");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing token from Session: {ex.Message}");
            }
            
            try
            {
                // Clear the token in the cookie
                _httpContextAccessor.HttpContext.Response.Cookies.Delete(CookieTokenKey);
                _httpContextAccessor.HttpContext.Response.Cookies.Delete(CookieUserInfoKey);
                // Console.WriteLine("Token in Cookie Cleared");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing token from cookie: {ex.Message}");
            }
        }

        // Save complete user information
        public void SaveUserInfo(User userInfo)
        {
            if (_httpContextAccessor.HttpContext != null && userInfo != null)
            {
                try
                {
                    var userInfoJson = JsonSerializer.Serialize(userInfo);
                    
                    // Save to Session
                    try
                    {
                        if (!_httpContextAccessor.HttpContext.Session.IsAvailable)
                        {
                            _httpContextAccessor.HttpContext.Session.Set("SessionTest", new byte[] { 1 });
                        }
                        _httpContextAccessor.HttpContext.Session.SetString(UserInfoKey, userInfoJson);
                        // Console.WriteLine("User info saved to Session");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving user info to Session: {ex.Message}");
                    }
                    
                    // Save to Cookie
                    try
                    {
                        _httpContextAccessor.HttpContext.Response.Cookies.Append(
                            CookieUserInfoKey,
                            userInfoJson,
                            new CookieOptions
                            {
                                Expires = DateTime.Now.AddDays(CookieDaysValid),
                                HttpOnly = true,
                                Secure = _httpContextAccessor.HttpContext.Request.IsHttps,
                                SameSite = SameSiteMode.Lax,
                                IsEssential = true
                            });
                        // Console.WriteLine("User info saved to Cookie");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving user info to Cookie: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error serializing user info: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Cannot save user info, HttpContext is null or user info is null");
            }
        }

        // Get current user's complete information
        public User GetUserInfo()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                Console.WriteLine("Cannot get user info, HttpContext is null");
                return null;
            }
            
            // Try to get from Session
            try
            {
                var userInfo = GetUserInfoFromSession();
                if (userInfo != null)
                {
                    // Console.WriteLine("Successfully retrieved user info from Session");
                    return userInfo;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user info from Session: {ex.Message}");
            }
            
            // Try to get from Cookie
            try
            {
                var userInfo = GetUserInfoFromCookie();
                if (userInfo != null)
                {
                    // Console.WriteLine("Successfully retrieved user info from Cookie");
                    
                    // Sync to Session
                    try
                    {
                        if (_httpContextAccessor.HttpContext.Session != null)
                        {
                            var userInfoJson = JsonSerializer.Serialize(userInfo);
                            _httpContextAccessor.HttpContext.Session.SetString(UserInfoKey, userInfoJson);
                            // Console.WriteLine("User info from Cookie synchronized to Session");
                        }
                    }
                    catch { /* Ignore sync errors */ }
                    
                    return userInfo;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user info from Cookie: {ex.Message}");
            }
            
            Console.WriteLine("No valid user information found");
            return null;
        }

        private User GetUserInfoFromSession()
        {
            if (_httpContextAccessor.HttpContext?.Session == null || !_httpContextAccessor.HttpContext.Session.IsAvailable)
            {
                return null;
            }
            
            var userInfoJson = ITSM_Insfrastruture.Repository.Token.SessionExtensions.GetString(
                _httpContextAccessor.HttpContext.Session, UserInfoKey);
                
            if (!string.IsNullOrEmpty(userInfoJson))
            {
                try
                {
                    return JsonSerializer.Deserialize<User>(userInfoJson);
                }
                catch
                {
                    return null;
                }
            }
            
            return null;
        }

        private User GetUserInfoFromCookie()
        {
            if (_httpContextAccessor.HttpContext?.Request?.Cookies == null)
            {
                return null;
            }
            
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(CookieUserInfoKey, out var userInfoJson) && 
                !string.IsNullOrEmpty(userInfoJson))
            {
                try
                {
                    return JsonSerializer.Deserialize<User>(userInfoJson);
                }
                catch
                {
                    return null;
                }
            }
            
            return null;
        }
    }
} 