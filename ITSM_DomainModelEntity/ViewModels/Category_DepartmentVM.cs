using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;

namespace ITSM_DomainModelEntity.ViewModels
{
    public class Category_DepartmentVM
    {
        public List<Category>? category {  get; set; }
        public List<Department>? department { get; set; }
    }
}
