using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_DomainModelEntity.Models
{
    public class Feedback
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(20)]
        public string? fb_number { get; set; }

        [ForeignKey("User")]
        public int user_id { get; set; }

        public DateTime create_date { get; set; }

        public DateTime update_date { get; set; }

        [Required]
        public string? message { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
    }
}
