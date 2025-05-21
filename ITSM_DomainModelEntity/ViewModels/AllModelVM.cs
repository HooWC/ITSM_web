using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;

namespace ITSM_DomainModelEntity.ViewModels
{
    public class AllModelVM
    {
        public User? user { get; set; }
        public User? info_user { get; set; }
        public List<User>? UserList { get; set; }
        public Category? category { get; set; }
        public List<Category>? CategoryList { get; set; }
        public Announcement? announcement { get; set; }
        public List<Announcement>? AnnouncementList { get; set; }
        public CMDB? CMDB { get; set; }
        public List<CMDB>? CMDBList { get; set; }
        public Department? department { get; set; }
        public List<Department>? DepartmentList { get; set; }
        public Feedback? feedback { get; set; }
        public List<Feedback>? FeedbackList { get; set; }
        public Incident? incident { get; set; }
        public List<Incident>? IncidentList { get; set; }
        public Knowledge? knowledge { get; set; }
        public List<Knowledge>? KnowledgeList { get; set; }
        public Note? note { get; set; }
        public List<Note>? NoteList { get; set; }
        public Product? product { get; set; }
        public List<Product>? ProductList { get; set; }
        public Request? request { get; set; }
        public List<Request>? RequestList { get; set; }
        public Role? role { get; set; }
        public List<Role>? RoleList { get; set; }
        public Todo? todo { get; set; }
        public List<Todo>? TodoList { get; set; }
    }
}
