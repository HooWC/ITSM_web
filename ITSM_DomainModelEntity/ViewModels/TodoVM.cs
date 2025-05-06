using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;

namespace ITSM_DomainModelEntity.ViewModels
{
    public class TodoVM
    {
        public User User { get; set; }
        public List<Todo> Todo { get; set; }
    }
}
