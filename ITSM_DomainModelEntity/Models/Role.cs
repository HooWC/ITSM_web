using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_DomainModelEntity.Models
{
    public class Role
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(50)]
        public string? role { get; set; }
    }
}
