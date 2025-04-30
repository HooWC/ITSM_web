using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Threading.Tasks;

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
        private const string CookieTokenKey = "AuthTokenCookie";
        private const int CookieDaysValid = 365; // Cookie有效期（天）

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
                // Console.WriteLine("Cannot get Token，HttpContext为null");
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
                // Console.WriteLine("Token in Cookie Cleared");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing token from cookie: {ex.Message}");
            }
        }
    }
} 