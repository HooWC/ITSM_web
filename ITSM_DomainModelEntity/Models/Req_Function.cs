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
    public class Req_Function
    {
        [Key]
        public int id { get; set; }

        public string? name { get; set; }

        [ForeignKey("Req_Subcategory")]
        public int req_subcategory_id { get; set; }

        // Navigation properties
        public virtual Req_Subcategory? Req_Subcategory { get; set; }
    }
}
