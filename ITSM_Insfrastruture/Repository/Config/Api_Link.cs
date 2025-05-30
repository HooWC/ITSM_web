using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITSM_Insfrastruture.Repository.Config
{
    public class Api_Link
    {
        // Base Api Link
        public static readonly string BaseUrl = "http://localhost:5300";

        // ============================================================================

        // === For User Part ===
        // login
        public static readonly string AuthLink = $"{BaseUrl}/users/authenticate";
        // register
        public static readonly string RegisterLink = $"{BaseUrl}/users/register";
        // get all user (need token)
        public static readonly string AllUserLink = $"{BaseUrl}/users";
        // get user by id && update user  (ID) (need token)
        public static readonly string User_F_U_Link = $"{BaseUrl}/users/";
        // get user current (need token)
        public static readonly string UserCurrentLink = $"{BaseUrl}/users/current";
        // get user delete  (ID) (need token) (x)
        public static readonly string UserDeleteLink = $"{BaseUrl}/users/";
        
        public static readonly string UserForgotPasswordLink = $"{BaseUrl}/userNoToken/forgotpassword";
        public static readonly string UserNoTokenLink = $"{BaseUrl}/userNoToken/getbyid";
        // === For User Part ===

        // ============================================================================

        // === For Todo Part ===
        // get all & create todo  (need token)
        public static readonly string TodoLink = $"{BaseUrl}/todos";
        // search by id & update && delete todo  (ID) (need token)
        public static readonly string TodoSUDLink = $"{BaseUrl}/todos/";
        // === For Todo Part ===

        // ============================================================================

        // === For Role Part ===
        // get all & create role  (need token)
        public static readonly string RoleLink = $"{BaseUrl}/roles";
        // search by id & update && delete todo  (ID) (need token)
        public static readonly string RoleSUDLink = $"{BaseUrl}/roles/";

        // no token
        public static readonly string RoleGetAllLink = $"{BaseUrl}/roles/getall";
        // === For Role Part ===

        // ============================================================================

        // === For Request Part ===
        // get all & create Request  (need token)
        public static readonly string ReqLink = $"{BaseUrl}/requests";
        // search by id & update && delete Request  (ID) (need token)
        public static readonly string ReqSUDLink = $"{BaseUrl}/requests/";
        // === For Request Part ===

        // ============================================================================

        // === For Product Part ===
        // get all & create Product  (need token)
        public static readonly string ProductLink = $"{BaseUrl}/products";
        // search by id & update && delete Product  (ID) (need token)
        public static readonly string ProductSUDLink = $"{BaseUrl}/products/";
        // Product photo upload endpoint
        public static readonly string ProductPhotoLink = $"{BaseUrl}/products/photo";
        // === For Product Part ===

        // ============================================================================

        // === For Note Part ===
        // get all & create Note  (need token)
        public static readonly string NoteLink = $"{BaseUrl}/notes";
        // search by id & update && delete Note  (ID) (need token)
        public static readonly string NoteSUDLink = $"{BaseUrl}/notes/";
        // === For Note Part ===

        // ============================================================================

        // === For Knowledge Part ===
        // get all & create Knowledge  (need token)
        public static readonly string KnowledgeLink = $"{BaseUrl}/knowledges";
        // search by id & update && delete Knowledge  (ID) (need token)
        public static readonly string KnowledgeSUDLink = $"{BaseUrl}/knowledges/";
        // === For Knowledge Part ===

        // ============================================================================

        // === For Incident Part ===
        // get all & create Incident  (need token)
        public static readonly string IncidentLink = $"{BaseUrl}/incidents";
        // search by id & update && delete Incident  (ID) (need token)
        public static readonly string IncidentSUDLink = $"{BaseUrl}/incidents/";
        // === For Incident Part ===

        // ============================================================================

        // === For Feedback Part ===
        // get all & create Feedback  (need token)
        public static readonly string FeedbackLink = $"{BaseUrl}/feedbacks";
        // search by id & update && delete Feedback  (ID) (need token)
        public static readonly string FeedbackSUDLink = $"{BaseUrl}/feedbacks/";
        // === For Feedback Part ===

        // ============================================================================

        // === For Department Part ===
        // get all & create Department  (need token)
        public static readonly string DepartmentLink = $"{BaseUrl}/departments";
        // search by id & update && delete Department  (ID) (need token)
        public static readonly string DepartmentSUDLink = $"{BaseUrl}/departments/";

        // no token
        public static readonly string DepartmentGetAllLink = $"{BaseUrl}/departments/getall";
        // === For Department Part ===

        // ============================================================================

        // === For CMDB Part ===
        // get all & create CMDB  (need token)
        public static readonly string CMDBLink = $"{BaseUrl}/cmdb";
        // search by id & update && delete CMDB  (ID) (need token)
        public static readonly string CMDBSUDLink = $"{BaseUrl}/cmdb/";
        // === For CMDB Part ===

        // ============================================================================

        // === For Category Part ===
        // get all & create Category  (need token)
        public static readonly string CategoryLink = $"{BaseUrl}/categorys";
        // search by id & update && delete Category  (ID) (need token)
        public static readonly string CategorySUDLink = $"{BaseUrl}/categorys/";
        // === For Category Part ===

        // ============================================================================

        // === For Announcement Part ===
        // get all & create Announcement  (need token)
        public static readonly string AnnouncementLink = $"{BaseUrl}/announcements";
        // search by id & update && delete Announcement  (ID) (need token)
        public static readonly string AnnouncementSUDLink = $"{BaseUrl}/announcements/";
        // === For Announcement Part ===

        // ============================================================================

        // === For Announcement Part ===
        // get all & create Announcement  (need token)
        public static readonly string VersionLink = $"{BaseUrl}/myversions";
        // search by id & update && delete Announcement  (ID) (need token)
        public static readonly string VersionSUDLink = $"{BaseUrl}/myversions/";
        // === For Announcement Part ===

        // ============================================================================

    }
}
