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

        public AjaxController(IHttpContextAccessor httpContextAccessor)
        {
            _tokenService = new TokenService(httpContextAccessor);
            _todoApi = new Todo_api(httpContextAccessor);
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
    }
}
