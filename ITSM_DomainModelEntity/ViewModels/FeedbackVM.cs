using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;

namespace ITSM_DomainModelEntity.ViewModels
{
    public class FeedbackVM
    {
        public User? User { get; set; }
        public List<Feedback>? Feedback { get; set; }
    }
}
