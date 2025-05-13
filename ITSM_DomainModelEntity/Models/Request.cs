using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_DomainModelEntity.Models
{
    public class Request
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(20)]
        public string? req_id { get; set; }

        [ForeignKey("Product")]
        public int pro_id { get; set; }

        [ForeignKey("Sender")]
        public int sender { get; set; }

        [Required]
        [StringLength(50)]
        public string? state { get; set; }

        [Required]
        [StringLength(255)]
        public string? short_description { get; set; }

        public string? description { get; set; }

        public DateTime create_date { get; set; }

        [ForeignKey("AssignmentGroup")]
        public int assignment_group { get; set; }

        public DateTime update_date { get; set; }

        [ForeignKey("UpdatedBy")]
        public int? updated_by { get; set; }

        public DateTime? closed_date { get; set; }

        public int quantity { get; set; }

        // Navigation properties
        public virtual Product? Product { get; set; }
        public virtual User? Sender { get; set; }
        public virtual Department? AssignmentGroup { get; set; }
        public virtual User? UpdatedBy { get; set; }
    }
}
