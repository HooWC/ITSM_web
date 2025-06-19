using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Function;

namespace ITSM_DomainModelEntity.Models
{
    public class Req_Note
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(20)]
        public string? note_number { get; set; }

        [ForeignKey("Request")]
        public int request_id { get; set; }

        [ForeignKey("User")]
        public int user_id { get; set; }

        public DateTime create_date { get; set; }

        [Required]
        public string? message { get; set; }

        public bool note_read { get; set; }

        [ForeignKey("ReceiverUser")]
        public int? receiver_id { get; set; }

        // Navigation properties
        public virtual Request? Request { get; set; }
        public virtual User? User { get; set; }
        public virtual User? ReceiverUser { get; set; }
    }
}
