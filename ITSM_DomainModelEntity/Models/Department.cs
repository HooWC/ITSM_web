using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_DomainModelEntity.Models
{
    public class Department
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(100)]
        public string? name { get; set; }

        [StringLength(255)]
        public string? description { get; set; }

        public DateTime created_date { get; set; } = DateTime.Now;

        public DateTime? update_date { get; set; }
    }

}
