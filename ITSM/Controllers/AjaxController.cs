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
                sortedTodos = userTodos.OrderByDescending(x => x.id).ToList();
            else
                sortedTodos = userTodos.OrderBy(x => x.id).ToList();

            var result = sortedTodos.Select(x => new {
                x.id,
                x.title,
                x.user_id,
                x.active,
                create_date = x.create_date.ToString("yyyy-MM-dd HH:mm:ss"),
                update_date = x.update_date.ToString("yyyy-MM-dd HH:mm:ss")
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

            // Get current user information
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                // Traverse all selected IDs and delete them one by one
                foreach (var id in ids)
                {
                    // Confirm that the Todo project exists and belongs to the current user
                    var allTodos = await _todoApi.GetAllTodo_API();
                    var todoToDelete = allTodos.FirstOrDefault(t => t.id == id && t.user_id == currentUser.id);
                    
                    if (todoToDelete != null)
                    {
                        // Call API to delete Todo
                        bool result = await _todoApi.DeleteTodo_API(id);
                        if (result)
                            successCount++;
                    }
                }

                if (successCount > 0)
                {
                    return Json(new { 
                        success = true, 
                        message = $"Successfully deleted {successCount} item(s)" 
                    });
                }
                else
                {
                    return Json(new { 
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
        public async Task<IActionResult> AssignedToData(int departmentId)
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                var usersInDepartment = await _userApi.GetAllUser_API();
                var result = usersInDepartment.Where(x => x.department_id == departmentId).ToList();

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
                userIncs = allIncs.Where(x => x.assigned_to == currentUser.id || x.updated_by == currentUser.id && x.state == "Resolved").OrderByDescending(y => y.id).ToList();
            else if (searchWord == "closed")
                userIncs = allIncs.Where(x => (x.assigned_to == currentUser.id || x.updated_by == currentUser.id) && x.state == "Closed").OrderByDescending(y => y.id).ToList();
            else if (searchWord == "tome")
                userIncs = allIncs.Where(x => x.assigned_to == currentUser.id).OrderByDescending(y => y.id).ToList();
            else if (searchWord == "toteam")
                userIncs = allIncs.Where(x => x.assignment_group == currentUser.department_id).OrderByDescending(y => y.id).ToList();
            else
                userIncs = allIncs.Where(x => x.sender == currentUser.id).OrderByDescending(y => y.id).ToList();

            var allDepartments = await _departmentApi.GetAllDepartment_API();
            var allUsers = await _userApi.GetAllUser_API();

            List<Incident> filteredIncs;

            switch (filterBy.ToLower())
            {
                case "short_description":
                    filteredIncs = userIncs
                        .Where(t => t.short_description != null && t.short_description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;
                case "priority":
                    filteredIncs = userIncs
                        .Where(t => t.priority != null && t.priority.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;
                case "state":
                    filteredIncs = userIncs
                        .Where(t => t.state != null && t.state.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;
                case "category":
                    filteredIncs = userIncs
                        .Where(t => t.category != null && t.category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();
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

            var result = filteredIncs.Select(t => new {
                t.id,
                t.inc_number,
                t.short_description,
                t.priority,
                t.state,
                t.category,
                assignment_group = allDepartments.FirstOrDefault(d => d.id == t.assignment_group)?.name ?? "",
                assigned_to = allUsers.FirstOrDefault(u => u.id == t.assigned_to)?.fullname ?? "",
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
                userIncs = allIncs.Where(x => x.assigned_to == currentUser.id).OrderByDescending(y => y.id).ToList();
            else if (filterword == "toteam")
                userIncs = allIncs.Where(x => x.assignment_group == currentUser.department_id).OrderByDescending(y => y.id).ToList();
            else
                userIncs = allIncs.Where(x => x.sender == currentUser.id).OrderByDescending(y => y.id).ToList();

            var allDepartments = await _departmentApi.GetAllDepartment_API();
            var allUsers = await _userApi.GetAllUser_API();

            List<Incident> filteredIncs;

            switch (status.ToLower())
            {
                case "pedding":
                    filteredIncs = userIncs.Where(t => t.state == "Pedding").ToList();
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
                t.short_description,
                t.priority,
                t.state,
                t.category,
                assignment_group = allDepartments.FirstOrDefault(d => d.id == t.assignment_group)?.name ?? "",
                assigned_to = allUsers.FirstOrDefault(u => u.id == t.assigned_to)?.fullname ?? "",
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
                userIncs = allIncs.Where(x => x.assigned_to == currentUser.id || x.updated_by == currentUser.id && x.state == "Resolved").OrderByDescending(y => y.id).ToList();
            else if (sortWord == "closed")
                userIncs = allIncs.Where(x => (x.assigned_to == currentUser.id || x.updated_by == currentUser.id) && x.state == "Closed").OrderByDescending(y => y.id).ToList();
            else if (sortWord == "tome")
                userIncs = allIncs.Where(x => x.assigned_to == currentUser.id).ToList();
            else if (sortWord == "toteam")
                userIncs = allIncs.Where(x => x.assignment_group == currentUser.department_id).ToList();
            else
                userIncs = allIncs.Where(x => x.sender == currentUser.id).OrderByDescending(y => y.id).ToList();

            var allDepartments = await _departmentApi.GetAllDepartment_API();
            var allUsers = await _userApi.GetAllUser_API();

            List<Incident> sortedIncidents;
            if (sortOrder.ToLower() == "desc")
                sortedIncidents = userIncs.OrderBy(x => x.id).ToList();
            else
                sortedIncidents = userIncs.OrderByDescending(x => x.id).ToList();

            var result = sortedIncidents.Select(t => new {
                t.id,
                t.inc_number,
                t.short_description,
                t.priority,
                t.state,
                t.category,
                assignment_group = allDepartments.FirstOrDefault(d => d.id == t.assignment_group)?.name ?? "",
                assigned_to = allUsers.FirstOrDefault(u => u.id == t.assigned_to)?.fullname ?? "",
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

            // Get current user information
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                // Traverse all selected IDs and delete them one by one
                foreach (var id in ids)
                {
                    // Confirm that the Incident project exists and belongs to the current user
                    var allIncs = await _incApi.GetAllIncident_API();
                    var incToDelete = allIncs.FirstOrDefault(x => x.id == id && x.sender == currentUser.id);

                    if (incToDelete != null)
                    {
                        // Call API to delete Incident
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
        public async Task<IActionResult> AddNote(int incidentId, string message)
        {
            if (string.IsNullOrEmpty(message))
                return Json(new { success = false, message = "Note content cannot be empty" });

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                // Get all notes to generate new note numbers
                var allNotes = await _noteApi.GetAllNote_API();
                string noteNumber = "NOT0001";

                // If a note exists, generate a new number based on the maximum number
                if (allNotes.Any())
                {
                    var maxNoteNumber = allNotes
                    .Where(n => n.note_number != null && n.note_number.StartsWith("NOT"))
                    .Select(n => n.note_number)
                    .DefaultIfEmpty("NOT0000")
                    .Max();

                    if (maxNoteNumber != null && maxNoteNumber.Length >= 7)
                    {
                        if (int.TryParse(maxNoteNumber.Substring(3), out int numberPart))
                        {
                            noteNumber = $"NOT{(numberPart + 1).ToString("D4")}";
                        }
                    }
                }

                // Create a new note
                var newNote = new ITSM_DomainModelEntity.Models.Note
                {
                    note_number = noteNumber,
                    incident_id = incidentId,
                    user_id = currentUser.id,
                    message = message
                };

                // Call API to create a note
                bool result = await _noteApi.CreateNote_API(newNote);
                
                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Note added successfully",
                        note = new
                        {
                            note_number = noteNumber,
                            user_name = currentUser.fullname,
                            user_avatar = currentUser.photo,
                            create_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            message = message
                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Note addition failed" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// IncidentManagement/Inc_Info_Form
        public async Task<IActionResult> GetNotesByIncident(int incidentId)
        {
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                var allNotes = await _noteApi.GetAllNote_API();
                var incidentNotes = allNotes
                    .Where(n => n.incident_id == incidentId)
                    .OrderByDescending(n => n.create_date)
                    .ToList();

                // Get all relevant user information
                var userIds = incidentNotes.Select(n => n.user_id).Distinct().ToList();
                var allUsers = await _userApi.GetAllUser_API();
                var relatedUsers = allUsers.Where(u => userIds.Contains(u.id)).ToList();

                var result = incidentNotes.Select(note => new
                {
                    id = note.id,
                    note_number = note.note_number,
                    user_id = note.user_id,
                    user_name = relatedUsers.FirstOrDefault(u => u.id == note.user_id)?.fullname ?? "Unknown",
                    user_avatar = relatedUsers.FirstOrDefault(u => u.id == note.user_id)?.photo,
                    create_date = note.create_date.ToString("yyyy-MM-dd HH:mm:ss"),
                    message = note.message
                });

                return Json(new
                {
                    success = true,
                    notes = result
                });
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
            // current user info
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            if (string.IsNullOrEmpty(resolveType) || string.IsNullOrEmpty(resolveNotes))
                return Json(new { success = false, message = "Resolution type and notes are required" });

            try
            {
                // Get original incident data
                var incData = await _incApi.FindByIDIncident_API(inc.id);

                if (incData == null)
                    return Json(new { success = false, message = "Incident not found" });

                // Update all incident fields from the form
                incData.short_description = inc.short_description;
                incData.describe = inc.describe;
                incData.impact = inc.impact;
                incData.urgency = inc.urgency;
                incData.priority = inc.priority;
                incData.category = inc.category;
                incData.subcategory = inc.subcategory;
                incData.assignment_group = inc.assignment_group;
                incData.assigned_to = inc.assigned_to == 0 ? null : inc.assigned_to;
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
                // Get incident information
                var incident = await _incApi.FindByIDIncident_API(incidentId);

                if (incident == null)
                    return Json(new { success = false, message = "Incident not found" });

                if (incident.state != "Resolved")
                    return Json(new { success = false, message = "Incident is not resolved yet" });

                // Get resolver information
                var resolvedBy = new ITSM_DomainModelEntity.Models.User();
                if (incident.resolved_by.HasValue)
                    resolvedBy = await _userApi.FindByIDUser_API(incident.resolved_by.Value);
                var resolvedByName = resolvedBy != null ? resolvedBy.fullname : "Unknown";

                // Organize return data
                var resolutionData = new
                {
                    incident.id,
                    resolved_type = incident.resolved_type,
                    resolution = incident.resolution,
                    resolved_date = incident.resolved_date?.ToString("yyyy-MM-dd HH:mm:ss"),
                    resolved_by = incident.resolved_by,
                    resolved_by_name = resolvedByName,
                    resolved_by_avatar = resolvedBy?.photo
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
                // Get the original event data
                var incData = await _incApi.FindByIDIncident_API(inc.id);

                if (incData == null)
                    return Json(new { success = false, message = "No event found" });

                // Update all event fields in the form
                incData.short_description = inc.short_description;
                incData.describe = inc.describe;
                incData.impact = inc.impact;
                incData.urgency = inc.urgency;
                incData.priority = inc.priority;
                incData.category = inc.category;
                incData.subcategory = inc.subcategory;
                incData.assignment_group = inc.assignment_group;
                incData.assigned_to = inc.assigned_to == 0 ? null : inc.assigned_to;

                // Set closing information
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
                // Get the original event data
                var incData = await _incApi.FindByIDIncident_API(inc.id);

                if (incData == null)
                    return Json(new { success = false, message = "No event found" });

                // Make sure the event is currently closed
                if (incData.state != "Closed")
                    return Json(new { success = false, message = "Only closed events can be reopened" });

                // Update all event fields in the form
                incData.short_description = inc.short_description;
                incData.describe = inc.describe;
                incData.impact = inc.impact;
                incData.urgency = inc.urgency;
                incData.priority = inc.priority;
                incData.category = inc.category;
                incData.subcategory = inc.subcategory;
                incData.assignment_group = inc.assignment_group;
                incData.assigned_to = inc.assigned_to == 0 ? null : inc.assigned_to;

                //Change status to Pedding
                incData.state = "Pedding";
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

            // Get current user information
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                // Traverse all selected IDs and delete them one by one
                foreach (var id in ids)
                {
                    // Confirm that the Incident project exists and belongs to the current user
                    var allFeeds = await _feedApi.GetAllFeedback_API();
                    var incToDelete = new Feedback();
                    if (word == "user")
                        incToDelete = allFeeds.FirstOrDefault(x => x.id == id && x.user_id == currentUser.id);
                    else
                        incToDelete = allFeeds.FirstOrDefault(x => x.id == id);

                    if (incToDelete != null)
                    {
                        // Call API to delete Incident
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
        public async Task<IActionResult> SearchCategory(string searchTerm, string filterBy = "number")
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json(new List<Category>());

            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            var allCategorys = await _categoryApi.GetAllCategory_API();
            var userCategory = new List<Category>();
            userCategory = allCategorys.OrderByDescending(y => y.id).ToList();

            List<Category> filteredCategorys;

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

            // Get current user information
            if (!IsUserLoggedIn(out var currentUser))
                return Json(new { success = false, message = "Not logged in" });

            try
            {
                int successCount = 0;

                // Traverse all selected IDs and delete them one by one
                foreach (var id in ids)
                {
                    // Confirm that the Incident project exists and belongs to the current user
                    var allCategorys = await _categoryApi.GetAllCategory_API();
                    var CategoryToDelete = allCategorys.FirstOrDefault(x => x.id == id);

                    if (CategoryToDelete != null)
                    {
                        // Call API to delete Incident
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


    }
}
