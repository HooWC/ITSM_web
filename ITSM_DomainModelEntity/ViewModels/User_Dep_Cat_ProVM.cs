using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;

namespace ITSM_DomainModelEntity.ViewModels
{
    public class User_Dep_Cat_ProVM
    {
        public User? user {  get; set; }
        public List<Product>? products { get; set; }
        public List<Category>? categorys { get; set; }

    }
}
