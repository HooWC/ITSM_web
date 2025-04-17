using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_DomainModelEntity.Models
{
    public class Todo
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("User")]
        public int user_id { get; set; }

        [Required]
        [StringLength(255)]
        public string? title { get; set; }

        public DateTime create_date { get; set; } = DateTime.Now;

        public DateTime? update_date { get; set; }

        public bool active { get; set; } = false;

        // Navigation properties
        public virtual User? User { get; set; }
    }
}
