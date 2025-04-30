using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_Insfrastruture.Repository.Config
{
    public class Api_Link
    {
        // Base Api Link
        public static readonly string BaseUrl = "http://localhost:5300";
        // login
        public static readonly string AuthLink = $"{BaseUrl}/users/authenticate";
        // register
        public static readonly string RegisterLink = $"{BaseUrl}/users/register";
        // get all user
        public static readonly string AllUserLink = $"{BaseUrl}/users";
        // get user by id && update user  (ID)
        public static readonly string UserFindIDLink = $"{BaseUrl}/users/";
    }
}
