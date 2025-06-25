using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using ITSM_DomainModelEntity.Function;

namespace ITSM_DomainModelEntity.Models
{
    public class User
    {
        [Key]
        public int id { get; set; }

        [StringLength(50)]
        public string? emp_id { get; set; }

        [StringLength(10)]
        public string? prefix { get; set; }

        [JsonConverter(typeof(Base64ToByteArrayConverter))]
        public byte[]? photo { get; set; }

        [StringLength(100)]
        public string? fullname { get; set; }

        [StringLength(255)]
        public string? email { get; set; }

        [StringLength(10)]
        public string? gender { get; set; }

        [ForeignKey("Department")]
        public int department_id { get; set; }

        [StringLength(100)]
        public string? title { get; set; }

        [StringLength(20)]
        public string? business_phone { get; set; }

        [StringLength(20)]
        public string? mobile_phone { get; set; }

        [ForeignKey("Role")]
        public int? role_id { get; set; }

        //[StringLength(50)]
        //public string? username { get; set; }

        [StringLength(255)]
        public string? password { get; set; }

        [StringLength(50)]
        public string? race { get; set; }

        public DateTime create_date { get; set; }

        public DateTime? update_date { get; set; }

        public bool active { get; set; }

        public string? photo_type { get; set; }

        public bool r_manager { get; set; }

        // Navigation properties
        public virtual Role? Role { get; set; }
        public virtual Department? Department { get; set; }
    }
}
