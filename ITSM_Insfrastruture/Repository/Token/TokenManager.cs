//using System;
//using System.IO;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace ITSM_Insfrastruture.Repository.Token
//{
//    public class TokenModel
//    {
//        public string Token { get; set; }
//        public int UserId { get; set; }
//        public string Username { get; set; }
//        public string EmpId { get; set; }
//    }

//    public class TokenManager
//    {
//        private static readonly string TokenFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.json");
//        private static TokenModel _cachedToken;

//        public static async Task SaveTokenAsync(TokenModel tokenModel)
//        {
//            _cachedToken = tokenModel;
//            var json = JsonSerializer.Serialize(tokenModel);
//            await File.WriteAllTextAsync(TokenFilePath, json);
//        }

//        public static async Task<TokenModel> GetTokenAsync()
//        {
//            if (_cachedToken != null)
//            {
//                return _cachedToken;
//            }

//            if (File.Exists(TokenFilePath))
//            {
//                try
//                {
//                    var json = await File.ReadAllTextAsync(TokenFilePath);
//                    var token = JsonSerializer.Deserialize<TokenModel>(json);
                    
//                    if (token != null)
//                    {
//                        _cachedToken = token;
//                        return token;
//                    }
//                }
//                catch (Exception)
//                {
//                    return null;
//                }
//            }

//            return null;
//        }

//        public static bool IsTokenValid()
//        {
//            if (_cachedToken != null)
//            {
//                return true;
//            }

//            if (File.Exists(TokenFilePath))
//            {
//                try
//                {
//                    var json = File.ReadAllText(TokenFilePath);
//                    var token = JsonSerializer.Deserialize<TokenModel>(json);
                    
//                    if (token != null)
//                    {
//                        _cachedToken = token;
//                        return true;
//                    }
//                }
//                catch (Exception)
//                {
//                    return false;
//                }
//            }

//            return false;
//        }

//        public static void ClearToken()
//        {
//            _cachedToken = null;
//            if (File.Exists(TokenFilePath))
//            {
//                File.Delete(TokenFilePath);
//            }
//        }
//    }
//} 