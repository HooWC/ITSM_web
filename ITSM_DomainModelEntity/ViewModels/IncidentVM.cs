using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;

namespace ITSM_DomainModelEntity.ViewModels
{
    public class IncidentVM
    {
        public User User { get; set; }
        public List<Incident> Inc { get; set; }
        public string? assignmentGroup { get; set; }
        public string? assignedTo { get; set; }
    }
}
