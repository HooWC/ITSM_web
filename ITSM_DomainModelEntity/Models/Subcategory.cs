using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_DomainModelEntity.Models
{
    public class Subcategory
    {
        [Key]
        public int id { get; set; } 

        public string? subcategory { get; set; }

        [ForeignKey("Incidentcategory")]
        public int category { get; set; }

        [ForeignKey("Department")]
        public int department_id { get; set; }

        // Navigation properties
        public virtual Incidentcategory? Incidentcategory { get; set; }
        public virtual Department? Department { get; set; }
    }
}
