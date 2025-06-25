using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Function;

namespace ITSM_DomainModelEntity.Models
{
    public class Req_Category
    {
        [Key]
        public int id { get; set; }

        public string? name { get; set; }

        public string? erp_version { get; set; }
    }
}
