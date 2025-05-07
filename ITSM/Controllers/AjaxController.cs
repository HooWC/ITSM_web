using Microsoft.AspNetCore.Mvc;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Http;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_DomainModelEntity.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System;

namespace ITSM.Controllers
{
    public class AjaxController : Controller
    {
        private readonly TokenService _tokenService;
        private readonly Todo_api _todoApi;
        private readonly Department_api _departmentApi;
        private readonly User_api _userApi;
        private readonly Incident_api _incApi;

        public AjaxController(IHttpContextAccessor httpContextAccessor)
        {
            _tokenService = new TokenService(httpContextAccessor);
            _todoApi = new Todo_api(httpContextAccessor);
            _departmentApi = new Department_api(httpContextAccessor);
            _userApi = new User_api(httpContextAccessor);
            _incApi = new Incident_api(httpContextAccessor);
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
        /// Search Todo list
        /// </summary>
        /// <param name="searchTerm">Search keyword</param>
        /// <param name="filterBy">Filter field, can be number/title/create_date</param>
        /// <returns>Matched Todo list</returns>
        public async Task<IActionResult> SearchTodo(string searchTerm, string filterBy = "number")
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json(new List<Todo>());

            var currentUser = _tokenService.GetUserInfo();
            if (currentUser == null)
                return Json(new { success = false, message = "Not logged in" });

            var allTodos = await _todoApi.GetAllTodo_API();
            var userTodos = allTodos.Where(x => x.user_id == currentUser.id).ToList();

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
                        .Where(t => t.create_date.ToString("yyyy-MM-dd").Contains(searchTerm))
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
        /// Filter the Todo list by status
        /// </summary>
        /// <param name="status">Status to filter: all/doing/completed</param>
        /// <returns>Filtered Todo list</returns>
        public async Task<IActionResult> FilterTodoByStatus(string status = "all")
        {
            var currentUser = _tokenService.GetUserInfo();
            if (currentUser == null)
                return Json(new { success = false, message = "Not logged in" });

            var allTodos = await _todoApi.GetAllTodo_API();
            var userTodos = allTodos.Where(x => x.user_id == currentUser.id).ToList();

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
        /// Sort the Todo list
        /// </summary>
        /// <param name="sortOrder">Sorting method: asc (ascending)/desc (descending)</param>
        /// <returns>The sorted Todo list</returns>
        public async Task<IActionResult> SortTodo(string sortOrder = "asc")
        {
            var currentUser = _tokenService.GetUserInfo();
            if (currentUser == null)
                return Json(new { success = false, message = "Not logged in" });

            var allTodos = await _todoApi.GetAllTodo_API();
            var userTodos = allTodos.Where(x => x.user_id == currentUser.id).ToList();

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
        /// Delete multiple Todo items
        /// </summary>
        /// <param name="ids">Todo item ID list to be deleted</param>
        /// <returns>Operation results</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteTodos([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for deletion" });

            // Get current user information
            var currentUser = _tokenService.GetUserInfo();
            if (currentUser == null)
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

        public async Task<IActionResult> DepartmentData()
        {
            var currentUser = _tokenService.GetUserInfo();
            if (currentUser == null)
                return Json(new { success = false, message = "Not logged in" });

            var allDepartment = await _departmentApi.GetAllDepartment_API();

            return Json(allDepartment);
        }

        /// <summary>
        /// 根据部门ID获取用户数据
        /// </summary>
        /// <param name="departmentId">部门ID</param>
        /// <returns>该部门下的用户列表</returns>
        [HttpPost]
        public async Task<IActionResult> AssignedToData(int departmentId)
        {
            var currentUser = _tokenService.GetUserInfo();
            if (currentUser == null)
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

        public async Task<IActionResult> SearchIncident(string searchTerm, string filterBy = "number")
        {
            if (string.IsNullOrEmpty(searchTerm))
                return Json(new List<Incident>());

            var currentUser = _tokenService.GetUserInfo();
            if (currentUser == null)
                return Json(new { success = false, message = "Not logged in" });

            var allIncs = await _incApi.GetAllIncident_API();
            var userIncs= allIncs.Where(x => x.sender == currentUser.id).ToList();

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

        public async Task<IActionResult> FilterIncidentByStatus(string status = "all")
        {
            var currentUser = _tokenService.GetUserInfo();
            if (currentUser == null)
                return Json(new { success = false, message = "Not logged in" });

            var allIncs = await _incApi.GetAllIncident_API();
            var userIncs = allIncs.Where(x => x.sender == currentUser.id).ToList();

            var allDepartments = await _departmentApi.GetAllDepartment_API();
            var allUsers = await _userApi.GetAllUser_API();

            List<Incident> filteredIncs;

            switch (status.ToLower())
            {
                case "pedding":
                    filteredIncs = allIncs.Where(t => t.state == "Pedding").ToList();
                    break;
                case "inprogress":
                    filteredIncs = allIncs.Where(t => t.state == "In Progress").ToList();
                    break;
                case "onhold":
                    filteredIncs = allIncs.Where(t => t.state == "On-Hold").ToList();
                    break;
                case "resolved":
                    filteredIncs = allIncs.Where(t => t.state == "Resolved").ToList();
                    break;
                case "closed":
                    filteredIncs = allIncs.Where(t => t.state == "Closed").ToList();
                    break;
                case "cancelled":
                    filteredIncs = allIncs.Where(t => t.state == "Cancelled").ToList();
                    break;
                case "all":
                default:
                    filteredIncs = allIncs;
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
        /// Personal/Todo
        /// Sort the Todo list
        /// </summary>
        /// <param name="sortOrder">Sorting method: asc (ascending)/desc (descending)</param>
        /// <returns>The sorted Todo list</returns>
        public async Task<IActionResult> SortIncident(string sortOrder = "asc")
        {
            var currentUser = _tokenService.GetUserInfo();
            if (currentUser == null)
                return Json(new { success = false, message = "Not logged in" });

            var allTodos = await _todoApi.GetAllTodo_API();
            var userTodos = allTodos.Where(x => x.user_id == currentUser.id).ToList();

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
        /// Delete multiple Todo items
        /// </summary>
        /// <param name="ids">Todo item ID list to be deleted</param>
        /// <returns>Operation results</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteIncidents([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return Json(new { success = false, message = "No items selected for deletion" });

            // Get current user information
            var currentUser = _tokenService.GetUserInfo();
            if (currentUser == null)
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
