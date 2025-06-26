using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITSM_DomainModelEntity.Models;

namespace ITSM_DomainModelEntity.ViewModels
{
    public class MonthlyStats
    {
        public int Month { get; set; }
        public int ResolvedCount { get; set; }
        public int? OtherCount { get; set; }
    }

    public class YearlyStats
    {
        public int Year { get; set; }
        public Dictionary<int, MonthlyStats> MonthlyData { get; set; }
        public double Growth { get; set; }

        public YearlyStats()
        {
            MonthlyData = new Dictionary<int, MonthlyStats>();
            Growth = 0;
        }
    }

    public class MonthlyTodoStats
    {
        public string MonthName { get; set; }
        public int ActiveCount { get; set; }
    }

    public class MonthlyRequestStats
    {
        public string MonthName { get; set; }
        public int RequestCount { get; set; }
    }

    public class IncMessage
    {
        public Incident? inc_info { get; set; }
        public int NoteMessageCount { get; set; }
    }

    public class AllModelVM
    {
        public User? user { get; set; }
        public User? info_user { get; set; }
        public List<User>? UserList { get; set; }
        public Category? category { get; set; }
        public List<Category>? CategoryList { get; set; }
        public Announcement? announcement { get; set; }
        public List<Announcement>? AnnouncementList { get; set; }
        public Myversion? myversion { get; set; }
        public List<Myversion>? MyversionList { get; set; }  
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
        public string? kb_search_word { get; set; }
        public string? kb_search_all { get; set; }

        //== Personal
        public User User { get; set; }
        public int CompletedTodo { get; set; }
        public int TodoCount { get; set; }
        public int AllTodo { get; set; }
        public int AllInc { get; set; }
        public int ApplyInc { get; set; }
        public int InProgressInc { get; set; }
        public int OnHoldInc { get; set; }
        public int CompletedInc { get; set; }
        public int AllReq { get; set; }
        public int ApplyReq { get; set; }
        public int CompletedReq { get; set; }
        public int AllKnowledge { get; set; }
        public int AllFeedback { get; set; }
        public string? DepartmentName { get; set; }
        public string? RoleName { get; set; }
        public List<User>? Team { get; set; }
        public List<Incident>? IncidentsHistory { get; set; }

        //== role back
        public string? roleBack { get; set; }

        //== Dashboards
        public int inc_count { get; set; }
        public int req_count { get; set; }
        public int kb_count { get; set; }
        public int fd_count { get; set; }
        public int total_count { get; set; }
        public int todo_count { get; set; }
        public double todo_d_count { get; set; }

        //== Today data
        public int today_inc_count { get; set; }
        public double today_inc_percent { get; set; }  
        
        public int today_req_count { get; set; }
        public double today_req_percent { get; set; }  
        
        public int today_kb_count { get; set; }
        public double today_kb_percent { get; set; }   
        
        public int today_fd_count { get; set; }
        public double today_fd_percent { get; set; }

        //== Request statistics for this year
        public int yearly_req_total { get; set; }                 
        public int yearly_req_completed_count { get; set; }       
        public double yearly_req_completed_percent { get; set; }  
        public int yearly_req_other_count { get; set; }          
        public double yearly_req_other_percent { get; set; }     

        //== Incidents yearly statistics attributes
        public List<YearlyStats>? YearlyIncidentStats { get; set; }

        //== Todo monthly statistics
        public List<MonthlyTodoStats>? MonthlyTodoStats { get; set; }

        //== Request monthly statistics
        public List<MonthlyRequestStats>? MonthlyRequestStats { get; set; }

        //== Incident Photos List
        public List<IncidentPhotos>? Incident_Photos_List { get; set; }

        //== Subcategory List
        public List<Subcategory>? Subcategory_List { get; set; }

        public Subcategory? Sub_Category {  get; set; }

        //== Incident Category List
        public List<Incidentcategory>? Incident_Category_List { get; set; }

        public Incidentcategory? Incident_Category { get; set; }

        public int? noteMessageCount { get; set; }

        public List<IncMessage>? incMessages { get; set; }

        public Req_Category? req_category { get; set; }
        public List<Req_Category>? reqCategoryList {  get; set; }
        public Req_Subcategory? req_subcategory { get; set; }
        public List<Req_Subcategory>? reqSubCategoryList {  get; set; }
        public Req_Function? req_function { get; set; }
        public List<Req_Function>? reqFunctionList {  get; set; }

        public List<RequestPhoto>? reqPhotoList { get; set; }

        public int incCount { get; set; }

        public int reqCount { get; set; }

        public AllModelVM()
        {
            YearlyIncidentStats = new List<YearlyStats>();
            MonthlyTodoStats = new List<MonthlyTodoStats>();
            MonthlyRequestStats = new List<MonthlyRequestStats>();
        }
    }
}
