using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_DomainModelEntity.Models
{
    public class Knowledge
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(20)]
        public string? kb_number { get; set; }

        [ForeignKey("Author")]
        public int author { get; set; }

        public string? article { get; set; }

        [Required]
        [StringLength(255)]
        public string? short_description { get; set; }

        [ForeignKey("Category")]
        public int category_id { get; set; }

        public DateTime create_date { get; set; } = DateTime.Now;

        public DateTime? updated { get; set; }

        public bool active { get; set; } = true;

        public byte[]? kb_file { get; set; }
        public string? title { get; set; }
        public int kb_view {  get; set; }

        // Navigation properties
        public virtual User? Author { get; set; }
        public virtual Category? Category { get; set; }
    }
}
