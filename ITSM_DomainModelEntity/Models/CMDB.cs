using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_DomainModelEntity.Models
{
    public class CMDB
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(100)]
        public string? full_name { get; set; }

        [ForeignKey("Department")]
        public int department_id { get; set; }

        public string? device_type { get; set; }

        public string? windows_version { get; set; }

        public string? hostname { get; set; }

        public string? microsoft_office { get; set; }

        public string? antivirus { get; set; }

        [Required]
        [StringLength(50)]
        public string? ip_address { get; set; }

        public string? erp_system { get; set; }

        public bool sql_account { get; set; }

        public string? processor { get; set; }

        [StringLength(100)]
        public string? motherboard { get; set; }

        [StringLength(50)]
        public string? ram { get; set; }

        [StringLength(100)]
        public string? monitor_led { get; set; }

        [StringLength(100)]
        public string? keyboard { get; set; }

        [StringLength(100)]
        public string? mouse { get; set; }

        [StringLength(100)]
        public string? hard_disk { get; set; }

        public string? dvdrw { get; set; }

        [StringLength(100)]
        public string? ms_office { get; set; }

        // Navigation properties
        public virtual Department? Department { get; set; }
    }
}
