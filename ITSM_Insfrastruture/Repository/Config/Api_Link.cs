using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_Insfrastruture.Repository.Config
{
    public class Api_Link
    {
        public static readonly string BaseUrl = "http://localhost:5300";
        public static readonly string AuthLink = $"{BaseUrl}/users/authenticate";
        public static readonly string RegisterLink = $"{BaseUrl}/users/register";
    }
}
