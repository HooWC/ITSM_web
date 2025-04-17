using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_DomainModelEntity.Models
{
    public class Category
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(100)]
        public string? title { get; set; }

        [StringLength(255)]
        public string? description { get; set; }
    }
}
