using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;

namespace ITSM_DomainModelEntity.ViewModels
{
    public class User_Dep_RoleVM
    {
        public User? user { get; set; }
        public List<Role>? role { get; set; }
        public List<Department>? department { get; set; }
    }
}
