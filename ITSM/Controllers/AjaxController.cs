using Microsoft.AspNetCore.Mvc;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Http;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_DomainModelEntity.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;
using ITSM_DomainModelEntity.FunctionModels;
using System.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using ITSM_DomainModelEntity.Function;
using Humanizer;

namespace ITSM.Controllers
{
    public class AjaxController : Controller
    {
        private readonly TokenService _tokenService;
        private readonly Todo_api _todoApi;
        private readonly Department_api _departmentApi;
        private readonly User_api _userApi;
        private readonly Incident_api _incApi;
        private readonly Note_api _noteApi;
        private readonly Feedback_api _feedApi;
        private readonly Category_api _categoryApi;
        private readonly Product_api _productApi;
        private readonly Role_api _roleApi;
        private readonly Request_api _reqApi;
        private readonly Announcement_api _announApi;
        private readonly CMDB_api _cmdbApi;
        private readonly Knowledge_api _kbApi;
        private readonly Myversion_api _myversionApi;
        private readonly Subcategory_api _subcategoryApi;
        private readonly Incident_Category_api _incidentcategoryApi;
        private readonly Req_Category_api _reqcategoryApi;
        private readonly Req_Subcategory_api _reqsubcategoryApi;
        private readonly Req_Function_api _reqfunctionApi;

        public AjaxController(IHttpContextAccessor httpContextAccessor)
        {
            _tokenService = new TokenService(httpContextAccessor);
            _todoApi = new Todo_api(httpContextAccessor);
            _departmentApi = new Department_api(httpContextAccessor);
            _userApi = new User_api(httpContextAccessor);
            _incApi = new Incident_api(httpContextAccessor);
            _noteApi = new Note_api(httpContextAccessor);
            _feedApi = new Feedback_api(httpContextAccessor);
            _categoryApi = new Category_api(httpContextAccessor);
            _productApi = new Product_api(httpContextAccessor);
            _roleApi = new Role_api(httpContextAccessor);
            _reqApi = new Request_api(httpContextAccessor);
            _announApi = new Announcement_api(httpContextAccessor);
            _cmdbApi = new CMDB_api(httpContextAccessor);
            _kbApi = new Knowledge_api(httpContextAccessor);
            _myversionApi = new Myversion_api(httpContextAccessor);
            _subcategoryApi = new Subcategory_api(httpContextAccessor);
            _incidentcategoryApi = new Incident_Category_api(httpContextAccessor);
            _reqcategoryApi = new Req_Category_api(httpContextAccessor);
            _reqsubcategoryApi = new Req_Subcategory_api(httpContextAccessor);
            _reqfunctionApi = new Req_Function_api(httpContextAccessor);
        }

        private bool IsUserLoggedIn(out User currentUser)
        {
            currentUser = _tokenService.GetUserInfo();
            return currentUser != null;
        }

        public IActionResult _Logout()
        {
            // Clear the token so the user must log in again the next time they visit
            _tokenService.ClearToken();

            // Returns a JSON result
            return Json(new { success = true, message = "Log out success" });
        }

        /// <summary>
        /// Personal/Todo
        public async Task<IActionResult> SearchTodo(string searchTerm, string filterBy = "number")
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json(new List<Todo>());

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allTodos = await _todoApi.GetAllTodo_API();
            var userTodos = allTodos.Where(x => x.user_id == currentUser.id).OrderByDescending(y => y.id).ToList();

            List<Todo> filteredTodos;

            if (searchTerm == "re_entrynovalue")
            {
                filteredTodos = userTodos;
            }
            else
            {
                switch (filterBy.ToLower())
                {
                    case "number":
                        filteredTodos = userTodos
                            .Where(t => t.todo_id != null && t.todo_id.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "title":
                        filteredTodos = userTodos
                            .Where(t => t.title != null && t.title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "create_date":
                        filteredTodos = userTodos
                            .Where(t => t.create_date.ToString("yyyy-MM-dd HH:mm:ss").Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "update_date":
                        filteredTodos = userTodos
                            .Where(t => t.update_date.ToString("yyyy-MM-dd HH:mm:ss").Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    default:
                        filteredTodos = userTodos
                            .Where(t => t.todo_id != null && t.todo_id.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                }
            }

            var result = filteredTodos.Select(t => new
            {
                t.id,
                t.todo_id,
                t.title,
                t.user_id,
                t.active,
                create_date = t.create_date.ToString("yyyy-MM-dd HH:mm:ss"),
                update_date = t.update_date.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Json(result);
        }

        /// <summary>
        /// Personal/Todo
        public async Task<IActionResult> FilterTodoByStatus(string status = "all")
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allTodos = await _todoApi.GetAllTodo_API();
            var userTodos = allTodos.Where(x => x.user_id == currentUser.id).OrderByDescending(y => y.id).ToList();

            List<Todo> filteredTodos;

            switch (status.ToLower())
            {
                case "doing":
                    filteredTodos = userTodos.Where(t => t.active == false).ToList();
                    break;
                case "completed":
                    filteredTodos = userTodos.Where(t => t.active == true).ToList();
                    break;
                case "all":
                default:
                    filteredTodos = userTodos;
                    break;
            }

            var result = filteredTodos.Select(t => new {
                t.id,
                t.todo_id,
                t.title,
                t.user_id,
                t.active,
                create_date = t.create_date.ToString("yyyy-MM-dd HH:mm:ss"),
                update_date = t.update_date.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Json(result);
        }

        /// <summary>
        /// Personal/Todo
        public async Task<IActionResult> SortTodo(string sortOrder = "asc")
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allTodos = await _todoApi.GetAllTodo_API();
            var userTodos = allTodos.Where(x => x.user_id == currentUser.id).OrderByDescending(y => y.id).ToList();

            List<Todo> sortedTodos;
            if (sortOrder.ToLower() == "desc")
                sortedTodos = userTodos.OrderBy(x => x.id).ToList();
            else
                sortedTodos = userTodos.OrderByDescending(x => x.id).ToList();

            var result = sortedTodos.Select(t => new {
                t.id,
                t.todo_id,
                t.title,
                t.user_id,
                t.active,
                create_date = t.create_date.ToString("yyyy-MM-dd HH:mm:ss"),
                update_date = t.update_date.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Json(result);
        }

        /// <summary>
        /// Personal/Todo
        [HttpPost]
        public async Task<IActionResult> DeleteTodos([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for deletion" });

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                foreach (var id in ids)
                {
                    var allTodos = await _todoApi.GetAllTodo_API();
                    var todoToDelete = allTodos.FirstOrDefault(t => t.id == id && t.user_id == currentUser.id);

                    if (todoToDelete != null)
                    {
                        bool result = await _todoApi.DeleteTodo_API(id);
                        if (result)
                            successCount++;
                    }
                }

                if (successCount > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully deleted {successCount} item(s)"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to delete items. Items may not exist or you don't have permission"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get all department data
        public async Task<IActionResult> DepartmentData()
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allDepartment = await _departmentApi.GetAllDepartment_API();

            return Json(allDepartment);
        }

        /// <summary>
        /// Get user data based on department ID
        [HttpPost]
        public async Task<IActionResult> AssignedToData(int departmentId, int caller_id)
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                var userTask = _userApi.GetAllUser_API();
                var roleTask = _roleApi.GetAllRole_API();
                await Task.WhenAll(roleTask, userTask);

                var allRole = roleTask.Result;
                var usersInDepartment = userTask.Result;

                var result = usersInDepartment
                    .Select(user =>
                    {
                        user.Role = allRole.FirstOrDefault(r => r.id == user.role_id);
                        return user;
                    })
                    .Where(user => user.department_id == departmentId && user.id != caller_id
                     && user.active)
                    .ToList();

                return Json(result);

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// IncidentManagement/All
        public async Task<IActionResult> SearchIncident(string searchWord, string searchTerm, string filterBy = "number")
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json(new List<Incident>());

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allIncs = await _incApi.GetAllIncident_API();
            var userIncs = new List<Incident>();
            if (searchWord == "resolve")
                userIncs = allIncs.Where(x => (x.assigned_to == currentUser.id || x.updated_by == currentUser.id) && x.state == "Resolved").OrderByDescending(y => y.id).ToList();
            else if (searchWord == "closed")
                userIncs = allIncs.Where(x => (x.assigned_to == currentUser.id || x.updated_by == currentUser.id) && x.state == "Closed").OrderByDescending(y => y.id).ToList();
            else if (searchWord == "tome")
                userIncs = allIncs.Where(x => x.assigned_to == currentUser.id && x.state != "Resolved" && x.state != "Closed").OrderByDescending(y => y.id).ToList();
            else if (searchWord == "toteam")
                userIncs = allIncs.Where(x => x.assignment_group == currentUser.department_id).OrderByDescending(y => y.id).ToList();
            else if (searchWord == "user")
                userIncs = allIncs.Where(x => x.sender == currentUser.id).OrderByDescending(y => y.id).ToList();
            else if (searchWord == "assign_work")
                userIncs = allIncs.Where(x => x.assignment_group == currentUser.department_id && x.assigned_to == null && x.state != "Closed").OrderByDescending(y => y.id).ToList();
            else
                userIncs = allIncs.OrderByDescending(y => y.id).ToList();

            var departmentTask = _departmentApi.GetAllDepartment_API();
            var userTask = _userApi.GetAllUser_API();
            var inc_categoryTask = _incidentcategoryApi.GetAllIncidentcategory_API();
            var sucategoryTask = _subcategoryApi.GetAllSubcategory_API();

            await Task.WhenAll(departmentTask, userTask, inc_categoryTask, sucategoryTask);

            var allDepartments = departmentTask.Result;
            var allUsers = userTask.Result;
            var allIncCategory = inc_categoryTask.Result;
            var allSucategory = sucategoryTask.Result;

            List<Incident> filteredIncs;

            if (searchTerm == "re_entrynovalue")
            {
                filteredIncs = userIncs;
            }
            else
            {
                switch (filterBy.ToLower())
                {
                    case "urgency":
                        filteredIncs = userIncs
                            .Where(t => t.urgency != null && t.urgency.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "sucategory":
                        var filterSubcategorys = allSucategory.Where(x => x.subcategory.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        filteredIncs = (from i in userIncs
                                        join s in filterSubcategorys on i.subcategory equals s.id
                                        select i).ToList();
                        break;
                    case "state":
                        filteredIncs = userIncs
                            .Where(t => t.state != null && t.state.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "category":
                        var filterIncCategorys = allIncCategory.Where(x => x.name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        filteredIncs = (from i in userIncs
                                        join c in filterIncCategorys on i.category equals c.id
                                        select i).ToList();
                        break;
                    case "assignment_group":
                        var filterDepartments = allDepartments.Where(x => x.name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        filteredIncs = (from i in userIncs
                                        join d in filterDepartments on i.assignment_group equals d.id
                                        select i).ToList();
                        break;
                    case "assigned_to":
                        var filterUsers = allUsers.Where(x => x.fullname.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        filteredIncs = (from i in userIncs
                                        join u in filterUsers on i.assigned_to equals u.id
                                        select i).ToList();
                        break;
                    case "opened":
                        filteredIncs = userIncs
                            .Where(t => t.create_date != null && t.create_date.ToString("yyyy-MM-dd HH:mm:ss").Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "updated":
                        filteredIncs = userIncs
                            .Where(t => t.updated != null && t.updated.ToString("yyyy-MM-dd HH:mm:ss").Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "number":
                    default:
                        filteredIncs = userIncs
                            .Where(t => t.inc_number != null && t.inc_number.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                }
            }

            var result = filteredIncs.Select(t => new
            {
                t.id,
                t.inc_number,
                t.urgency,
                t.state,
                category = allIncCategory.FirstOrDefault(x => x.id == t.category)?.name,
                subcategory = allSucategory.FirstOrDefault(x => x.id == t.subcategory)?.subcategory,
                assignment_group = allDepartments.FirstOrDefault(d => d.id == t.assignment_group)?.name ?? "",
                create_date = t.create_date.ToString("yyyy-MM-dd HH:mm:ss"),
                update_date = t.updated.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Json(result);
        }

        /// <summary>
        /// IncidentManagement/All
        public async Task<IActionResult> FilterIncidentByStatus(string filterword, string status = "all")
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allIncs = await _incApi.GetAllIncident_API();

            var userIncs = new List<Incident>();
            if (filterword == "tome")
                userIncs = allIncs.Where(x => x.assigned_to == currentUser.id && x.state != "Resolved" && x.state != "Closed").OrderByDescending(y => y.id).ToList();
            else if (filterword == "toteam")
                userIncs = allIncs.Where(x => x.assignment_group == currentUser.department_id).OrderByDescending(y => y.id).ToList();
            else if (filterword == "closed")
                userIncs = allIncs.Where(x => (x.assigned_to == currentUser.id || x.updated_by == currentUser.id) && x.state == "Closed").OrderByDescending(y => y.id).ToList();
            else if (filterword == "user")
                userIncs = allIncs.Where(x => x.sender == currentUser.id).OrderByDescending(y => y.id).ToList();
            else
                userIncs = allIncs.OrderByDescending(y => y.id).ToList();

            var departmentTask = _departmentApi.GetAllDepartment_API();
            var userTask = _userApi.GetAllUser_API();
            var inc_categoryTask = _incidentcategoryApi.GetAllIncidentcategory_API();
            var sucategoryTask = _subcategoryApi.GetAllSubcategory_API();

            await Task.WhenAll(departmentTask, userTask, inc_categoryTask, sucategoryTask);

            var allDepartments = departmentTask.Result;
            var allUsers = userTask.Result;
            var allIncCategory = inc_categoryTask.Result;
            var allSucategory = sucategoryTask.Result;

            List<Incident> filteredIncs;

            switch (status.ToLower())
            {
                case "pending":
                    filteredIncs = userIncs.Where(t => t.state == "Pending").ToList();
                    break;
                case "inprogress":
                    filteredIncs = userIncs.Where(t => t.state == "In Progress").ToList();
                    break;
                case "onhold":
                    filteredIncs = userIncs.Where(t => t.state == "On-Hold").ToList();
                    break;
                case "resolved":
                    filteredIncs = userIncs.Where(t => t.state == "Resolved").ToList();
                    break;
                case "closed":
                    filteredIncs = userIncs.Where(t => t.state == "Closed").ToList();
                    break;
                case "all":
                default:
                    filteredIncs = userIncs;
                    break;
            }

            var result = filteredIncs.Select(t => new {
                t.id,
                t.inc_number,
                t.urgency,
                t.state,
                category = allIncCategory.FirstOrDefault(x => x.id == t.category)?.name,
                subcategory = allSucategory.FirstOrDefault(x => x.id == t.subcategory)?.subcategory,
                assignment_group = allDepartments.FirstOrDefault(d => d.id == t.assignment_group)?.name ?? "",
                create_date = t.create_date.ToString("yyyy-MM-dd HH:mm:ss"),
                update_date = t.updated.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Json(result);
        }

        /// <summary>
        /// IncidentManagement/All
        public async Task<IActionResult> SortIncident(string sortWord, string sortOrder = "asc")
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allIncs = await _incApi.GetAllIncident_API();
            var userIncs = new List<Incident>();
            if (sortWord == "resolve")
                userIncs = allIncs.Where(x => (x.assigned_to == currentUser.id || x.updated_by == currentUser.id) && x.state == "Resolved").OrderByDescending(y => y.id).ToList();
            else if (sortWord == "closed")
                userIncs = allIncs.Where(x => (x.assigned_to == currentUser.id || x.updated_by == currentUser.id) && x.state == "Closed").OrderByDescending(y => y.id).ToList();
            else if (sortWord == "tome")
                userIncs = allIncs.Where(x => x.assigned_to == currentUser.id && x.state != "Resolved" && x.state != "Closed").ToList();
            else if (sortWord == "toteam")
                userIncs = allIncs.Where(x => x.assignment_group == currentUser.department_id).ToList();
            else if (sortWord == "user")
                userIncs = allIncs.Where(x => x.sender == currentUser.id).OrderByDescending(y => y.id).ToList();
            else if (sortWord == "assign_work")
                userIncs = allIncs.Where(x => x.assignment_group == currentUser.department_id && x.assigned_to == null && x.state != "Closed").OrderByDescending(y => y.id).ToList();
            else
                userIncs = allIncs.OrderByDescending(y => y.id).ToList();

            var departmentTask = _departmentApi.GetAllDepartment_API();
            var userTask = _userApi.GetAllUser_API();
            var inc_categoryTask = _incidentcategoryApi.GetAllIncidentcategory_API();
            var sucategoryTask = _subcategoryApi.GetAllSubcategory_API();

            await Task.WhenAll(departmentTask, userTask, inc_categoryTask, sucategoryTask);

            var allDepartments = departmentTask.Result;
            var allUsers = userTask.Result;
            var allIncCategory = inc_categoryTask.Result;
            var allSucategory = sucategoryTask.Result;

            List<Incident> sortedIncidents;
            if (sortOrder.ToLower() == "desc")
                sortedIncidents = userIncs.OrderBy(x => x.id).ToList();
            else
                sortedIncidents = userIncs.OrderByDescending(x => x.id).ToList();

            var result = sortedIncidents.Select(t => new {
                t.id,
                t.inc_number,
                t.urgency,
                t.state,
                category = allIncCategory.FirstOrDefault(x => x.id == t.category)?.name,
                subcategory = allSucategory.FirstOrDefault(x => x.id == t.subcategory)?.subcategory,
                assignment_group = allDepartments.FirstOrDefault(d => d.id == t.assignment_group)?.name ?? "",
                create_date = t.create_date.ToString("yyyy-MM-dd HH:mm:ss"),
                update_date = t.updated.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Json(result);
        }

        /// <summary>
        /// IncidentManagement/All
        [HttpPost]
        public async Task<IActionResult> DeleteIncidents([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for deletion" });

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                foreach (var id in ids)
                {
                    var allIncs = await _incApi.GetAllIncident_API();
                    var incToDelete = allIncs.FirstOrDefault(x => x.id == id);

                    if (incToDelete != null)
                    {
                        bool result = await _incApi.DeleteIncident_API(id);
                        if (result)
                            successCount++;
                    }
                }

                if (successCount > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully deleted {successCount} item(s)"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to delete items. Items may not exist or you don't have permission"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// IncidentManagement/Inc_Info_Form
        [HttpPost]
        public async Task<JsonResult> ResolveIncident(Incident inc, string resolveType, string resolveNotes)
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            if (string.IsNullOrEmpty(resolveType) || string.IsNullOrEmpty(resolveNotes))
                return Json(new { success = false, message = "Resolution type and resolution notes are required." });

            try
            {
                var incData = await _incApi.FindByIDIncident_API(inc.id);

                if (incData == null)
                    return Json(new { success = false, message = "Incident not found" });

                incData.describe = inc.describe;
                incData.urgency = inc.urgency;

                // resolved data
                incData.resolution = resolveNotes;
                incData.resolved_by = currentUser.id;
                incData.resolved_date = DateTime.Now;
                incData.resolved_type = resolveType;

                // Force state to Resolved regardless of what was in the form
                incData.state = "Resolved";
                incData.updated_by = currentUser.id;

                // TODO: Save resolution notes and type to database
                // This would require adding a new field to the Incident model or creating a Resolution model

                bool result = await _incApi.UpdateIncident_API(incData);

                if (result)
                    return Json(new { success = true });
                else
                    return Json(new { success = false, message = "Failed to update incident" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// IncidentManagement/Inc_Info_Form
        public async Task<IActionResult> GetResolutionHistory(int incidentId)
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                var incident = await _incApi.FindByIDIncident_API(incidentId);

                if (incident == null)
                    return Json(new { success = false, message = "Incident not found" });

                if (incident.state != "Resolved")
                    return Json(new { success = false, message = "Incident is not resolved yet" });

                var resolvedBy = new ITSM_DomainModelEntity.Models.User();
                if (incident.resolved_by.HasValue)
                    resolvedBy = await _userApi.FindByIDUser_API(incident.resolved_by.Value);
                var resolvedByName = resolvedBy != null ? resolvedBy.fullname : "Unknown";

                var resolutionData = new
                {
                    incident.id,
                    resolved_type = incident.resolved_type,
                    resolution = incident.resolution,
                    resolved_date = incident.resolved_date?.ToString("yyyy-MM-dd HH:mm:ss"),
                    resolved_by = incident.resolved_by,
                    resolved_by_name = resolvedByName,
                    resolved_by_avatar = resolvedBy?.photo,
                    resolved_photo_type = resolvedBy?.photo_type,
                    user_id = resolvedBy?.id
                };

                return Json(new { success = true, resolution = resolutionData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// IncidentManagement/Inc_Info_Form
        [HttpPost]
        public async Task<JsonResult> CloseIncident(Incident inc)
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                var incData = await _incApi.FindByIDIncident_API(inc.id);

                if (incData == null)
                    return Json(new { success = false, message = "No event found" });

                incData.close_date = DateTime.Now;
                incData.state = "Closed";
                incData.updated_by = currentUser.id;

                bool result = await _incApi.UpdateIncident_API(incData);

                if (result)
                    return Json(new { success = true });
                else
                    return Json(new { success = false, message = "Failed to update the event" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// IncidentManagement/Inc_Info_Form
        [HttpPost]
        public async Task<JsonResult> ReopenIncident(Incident inc)
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                var incData = await _incApi.FindByIDIncident_API(inc.id);

                if (incData == null)
                    return Json(new { success = false, message = "No event found" });

                if (incData.state != "Closed")
                    return Json(new { success = false, message = "Only closed events can be reopened" });

                incData.state = "Pending";
                incData.updated_by = currentUser.id;

                bool result = await _incApi.UpdateIncident_API(incData);

                if (result)
                    return Json(new { success = true });
                else
                    return Json(new { success = false, message = "Failed to reopen event" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Feedback/Feedback_List
        public async Task<IActionResult> SearchFeedback(string searchWord, string searchTerm, string filterBy = "number")
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json(new List<Feedback>());

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allFeedbacks = await _feedApi.GetAllFeedback_API();
            var userFeed = new List<Feedback>();
            if (searchWord == "_user")
                userFeed = allFeedbacks.Where(x => x.user_id == currentUser.id).OrderByDescending(y => y.id).ToList();
            else
                userFeed = allFeedbacks.OrderByDescending(y => y.id).ToList();

            var allUsers = await _userApi.GetAllUser_API();

            List<Feedback> filteredFeeds;

            if (searchTerm == "re_entrynovalue")
            {
                filteredFeeds = userFeed;
            }
            else
            {
                switch (filterBy.ToLower())
                {
                    case "message":
                        filteredFeeds = userFeed
                            .Where(t => t.message != null && t.message.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "user":
                        var filterUsers = allUsers.Where(x => x.fullname.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        filteredFeeds = (from i in userFeed
                                         join u in filterUsers on i.user_id equals u.id
                                         select i).ToList();
                        break;
                    case "create_date":
                        filteredFeeds = userFeed
                            .Where(t => t.create_date != null && t.create_date.ToString("yyyy-MM-dd HH:mm:ss").Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "update_date":
                        filteredFeeds = userFeed
                            .Where(t => t.update_date != null && t.update_date.ToString("yyyy-MM-dd HH:mm:ss").Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "number":
                    default:
                        filteredFeeds = userFeed
                            .Where(t => t.fb_number != null && t.fb_number.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                }
            }

            var result = filteredFeeds.Select(t => new {
                t.id,
                t.fb_number,
                t.message,
                user = allUsers.FirstOrDefault(u => u.id == t.user_id)?.fullname ?? "",
                create_date = t.create_date.ToString("yyyy-MM-dd HH:mm:ss"),
                update_date = t.update_date.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Json(result);
        }

        /// <summary>
        /// Feedback/Feedback_List
        public async Task<IActionResult> SortFeedback(string sortWord, string sortOrder = "asc")
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allFeeds = await _feedApi.GetAllFeedback_API();
            var userFeeds = new List<Feedback>();
            if (sortWord == "_user")
                userFeeds = allFeeds.Where(x => x.user_id == currentUser.id).OrderByDescending(y => y.id).ToList();
            else
                userFeeds = allFeeds.OrderByDescending(y => y.id).ToList();

            var allUsers = await _userApi.GetAllUser_API();

            List<Feedback> sortedFeedbacks;
            if (sortOrder.ToLower() == "desc")
                sortedFeedbacks = userFeeds.OrderBy(x => x.id).ToList();
            else
                sortedFeedbacks = userFeeds.OrderByDescending(x => x.id).ToList();

            var result = sortedFeedbacks.Select(t => new {
                t.id,
                t.fb_number,
                t.message,
                user = allUsers.FirstOrDefault(u => u.id == t.user_id)?.fullname ?? "",
                create_date = t.create_date.ToString("yyyy-MM-dd HH:mm:ss"),
                update_date = t.update_date.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Json(result);
        }

        /// <summary>
        /// Feedback/Feedback_List
        [HttpPost]
        public async Task<IActionResult> DeleteFeedbacks([FromBody] DeleteFeedbackRequestFM request)
        {
            var word = request.word;
            var ids = request.ids;

            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for deletion" });

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                foreach (var id in ids)
                {
                    var allFeeds = await _feedApi.GetAllFeedback_API();
                    var incToDelete = new Feedback();
                    if (word == "user")
                        incToDelete = allFeeds.FirstOrDefault(x => x.id == id && x.user_id == currentUser.id);
                    else
                        incToDelete = allFeeds.FirstOrDefault(x => x.id == id);

                    if (incToDelete != null)
                    {
                        bool result = await _feedApi.DeleteFeedback_API(id);
                        if (result)
                            successCount++;
                    }
                }

                if (successCount > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully deleted {successCount} item(s)"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to delete items. Items may not exist or you don't have permission"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Category/Category_List
        public async Task<IActionResult> SearchCategory(string searchTerm, string filterBy = "title")
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json(new List<Category>());

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allCategorys = await _categoryApi.GetAllCategory_API();
            var userCategory = new List<Category>();
            userCategory = allCategorys.OrderByDescending(y => y.id).ToList();

            List<Category> filteredCategorys;

            if (searchTerm == "re_entrynovalue")
            {
                filteredCategorys = userCategory;
            }
            else
            {
                switch (filterBy.ToLower())
                {

                    case "description":
                        filteredCategorys = userCategory
                            .Where(x => x.description != null && x.description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "title":
                    default:
                        filteredCategorys = userCategory
                            .Where(t => t.title != null && t.title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                }
            }

            var result = filteredCategorys.Select(t => new {
                t.title,
                t.description
            });

            return Json(result);
        }

        /// <summary>
        /// Category/Category_List
        [HttpPost]
        public async Task<IActionResult> DeleteCategory([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for deletion" });

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                foreach (var id in ids)
                {
                    var allCategorys = await _categoryApi.GetAllCategory_API();
                    var CategoryToDelete = allCategorys.FirstOrDefault(x => x.id == id);

                    if (CategoryToDelete != null)
                    {
                        bool result = await _categoryApi.DeleteCategory_API(id);
                        if (result)
                            successCount++;
                    }
                }

                if (successCount > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully deleted {successCount} item(s)"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to delete items. Items may not exist or you don't have permission"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Product/Product_List
        public async Task<IActionResult> SearchProduct(string searchTerm, string filterBy = "number")
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json(new List<Product>());

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var departmentTask = _departmentApi.GetAllDepartment_API();
            var productTask = _productApi.GetAllProduct_API();
            await Task.WhenAll(departmentTask, productTask);

            var allDepartment = departmentTask.Result;
            var allProduct = productTask.Result;

            var selectProduct = new List<Product>();
            selectProduct = allProduct.OrderByDescending(y => y.id).ToList();

            List<Product> filteredProduct;

            if (searchTerm == "re_entrynovalue")
            {
                filteredProduct = selectProduct;
            }
            else
            {
                switch (filterBy.ToLower())
                {
                    case "title":
                        filteredProduct = selectProduct
                            .Where(x => x.item_title != null && x.item_title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "description":
                        filteredProduct = selectProduct
                            .Where(x => x.description != null && x.description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "responsible":
                        var filteredDepartmentData = allDepartment.Where(x => x.name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        filteredProduct = (from i in selectProduct
                                           join d in filteredDepartmentData on i.responsible equals d.id
                                           select i).ToList();
                        break;
                    case "quantity":
                        filteredProduct = selectProduct
                            .Where(x => x.quantity.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "number":
                    default:
                        filteredProduct = selectProduct
                            .Where(t => t.pro_number != null && t.pro_number.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                }
            }

            var result = filteredProduct.Select(t => new
            {
                t.pro_number,
                t.item_title,
                t.description,
                t.quantity,
                active = t.active ? "Active" : "Inactive",
                department_name = allDepartment.FirstOrDefault(d => d.id == t.responsible)?.name ?? ""
            });

            return Json(result);
        }

        /// <summary>
        /// Product/Product_List
        public async Task<IActionResult> SortProduct(string sortOrder = "asc")
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var productTask = _productApi.GetAllProduct_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            await Task.WhenAll(productTask, departmentTask);

            var allProduct = productTask.Result;
            var allDepartment = departmentTask.Result;

            var SelectProduct = new List<Product>();
            SelectProduct = allProduct.OrderByDescending(y => y.id).ToList();

            List<Product> sortedProducts;
            if (sortOrder.ToLower() == "desc")
                sortedProducts = SelectProduct.OrderBy(x => x.id).ToList();
            else
                sortedProducts = SelectProduct.OrderByDescending(x => x.id).ToList();

            var result = sortedProducts.Select(t => new {
                t.pro_number,
                t.item_title,
                t.description,
                t.quantity,
                active = t.active ? "Active" : "Inactive",
                department_name = allDepartment.FirstOrDefault(d => d.id == t.responsible)?.name ?? ""
            });

            return Json(result);
        }

        /// <summary>
        /// Product/Product_List
        public async Task<IActionResult> FilterProductByStatus(string status = "all")
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var productTask = _productApi.GetAllProduct_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            await Task.WhenAll(productTask, departmentTask);

            var allProduct = productTask.Result;
            var allDepartment = departmentTask.Result;

            var SelectProduct = new List<Product>();
            SelectProduct = allProduct.OrderByDescending(y => y.id).ToList();

            List<Product> filteredProducts;

            switch (status.ToLower())
            {
                case "active":
                    filteredProducts = SelectProduct.Where(t => t.active == true).ToList();
                    break;
                case "inactive":
                    filteredProducts = SelectProduct.Where(t => t.active == false).ToList();
                    break;
                case "all":
                default:
                    filteredProducts = SelectProduct;
                    break;
            }

            var result = filteredProducts.Select(t => new {
                t.pro_number,
                t.item_title,
                t.description,
                t.quantity,
                active = t.active ? "Active" : "Inactive",
                department_name = allDepartment.FirstOrDefault(d => d.id == t.responsible)?.name ?? ""
            });

            return Json(result);
        }

        /// <summary>
        /// Product/Product_List
        [HttpPost]
        public async Task<IActionResult> DeleteProduct([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for deletion" });

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                var productTask = _productApi.GetAllProduct_API();
                await Task.WhenAll(productTask);

                var allProduct = productTask.Result;

                foreach (var id in ids)
                {
                    var ProductToDelete = allProduct.FirstOrDefault(x => x.id == id);

                    if (ProductToDelete != null)
                    {
                        bool result = await _productApi.DeleteProduct_API(id);
                        if (result)
                            successCount++;
                    }
                }

                if (successCount > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully deleted {successCount} item(s)"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to delete items. Items may not exist or you don't have permission"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Department/Department_List
        public async Task<IActionResult> SearchDepartment(string searchTerm, string filterBy = "title")
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json(new List<Department>());

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allDepartment = await _departmentApi.GetAllDepartment_API();
            var AllDepartment = new List<Department>();
            AllDepartment = allDepartment.OrderByDescending(y => y.id).ToList();

            List<Department> filteredDepartments;

            if (searchTerm == "re_entrynovalue")
            {
                filteredDepartments = AllDepartment;
            }
            else
            {
                switch (filterBy.ToLower())
                {

                    case "description":
                        filteredDepartments = AllDepartment
                            .Where(x => x.description != null && x.description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "name":
                    default:
                        filteredDepartments = AllDepartment
                            .Where(t => t.name != null && t.name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                }
            }

            var result = filteredDepartments.Select(t => new {
                t.name,
                description = t.description != null ? t.description : ""
            });

            return Json(result);
        }

        /// <summary>
        /// Department/Department_List
        [HttpPost]
        public async Task<IActionResult> DeleteDepartment([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for deletion" });

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                foreach (var id in ids)
                {
                    var allDepartments = await _departmentApi.GetAllDepartment_API();
                    var DepartmentToDelete = allDepartments.FirstOrDefault(x => x.id == id);

                    if (DepartmentToDelete != null)
                    {
                        bool result = await _departmentApi.DeleteDepartment_API(id);
                        if (result)
                            successCount++;
                    }
                }

                if (successCount > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully deleted {successCount} item(s)"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to delete items. Items may not exist or you don't have permission"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Role/Role_List
        public async Task<IActionResult> SearchRole(string searchTerm, string filterBy = "role")
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json(new List<Role>());

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var RoleTask = await _roleApi.GetAllRole_API();
            var AllRole = new List<Role>();
            AllRole = RoleTask.ToList();

            List<Role> filteredRole;

            if (searchTerm == "re_entrynovalue")
            {
                filteredRole = AllRole;
            }
            else
            {
                switch (filterBy.ToLower())
                {
                    case "role":
                    default:
                        filteredRole = AllRole
                            .Where(t => t.role != null && t.role.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                }
            }

            var result = filteredRole.Select(t => new {
                t.role
            });

            return Json(result);
        }

        /// <summary>
        /// Role/Role_List
        [HttpPost]
        public async Task<IActionResult> DeleteRole([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for deletion" });

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                foreach (var id in ids)
                {
                    var allRole = await _roleApi.GetAllRole_API();
                    var RoleToDelete = allRole.FirstOrDefault(x => x.id == id);

                    if (RoleToDelete != null)
                    {
                        bool result = await _roleApi.DeleteRole_API(id);
                        if (result)
                            successCount++;
                    }
                }

                if (successCount > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully deleted {successCount} item(s)"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to delete items. Items may not exist or you don't have permission"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// User/User_List
        public async Task<IActionResult> SearchUser(string searchTerm, string filterBy = "number")
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json(new List<User>());

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allUsers = await _userApi.GetAllUser_API();
            var userList = allUsers.OrderByDescending(y => y.id).ToList();

            var allDepartments = await _departmentApi.GetAllDepartment_API();
            var allRoles = await _roleApi.GetAllRole_API();

            List<User> filteredUsers;

            if (searchTerm == "re_entrynovalue")
            {
                filteredUsers = userList;
            }
            else
            {
                switch (filterBy.ToLower())
                {
                    case "emp_id":
                        filteredUsers = userList
                            .Where(t => t.emp_id != null && t.emp_id.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "fullname":
                        filteredUsers = userList
                            .Where(t => t.fullname != null && t.fullname.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;

                    case "department":
                        var filterDepartments = allDepartments.Where(x => x.name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        filteredUsers = (from i in userList
                                         join d in filterDepartments on i.department_id equals d.id
                                         select i).ToList();
                        break;
                    case "mobile":
                        filteredUsers = userList
                            .Where(t => t.mobile_phone != null && t.mobile_phone.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "role":
                        var filterRoles = allRoles.Where(x => x.role.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        filteredUsers = (from i in userList
                                         join d in filterRoles on i.role_id equals d.id
                                         select i).ToList();
                        break;
                    case "r_manager":
                        filteredUsers = userList
                            .Where(t => (t.r_manager ? "Yes" : "No").Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    default:
                        filteredUsers = userList;
                        break;
                }
            }

            var result = filteredUsers.Select(t => new {
                t.id,
                t.emp_id,
                t.fullname,
                t.gender,
                departmentName = allDepartments.FirstOrDefault(d => d.id == t.department_id)?.name ?? "",
                t.mobile_phone,
                role = allRoles.FirstOrDefault(d => d.id == t.role_id)?.role ?? "",
                r_manager = t.r_manager ? "Yes" : "No",
                active = t.active == true ? "Active" : "Blocked"
            });

            return Json(result);
        }

        /// <summary>
        /// User/User_List
        public async Task<IActionResult> FilterUserByStatus(string status = "all")
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allUsers = await _userApi.GetAllUser_API();
            var userList = allUsers.OrderByDescending(y => y.id).ToList();

            var allDepartments = await _departmentApi.GetAllDepartment_API();
            var allRoles = await _roleApi.GetAllRole_API();

            List<User> filteredUsers;

            switch (status.ToLower())
            {
                case "active":
                    filteredUsers = userList.Where(t => t.active == true).ToList();
                    break;
                case "blocked":
                    filteredUsers = userList.Where(t => t.active == false).ToList();
                    break;
                case "all":
                default:
                    filteredUsers = userList;
                    break;
            }

            var result = filteredUsers.Select(t => new {
                t.id,
                t.emp_id,
                t.fullname,
                t.gender,
                departmentName = allDepartments.FirstOrDefault(d => d.id == t.department_id)?.name ?? "",
                t.mobile_phone,
                role = allRoles.FirstOrDefault(d => d.id == t.role_id)?.role ?? "",
                r_manager = t.r_manager ? "Yes" : "No",
                active = t.active == true ? "Active" : "Blocked"
            });

            return Json(result);
        }

        /// <summary>
        /// User/User_List
        public async Task<IActionResult> SortUser(string sortOrder = "asc")
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allUsers = await _userApi.GetAllUser_API();
            var userList = allUsers.OrderByDescending(y => y.id).ToList();

            var allDepartments = await _departmentApi.GetAllDepartment_API();
            var allRoles = await _roleApi.GetAllRole_API();

            List<User> sortedUsers;

            if (sortOrder.ToLower() == "desc")
                sortedUsers = userList.OrderBy(x => x.id).ToList();
            else
                sortedUsers = userList.OrderByDescending(x => x.id).ToList();

            var result = sortedUsers.Select(t => new {
                t.id,
                t.emp_id,
                t.fullname,
                t.gender,
                departmentName = allDepartments.FirstOrDefault(d => d.id == t.department_id)?.name ?? "",
                t.mobile_phone,
                role = allRoles.FirstOrDefault(d => d.id == t.role_id)?.role ?? "",
                r_manager = t.r_manager ? "Yes" : "No",
                active = t.active == true ? "Active" : "Blocked"
            });

            return Json(result);
        }

        /// <summary>
        /// User/User_List
        [HttpPost]
        public async Task<IActionResult> BlockUsers([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for blocked" });

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allUsers = await _userApi.GetAllUser_API();

            foreach (var id in ids)
            {
                var userToBlock = allUsers.FirstOrDefault(t => t.id == id);

                if (userToBlock != null)
                {
                    // Block User
                    userToBlock.active = false;
                    await _userApi.UpdateUser_API(userToBlock);
                }
            }

            var userList = allUsers.Where(x => x.id != currentUser.id).OrderByDescending(y => y.id).ToList();

            var allDepartments = await _departmentApi.GetAllDepartment_API();
            var allRoles = await _roleApi.GetAllRole_API();

            List<User> ReNewUsers = userList;

            var result = ReNewUsers.Select(t => new {
                t.id,
                t.emp_id,
                t.fullname,
                t.gender,
                departmentName = allDepartments.FirstOrDefault(d => d.id == t.department_id)?.name ?? "",
                t.mobile_phone,
                role = allRoles.FirstOrDefault(d => d.id == t.role_id)?.role ?? "",
                r_manager = t.r_manager ? "Yes" : "No",
                active = t.active == true ? "Active" : "Blocked"
            });

            return Json(result);
        }

        /// <summary>
        /// User/User_List
        [HttpPost]
        public async Task<IActionResult> ActiveUsers([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for active" });

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allUsers = await _userApi.GetAllUser_API();

            foreach (var id in ids)
            {
                var userToBlock = allUsers.FirstOrDefault(t => t.id == id);

                if (userToBlock != null)
                {
                    // Active User
                    userToBlock.active = true;
                    await _userApi.UpdateUser_API(userToBlock);
                }
            }

            var userList = allUsers.Where(x => x.id != currentUser.id).OrderByDescending(y => y.id).ToList();

            var allDepartments = await _departmentApi.GetAllDepartment_API();
            var allRoles = await _roleApi.GetAllRole_API();

            List<User> ReNewUsers = userList;

            var result = ReNewUsers.Select(t => new {
                t.id,
                t.emp_id,
                t.fullname,
                t.gender,
                departmentName = allDepartments.FirstOrDefault(d => d.id == t.department_id)?.name ?? "",
                t.mobile_phone,
                role = allRoles.FirstOrDefault(d => d.id == t.role_id)?.role ?? "",
                r_manager = t.r_manager ? "Yes" : "No",
                active = t.active == true ? "Active" : "Blocked"
            });

            return Json(result);
        }

        /// <summary>
        /// User/Approve User
        [HttpPost]
        public async Task<IActionResult> DeleteUser([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for deletion" });

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                foreach (var id in ids)
                {
                    var allUser = await _userApi.GetAllUser_API();
                    var UserToDelete = allUser.FirstOrDefault(x => x.id == id);

                    if (UserToDelete != null)
                    {
                        bool result = await _userApi.DeleteTodo_API(id);
                        if (result)
                            successCount++;
                    }
                }

                if (successCount > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully deleted {successCount} item(s)"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to delete items. Items may not exist or you don't have permission"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Request/All
        public async Task<IActionResult> SearchRequest(string searchWord, string searchTerm, string filterBy = "number")
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json(new List<Incident>());

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allReqs = await _reqApi.GetAllRequest_API();
            var userReqs = new List<Request>();
            if (searchWord == "req")
                userReqs = allReqs.OrderByDescending(x => x.id).ToList();
            else if (searchWord == "req_user")
                userReqs = allReqs.Where(x => x.sender == currentUser.id).OrderByDescending(x => x.id).ToList();
            else if (searchWord == "assign_work")
                userReqs = allReqs.Where(x => x.assignment_group == currentUser.department_id && x?.assigned_to == null && x.state != "Rejected" && x.state != "Completed").OrderByDescending(x => x.id).ToList();
            else if (searchWord == "req_group")
                userReqs = allReqs.Where(x => x.assignment_group == currentUser.department_id).OrderByDescending(x => x.id).ToList();
            else if (searchWord == "req_tome")
                userReqs = allReqs.Where(x => x.assigned_to == currentUser.id).OrderByDescending(y => y.id).ToList();
            else
                userReqs = allReqs.OrderByDescending(x => x.id).ToList();

            var productTask = _productApi.GetAllProduct_API();
            var userTask = _userApi.GetAllUser_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            var requestTask = _reqApi.GetAllRequest_API();
            await Task.WhenAll(productTask, userTask, departmentTask, requestTask);

            var allProduct = productTask.Result;
            var allUser = userTask.Result;
            var allDepartment = departmentTask.Result;
            var allRequest = requestTask.Result;

            List<Request> filteredReqs;

            if (searchTerm == "re_entrynovalue")
            {
                filteredReqs = userReqs;
            }
            else
            {
                switch (filterBy.ToLower())
                {
                    case "product_id":
                        var filterProducts = allProduct.Where(x => x.pro_number.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        filteredReqs = (from i in userReqs
                                        join p in filterProducts on i.pro_id equals p.id
                                        select i).ToList();
                        break;
                    case "user":
                        var filterUsers = allUser.Where(x => x.fullname.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        filteredReqs = (from i in userReqs
                                        join u in filterUsers on i.sender equals u.id
                                        select i).ToList();
                        break;
                    case "assignment_group":
                        var filterDepartments = allDepartment.Where(x => x.name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        filteredReqs = (from i in userReqs
                                        join d in filterDepartments on i.assignment_group equals d.id
                                        select i).ToList();
                        break;
                    case "assigned_to":
                        var filterAssigned_To = allUser.Where(x => x.fullname.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        filteredReqs = (from i in userReqs
                                        join d in filterAssigned_To on i.assigned_to equals d.id
                                        select i).ToList();
                        break;
                    case "quantity":
                        filteredReqs = userReqs
                            .Where(x => x.quantity.ToString().Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "updated_by":
                        var filterUsers_Updated = allUser.Where(x => x.fullname.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                        filteredReqs = (from i in userReqs
                                        join u in filterUsers_Updated on i.updated_by equals u.id
                                        select i).ToList();
                        break;
                    case "create_date":
                        filteredReqs = userReqs
                            .Where(t => t.create_date != null && t.create_date.ToString("yyyy-MM-dd HH:mm:ss").Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                    case "type":
                        var filteredByType = userReqs.Where(t =>
                            (t.req_type == false
                                ? "Hardware"
                                : (t.erp_version?.ToLower() == "erp 8"
                                    ? "ERP Account"
                                    : "ERP"))
                            .Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                        ).ToList();

                        filteredReqs = filteredByType;
                        break;
                    case "number":
                    default:
                        filteredReqs = userReqs
                            .Where(t => t.req_id != null && t.req_id.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        break;
                }
            }

            var result = filteredReqs.Select(t => new
            {
                t.id,
                t.req_id,
                t.state,
                req_type_name = t.req_type == false
                    ? "Hardware"
                    : (t.erp_version.ToLower() == "erp 8" ? "ERP Account" : "ERP"),
                user_name = allUser.FirstOrDefault(u => u.id == t.sender)?.fullname ?? "",
                assignment_group = allDepartment.FirstOrDefault(x => x.id == t.assignment_group).name,
                assigned_to = t.assigned_to != null ? allUser.FirstOrDefault(x => x.id == t.assigned_to).fullname : "-",
                create_date = t.create_date.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Json(result);
        }

        /// <summary>
        /// Request/All
        public async Task<IActionResult> FilterRequestByStatus(string filterword, string status = "all")
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allReqs = await _reqApi.GetAllRequest_API();

            var userReqs = new List<Request>();
            if (filterword == "req_filter")
                userReqs = allReqs.OrderByDescending(x => x.id).ToList();
            else if (filterword == "req_filter_user")
                userReqs = allReqs.Where(x => x.sender == currentUser.id).OrderByDescending(x => x.id).ToList();
            else if (filterword == "req_filter_group")
                userReqs = allReqs.Where(x => x.assignment_group == currentUser.department_id).OrderByDescending(x => x.id).ToList();
            else if (filterword == "req_filter_tome")
                userReqs = allReqs.Where(x => x.assigned_to == currentUser.id).OrderByDescending(y => y.id).ToList();
            else
                userReqs = allReqs.OrderByDescending(x => x.id).ToList();

            var productTask = _productApi.GetAllProduct_API();
            var userTask = _userApi.GetAllUser_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            var requestTask = _reqApi.GetAllRequest_API();
            await Task.WhenAll(productTask, userTask, departmentTask, requestTask);

            var allProduct = productTask.Result;
            var allUser = userTask.Result;
            var allDepartment = departmentTask.Result;
            var allRequest = requestTask.Result;

            List<Request> filteredReqs;

            switch (status.ToLower())
            {
                case "pending":
                    filteredReqs = userReqs.Where(t => t.state == "Pending").ToList();
                    break;
                case "rejected":
                    filteredReqs = userReqs.Where(t => t.state == "Rejected").ToList();
                    break;
                case "in_progress":
                    filteredReqs = userReqs.Where(t => t.state == "In Progress").ToList();
                    break;
                case "on_hold":
                    filteredReqs = userReqs.Where(t => t.state == "On-Hold").ToList();
                    break;
                case "completed":
                    filteredReqs = userReqs.Where(t => t.state == "Completed").ToList();
                    break;
                case "all":
                default:
                    filteredReqs = userReqs;
                    break;
            }

            var result = filteredReqs.Select(t => new {
                t.id,
                t.req_id,
                t.state,
                req_type_name = t.req_type == false
                    ? "Hardware"
                    : (t.erp_version.ToLower() == "erp 8" ? "ERP Account" : "ERP"),
                user_name = allUser.FirstOrDefault(u => u.id == t.sender)?.fullname ?? "",
                assignment_group = allDepartment.FirstOrDefault(x => x.id == t.assignment_group).name,
                assigned_to = t.assigned_to != null ? allUser.FirstOrDefault(x => x.id == t.assigned_to).fullname : "-",
                create_date = t.create_date.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Json(result);
        }

        /// <summary>
        /// Request/All
        public async Task<IActionResult> SortRequest(string sortWord, string sortOrder = "asc")
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allReqs = await _reqApi.GetAllRequest_API();
            var userReqs = new List<Request>();
            if (sortWord == "req_sort")
                userReqs = allReqs.OrderByDescending(x => x.id).ToList();
            else if (sortWord == "req_sort_user")
                userReqs = allReqs.Where(x => x.sender == currentUser.id).OrderByDescending(x => x.id).ToList();
            else if (sortWord == "req_sort_group")
                userReqs = allReqs.Where(x => x.assignment_group == currentUser.department_id).OrderByDescending(x => x.id).ToList();
            else if (sortWord == "assign_work")
                userReqs = allReqs.Where(x => x.assignment_group == currentUser.department_id && x?.assigned_to == null && x.state != "Rejected" && x.state != "Completed").OrderByDescending(x => x.id).ToList();
            else if (sortWord == "req_sort_tome")
                userReqs = allReqs.Where(x => x.assigned_to == currentUser.id).OrderByDescending(y => y.id).ToList();
            else
                userReqs = allReqs.OrderByDescending(x => x.id).ToList();

            var productTask = _productApi.GetAllProduct_API();
            var userTask = _userApi.GetAllUser_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            var requestTask = _reqApi.GetAllRequest_API();
            await Task.WhenAll(productTask, userTask, departmentTask, requestTask);

            var allProduct = productTask.Result;
            var allUser = userTask.Result;
            var allDepartment = departmentTask.Result;
            var allRequest = requestTask.Result;

            List<Request> sortedRequests;
            if (sortOrder.ToLower() == "desc")
                sortedRequests = userReqs.OrderBy(x => x.id).ToList();
            else
                sortedRequests = userReqs.OrderByDescending(x => x.id).ToList();

            var result = sortedRequests.Select(t => new {
                t.id,
                t.req_id,
                t.state,
                req_type_name = t.req_type == false
                    ? "Hardware"
                    : (t.erp_version.ToLower() == "erp 8" ? "ERP Account" : "ERP"),
                user_name = allUser.FirstOrDefault(u => u.id == t.sender)?.fullname ?? "",
                assignment_group = allDepartment.FirstOrDefault(x => x.id == t.assignment_group).name,
                assigned_to = t.assigned_to != null ? allUser.FirstOrDefault(x => x.id == t.assigned_to).fullname : "-",
                create_date = t.create_date.ToString("yyyy-MM-dd HH:mm:ss")
            });

            return Json(result);
        }

        /// <summary>
        /// Request/All
        [HttpPost]
        public async Task<IActionResult> DeleteRequests([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for deletion" });

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                foreach (var id in ids)
                {
                    var allReqs = await _reqApi.GetAllRequest_API();
                    var reqToDelete = allReqs.FirstOrDefault(x => x.id == id);

                    if (reqToDelete != null)
                    {
                        bool result = await _reqApi.DeleteRequest_API(id);
                        if (result)
                            successCount++;
                    }
                }

                if (successCount > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully deleted {successCount} item(s)"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to delete items. Items may not exist or you don't have permission"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Request/Req_Info
        [HttpPost]
        public async Task<JsonResult> CompletedRequest(Request req, string resolveType, string resolveNotes)
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            if (string.IsNullOrEmpty(resolveType) || string.IsNullOrEmpty(resolveNotes))
                return Json(new { success = false, message = "Resolution type and resolution notes are required." });

            try
            {
                var reqData = await _reqApi.FindByIDRequest_API(req.id);

                if (reqData == null)
                    return Json(new { success = false, message = "Request not found" });

                reqData.description = req.description;
                reqData.assigned_to = req.assigned_to == 0 ? null : req.assigned_to;

                // resolved data
                reqData.erp_resolution = resolveNotes;
                reqData.erp_resolved_date = DateTime.Now;
                reqData.erp_resolution_type = resolveType;

                // Force state to Resolved regardless of what was in the form
                reqData.state = "Completed";
                reqData.updated_by = currentUser.id;

                bool result = await _reqApi.UpdateRequest_API(reqData);

                if (result)
                    return Json(new { success = true });
                else
                    return Json(new { success = false, message = "Failed to update request" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Request/Req_Info
        [HttpPost]
        public async Task<JsonResult> RejectedRequest(int req_id)
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                var reqData = await _reqApi.FindByIDRequest_API(req_id);
                
                var allRole = await _roleApi.GetAllRole_API();
                currentUser.Role = allRole.FirstOrDefault(x => x.id == currentUser.role_id);

                if (reqData == null)
                    return Json(new { success = false, message = "Request not found" });

                reqData.state = "Rejected";
                reqData.updated_by = currentUser.id;

                if(reqData.req_type == false)
                {
                    var productData = await _productApi.FindByIDProduct_API(reqData.pro_id != null ? (int)reqData.pro_id : 0);

                    if (productData != null)
                    {
                        int pro_count = productData.quantity + (reqData.quantity ?? 0);
                        if (pro_count > 0)
                        {
                            productData.active = true;
                            productData.quantity = pro_count;

                            await _productApi.UpdateProduct_API(productData);
                        }
                    }
                }

                bool result = await _reqApi.UpdateRequest_API(reqData);

                if (result)
                    return Json(new { success = true, role = currentUser.Role.role.ToLower() });
                else
                    return Json(new { success = false, message = "Failed to update request" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Request/Req_Info
        [HttpPost]
        public async Task<JsonResult> ReopenRequest(int req_id)
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                var reqData = await _reqApi.FindByIDRequest_API(req_id);

                var allRole = await _roleApi.GetAllRole_API();
                currentUser.Role = allRole.FirstOrDefault(x => x.id == currentUser.role_id);

                if (reqData == null)
                    return Json(new { success = false, message = "Request not found" });

                reqData.state = "Pending";
                reqData.updated_by = currentUser.id;

                if(reqData.req_type == false)
                {
                    var productData = await _productApi.FindByIDProduct_API(reqData.pro_id != null ? (int)reqData.pro_id : 0);

                    if (productData != null)
                    {
                        var pro_count = productData.quantity - reqData.quantity;
                        if (pro_count < 0)
                            return Json(new { success = false, message = "Not enough product" });
                        else
                        {
                            if (pro_count <= 0)
                                productData.active = false;
                            productData.quantity = (pro_count ?? 0);
                            await _productApi.UpdateProduct_API(productData);
                        }
                    }
                }

                bool result = await _reqApi.UpdateRequest_API(reqData);

                if (result)
                    return Json(new { success = true, role = currentUser.Role.role.ToLower() });
                else
                    return Json(new { success = false, message = "Failed to update request" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// IncidentManagement/Inc_Info_Form
        public async Task<IActionResult> GetReqResolutionHistory(int reqId)
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                var req = await _reqApi.FindByIDRequest_API(reqId);

                if (req == null)
                    return Json(new { success = false, message = "Request not found" });

                if (req.state != "Completed")
                    return Json(new { success = false, message = "Request is not completed yet" });

                var resolvedBy = new ITSM_DomainModelEntity.Models.User();
                resolvedBy = await _userApi.FindByIDUser_API(req.assigned_to != null ? (int)req.assigned_to : 0);

                var resolvedByName = resolvedBy != null ? resolvedBy.fullname : "Unknown";

                var resolutionData = new
                {
                    req.id,
                    erp_resolution_type = req.erp_resolution_type,
                    erp_resolution = req.erp_resolution,
                    erp_resolved_date = req.erp_resolved_date?.ToString("yyyy-MM-dd HH:mm:ss"),
                    assigned_to = req.assigned_to,
                    resolved_by_name = resolvedByName,
                    resolved_by_avatar = resolvedBy?.photo,
                    resolved_photo_type = resolvedBy?.photo_type,
                    user_id = resolvedBy?.id
                };

                return Json(new { success = true, resolution = resolutionData });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Category/Subategory_List
        [HttpPost]
        public async Task<IActionResult> DeleteSubcategory([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for deletion" });

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                foreach (var id in ids)
                {
                    var allSUcategorys = await _subcategoryApi.GetAllSubcategory_API();
                    var SucategoryToDelete = allSUcategorys.FirstOrDefault(x => x.id == id);

                    if (SucategoryToDelete != null)
                    {
                        bool result = await _subcategoryApi.DeleteSubcategory_API(id);
                        if (result)
                            successCount++;
                    }
                }

                if (successCount > 0)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Successfully deleted {successCount} item(s)"
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to delete items. Items may not exist or you don't have permission"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        public async Task<IActionResult> GetSubcategoriesByCategory(int categoryId)
        {
            try
            {
                var allSubcategories = await _reqsubcategoryApi.GetAllReq_Subcategory_API();
                var filteredSubcategories = allSubcategories
                    .Where(s => s.req_category_id == categoryId)
                    .OrderBy(s => s.name)
                    .ToList();

                return Json(new { success = true, data = filteredSubcategories });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> GetFunctionsBySubcategory(int subcategoryId)
        {
            try
            {
                var allFunctions = await _reqfunctionApi.GetAllReq_Function_API();
                var filteredFunctions = allFunctions
                    .Where(f => f.req_subcategory_id == subcategoryId)
                    .OrderBy(f => f.name)
                    .ToList();

                return Json(new { success = true, data = filteredFunctions });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}