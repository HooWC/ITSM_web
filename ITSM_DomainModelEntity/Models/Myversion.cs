using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_DomainModelEntity.Models
{
    public class Myversion
    {
        [Key]
        public int id { get; set; }

        [Required]
        public string? version_num { get; set; }

        public DateTime release_date { get; set; }

        public string? message { get; set; }

        
    }
}
