﻿using System;
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
    public class Announcement
    {
        [Key]
        public int id { get; set; }

        [Required]
        [StringLength(20)]
        public string? at_number { get; set; }

        [ForeignKey("User")]
        public int create_by { get; set; }

        public DateTime create_date { get; set; }

        public DateTime update_date { get; set; }

        public string? message { get; set; }

        public string? ann_title { get; set; }

        [JsonConverter(typeof(Base64ToByteArrayConverter))]
        public byte[]? ann_file { get; set; }

        public string? ann_type { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
    }
}
