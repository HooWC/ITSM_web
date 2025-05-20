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
    public class Product
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(50)]
        public string? pro_number { get; set; }

        [ForeignKey("Category")]
        public int category_id { get; set; }

        [ForeignKey("ResponsibleDepartment")]
        public int responsible { get; set; }

        [JsonConverter(typeof(Base64ToByteArrayConverter))]
        public byte[]? photo { get; set; }

        [Required]
        [StringLength(100)]
        public string? item_title { get; set; }

        [Required]
        public string? description { get; set; }

        public int quantity { get; set; }

        public bool active { get; set; }

        public string? photo_type {  get; set; }

        public string? product_type {  get; set; }

        // Navigation properties
        public virtual Category? Category { get; set; }
        public virtual Department? ResponsibleDepartment { get; set; }
    }
}
