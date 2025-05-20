using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;

namespace ITSM_DomainModelEntity.ViewModels
{
    public class User_ReqVM
    {
        public User? user { get; set; }
        public List<Request>? requests { get; set; }
        public Request? req { get; set; }
    }
}
