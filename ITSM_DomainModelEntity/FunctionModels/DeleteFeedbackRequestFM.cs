using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_DomainModelEntity.FunctionModels
{
    public class DeleteFeedbackRequestFM
    {
        public string? word { get; set; }
        public List<int>? ids { get; set; }
    }
}
