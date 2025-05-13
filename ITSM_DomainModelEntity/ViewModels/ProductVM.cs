using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;

namespace ITSM_DomainModelEntity.ViewModels
{
    public class ProductVM
    {
        public User? User { get; set; }
        public List<Product>? Product { get; set; }
    }
}
