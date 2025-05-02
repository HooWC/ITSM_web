using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;

namespace ITSM_DomainModelEntity.ViewModels
{
    public class PersonalHomeVM
    {
        public User Users { get; set; }
        public int CompletedTodo {  get; set; }
        public int TodoCount { get; set; }
        public int AllTodo {  get; set; }
        public int AllInc {  get; set; }
        public int ApplyInc {  get; set; }
        public int CompletedInc { get; set; }
        public int AllReq { get; set; }
        public int ApplyReq { get; set; }
        public int CompletedReq { get; set; }
        public int AllKnowledge { get; set; }
        public int AllFeedback { get; set; }
        public List<User> Team { get; set; }
        public List<Incident> IncidentsHistory { get; set; }
    }
}
