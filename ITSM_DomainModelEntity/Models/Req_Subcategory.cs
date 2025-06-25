using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Function;
using System.Xml.Linq;

namespace ITSM_DomainModelEntity.Models
{
    public class Req_Subcategory
    {
        [Key]
        public int id { get; set; }

        public string? name { get; set; }

        [ForeignKey("Req_Category")]
        public int? req_category_id { get; set; }

        // Navigation properties
        public virtual Req_Category? Req_Category { get; set; }
    }
}
