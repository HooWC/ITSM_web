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

        /// <summary>
        /// 保存用户令牌到会话和持久Cookie
        /// </summary>
        public void SaveToken(TokenModel tokenModel)
        {
            if (_httpContextAccessor.HttpContext != null)
            {
                try
                {
                    var tokenJson = JsonSerializer.Serialize(tokenModel);
                    Console.WriteLine($"正在保存令牌: {tokenJson}");
                    
                    // 保存到Session
                    try
                    {
                        if (!_httpContextAccessor.HttpContext.Session.IsAvailable)
                        {
                            _httpContextAccessor.HttpContext.Session.Set("SessionTest", new byte[] { 1 });
                        }
                        _httpContextAccessor.HttpContext.Session.SetString(TokenKey, tokenJson);
                        Console.WriteLine("令牌已保存到Session");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"保存令牌到Session时出错: {ex.Message}");
                    }
                    
                    // 保存到持久Cookie
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
                        Console.WriteLine("令牌已保存到持久Cookie");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"保存令牌到Cookie时出错: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"令牌序列化时出错: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("无法保存令牌，HttpContext为null");
            }
        }

        /// <summary>
        /// 从会话或持久Cookie获取令牌
        /// </summary>
        public TokenModel GetToken()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                Console.WriteLine("无法获取令牌，HttpContext为null");
                return null;
            }
            
            // 首先尝试从Session获取
            try
            {
                var sessionToken = GetTokenFromSession();
                if (sessionToken != null)
                {
                    Console.WriteLine("从Session中获取到令牌");
                    return sessionToken;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从Session获取令牌时出错: {ex.Message}");
            }
            
            // 如果Session中没有，尝试从Cookie获取
            try
            {
                var cookieToken = GetTokenFromCookie();
                if (cookieToken != null)
                {
                    Console.WriteLine("从Cookie中获取到令牌");
                    // 发现cookie中有令牌但session中没有，自动同步到session
                    try
                    {
                        if (_httpContextAccessor.HttpContext.Session != null)
                        {
                            var tokenJson = JsonSerializer.Serialize(cookieToken);
                            _httpContextAccessor.HttpContext.Session.SetString(TokenKey, tokenJson);
                            Console.WriteLine("已将Cookie令牌同步到Session");
                        }
                    }
                    catch { /* 忽略同步错误 */ }
                    
                    return cookieToken;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从Cookie获取令牌时出错: {ex.Message}");
            }
            
            Console.WriteLine("没有找到有效的令牌");
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

        /// <summary>
        /// 检查令牌是否有效
        /// </summary>
        public bool IsTokenValid()
        {
            try
            {
                var token = GetToken();
                
                if (token != null && !string.IsNullOrEmpty(token.Token))
                {
                    Console.WriteLine($"发现有效令牌，用户名: {token.Username}");
                    return true;
                }
                
                Console.WriteLine("没有找到有效令牌");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"验证令牌时发生错误: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 清除令牌
        /// </summary>
        public void ClearToken()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                Console.WriteLine("无法清除令牌，HttpContext为null");
                return;
            }
            
            try
            {
                // 清除Session中的令牌
                if (_httpContextAccessor.HttpContext.Session != null)
                {
                    _httpContextAccessor.HttpContext.Session.Remove(TokenKey);
                    Console.WriteLine("已清除Session中的令牌");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清除Session中的令牌时出错: {ex.Message}");
            }
            
            try
            {
                // 清除Cookie中的令牌
                _httpContextAccessor.HttpContext.Response.Cookies.Delete(CookieTokenKey);
                Console.WriteLine("已清除Cookie中的令牌");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"清除Cookie中的令牌时出错: {ex.Message}");
            }
        }
    }
} 