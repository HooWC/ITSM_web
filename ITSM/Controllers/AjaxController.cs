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

            // Returns a JSON result, indicating a successful exit
            return Json(new { success = true, message = "Log out success" });
        }

        /// <summary>
        /// Search Todo list
        /// </summary>
        /// <param name="searchTerm">Search keyword</param>
        /// <param name="filterBy">Filter field, can be number/title/create_date</param>
        /// <returns>Matched Todo list</returns>
        public async Task<IActionResult> SearchTodo(string searchTerm, string filterBy = "number")
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return Json(new List<Todo>());
            }

            // 获取当前用户信息
            var currentUser = _tokenService.GetUserInfo();
            if (currentUser == null)
            {
                return Json(new { success = false, message = "未登录" });
            }

            // 获取用户的所有Todo
            var allTodos = await _todoApi.GetAllTodo_API();
            var userTodos = allTodos.Where(x => x.user_id == currentUser.id).ToList();

            // 根据筛选字段和搜索关键词筛选结果
            List<Todo> filteredTodos = new List<Todo>();
            
            switch (filterBy.ToLower())
            {
                case "number":
                    filteredTodos = userTodos.Where(t => t.todo_id != null && t.todo_id.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "title":
                    filteredTodos = userTodos.Where(t => t.title != null && t.title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                case "create_date":
                    filteredTodos = userTodos.Where(t => t.create_date.ToString("yyyy-MM-dd").Contains(searchTerm)).ToList();
                    break;
                default:
                    filteredTodos = userTodos.Where(t => t.todo_id != null && t.todo_id.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
            }

            return Json(filteredTodos);
        }

        /// <summary>
        /// Filter the Todo list by status
        /// </summary>
        /// <param name="status">Status to filter: all/doing/completed</param>
        /// <returns>Filtered Todo list</returns>
        public async Task<IActionResult> FilterTodoByStatus(string status = "all")
        {
            // Get current user info
            var currentUser = _tokenService.GetUserInfo();
            if (currentUser == null)
                return Json(new { success = false, message = "Not logged in" });

            // Get all todos for the user
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

            return Json(filteredTodos);
        }

        /// <summary>
        /// Sort the Todo list
        /// </summary>
        /// <param name="sortOrder">Sorting method: asc (ascending)/desc (descending)</param>
        /// <returns>The sorted Todo list</returns>
        public async Task<IActionResult> SortTodo(string sortOrder = "asc")
        {
            // Get current user info
            var currentUser = _tokenService.GetUserInfo();
            if (currentUser == null)
                return Json(new { success = false, message = "Not logged in" });

            // Get all todos for the user
            var allTodos = await _todoApi.GetAllTodo_API();
            var userTodos = allTodos.Where(x => x.user_id == currentUser.id).ToList();

            List<Todo> sortedTodos;

            if (sortOrder.ToLower() == "desc")
            {
                // OrderByDescending
                sortedTodos = userTodos.OrderByDescending(x => x.id).ToList();
            }
            else
            {
                // OrderBy
                sortedTodos = userTodos.OrderBy(x => x.id).ToList();
            }

            return Json(sortedTodos);
        }

        /// <summary>
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
                return RedirectToAction("Login", "Auth");

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
