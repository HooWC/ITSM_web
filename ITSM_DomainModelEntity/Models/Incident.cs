using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_DomainModelEntity.Models
{
    public class Incident
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(20)]
        public string? inc_number { get; set; }

        public DateTime create_date { get; set; } = DateTime.Now;

        [Required]
        [StringLength(255)]
        public string? short_description { get; set; }

        public string? describe { get; set; }

        [ForeignKey("Sender")]
        public int sender { get; set; }

        [Required]
        [StringLength(50)]
        public string? impact { get; set; }

        [Required]
        [StringLength(50)]
        public string? urgency { get; set; }

        [Required]
        [StringLength(50)]
        public string? priority { get; set; }

        [Required]
        [StringLength(50)]
        public string? state { get; set; }

        [Required]
        [StringLength(100)]
        public string? category { get; set; }

        [Required]
        [StringLength(100)]
        public string? subcategory { get; set; }

        [ForeignKey("AssignmentGroup")]
        public int assignment_group { get; set; }

        [ForeignKey("AssignedTo")]
        public int assigned_to { get; set; }

        public DateTime? updated { get; set; }

        [ForeignKey("UpdatedBy")]
        public int? updated_by { get; set; }

        public string? resolution { get; set; }

        [ForeignKey("ResolvedBy")]
        public int? resolved_by { get; set; }

        public DateTime? resolved_date { get; set; }

        public DateTime? close_date { get; set; }

        // Navigation properties
        public virtual User? Sender { get; set; }
        public virtual User? AssignedTo { get; set; }
        public virtual User? UpdatedBy { get; set; }
        public virtual User? ResolvedBy { get; set; }
        public virtual Department? AssignmentGroup { get; set; }
    }
}
