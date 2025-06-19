using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Function;
using System.Text.Json.Serialization;

namespace ITSM_DomainModelEntity.Models
{
    public class RequestPhoto
    {
        [Key]
        public int id { get; set; }

        [ForeignKey("Request")]
        public int request_id { get; set; }

        [JsonConverter(typeof(Base64ToByteArrayConverter))]
        public byte[]? photo { get; set; }

        public string? photo_type { get; set; }

        public DateTime upload_time { get; set; }

        // Navigation properties
        public virtual Request? Request { get; set; }
    }
}
