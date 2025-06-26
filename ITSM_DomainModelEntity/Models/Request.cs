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
        public int? pro_id { get; set; }

        [ForeignKey("Sender")]
        public int sender { get; set; }

        [StringLength(50)]
        public string? state { get; set; }

        public string? description { get; set; }

        public DateTime create_date { get; set; }

        [ForeignKey("AssignmentGroup")]
        public int? assignment_group { get; set; }

        public DateTime update_date { get; set; }

        [ForeignKey("UpdatedBy")]
        public int? updated_by { get; set; }

        public int? quantity { get; set; }

        [ForeignKey("AssignedTo")]
        public int? assigned_to {  get; set; }

        //==

        public bool? req_type {  get; set; }
        public string? erp_version { get; set; }
        [ForeignKey("AssignedTo")]
        public int? erp_category { get; set; }
        [ForeignKey("AssignedTo")]
        public int? erp_subcategory { get; set; }
        [ForeignKey("AssignedTo")]
        public int? erp_function { get; set; }
        public string? erp_module { get; set; }
        public string? erp_user_account { get; set; }
        public bool? erp_report { get; set; }
        public string? erp_resolution_type { get; set; }
        public string? erp_resolution { get; set; } 
        public DateTime? erp_resolved_date { get; set; }

        // Navigation properties
        public virtual Product? Product { get; set; }
        public virtual User? Sender { get; set; }
        public virtual Department? AssignmentGroup { get; set; }
        public virtual User? UpdatedBy { get; set; }
        public virtual User? AssignedTo { get; set; }
        public virtual Req_Category? ERPCategory { get; set; }
        public virtual Req_Subcategory? ERPSubcategory { get; set; }
        public virtual Req_Function? ERPFunction { get; set; }
    }
}
