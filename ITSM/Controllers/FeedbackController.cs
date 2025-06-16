using System.Data;
using System.Runtime.Intrinsics.Arm;
using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserService _userService;
        private readonly User_api _userApi;
        private readonly Todo_api _todoApi;
        private readonly Feedback_api _feedbackApi;
        private readonly Incident_api _incApi;
        private readonly Knowledge_api _knowledgeApi;
        private readonly Request_api _reqApi;
        private readonly Department_api _depApi;
        private readonly Role_api _roleApi;

        public FeedbackController(IHttpContextAccessor httpContextAccessor, UserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userApi = new User_api(httpContextAccessor);
            _todoApi = new Todo_api(httpContextAccessor);
            _feedbackApi = new Feedback_api(httpContextAccessor);
            _incApi = new Incident_api(httpContextAccessor);
            _knowledgeApi = new Knowledge_api(httpContextAccessor);
            _reqApi = new Request_api(httpContextAccessor);
            _depApi = new Department_api(httpContextAccessor);
            _roleApi = new Role_api(httpContextAccessor);
            _userService = userService;
        }

        public async Task<IActionResult> All_Feedback_List()
        {
            var feedList = await GetCommonFeedbackData();  
            return View(feedList);  
        }

        public async Task<IActionResult> Feedback_List()
        {
            var feedList = await GetCommonFeedbackData();  
            return View(feedList);  
        }

        private async Task<AllModelVM> GetCommonFeedbackData()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var feedTask = _feedbackApi.GetAllFeedback_API();
            var userTask = _userApi.GetAllUser_API();
            var roleTask = _roleApi.GetAllRole_API();
            await Task.WhenAll(feedTask, userTask, roleTask);

            var allFeed = feedTask.Result;
            var allUsers = userTask.Result;
            var allRoles = roleTask.Result;

            var feedList = currentUser.role_id == allRoles.Where(x => x.role.ToLower() == "user" || x.role.ToLower() == "itil").FirstOrDefault()?.id
                ? allFeed.Where(x => x.user_id == currentUser.id).OrderByDescending(y => y.id).ToList()
                : allFeed.OrderByDescending(y => y.id).ToList();

            foreach (var feedback in feedList)
                feedback.User = allUsers.FirstOrDefault(x => x.id == feedback.user_id);

            return new AllModelVM
            {
                user = currentUser,
                FeedbackList = feedList,
                noteMessageCount = noteMessageCount
            };
        }

        public async Task<IActionResult> Feedback_Create()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var model =  new AllModelVM()
            {
                user = currentUser,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Feedback_Create(Feedback feed)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var model = new AllModelVM()
            {
                user = currentUser,
                noteMessageCount = noteMessageCount
            };

            if (feed.message == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            // Making concurrent API requests
            var FeedTask = _feedbackApi.GetAllFeedback_API();

            // get todo new id
            var allFeed = await FeedTask;

            string newId = "";
            if (allFeed.Count > 0)
            {
                var last_feed = allFeed.Last();
                string f_id_up = last_feed.fb_number;
                string prefix = new string(f_id_up.TakeWhile(char.IsLetter).ToArray());
                string numberPart = new string(f_id_up.SkipWhile(char.IsLetter).ToArray());
                int number = int.Parse(numberPart);
                newId = prefix + (number + 1);
            }
            else
                newId = "FBK1";

            // Create New Feedback
            Feedback new_feedback = new Feedback()
            {
                fb_number = newId,
                user_id = currentUser.id,
                message = feed.message
            };

            // API requests
            bool result = await _feedbackApi.CreateFeedback_API(new_feedback);

            if (result)
                return RedirectToAction("Feedback_List", "Feedback");
            else
            {
                ViewBag.Error = "Create Feedback Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Feedback_Info(int id, string role)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var feedback = await _feedbackApi.FindByIDFeedback_API(id);
            var feedbackUser = await _userApi.FindByIDUser_API(feedback.user_id);
            feedback.User = feedbackUser;

            var model = new AllModelVM()
            {
                user = currentUser,
                feedback = feedback,
                roleBack = role,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Feedback_Info(Feedback feed, string roleBack)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var feedback = await _feedbackApi.FindByIDFeedback_API(feed.id);
            var feedbackUser = await _userApi.FindByIDUser_API(feedback.user_id);
            feedback.User = feedbackUser;

            var model = new AllModelVM()
            {
                user = currentUser,
                feedback = feedback,
                roleBack = roleBack,
                noteMessageCount = noteMessageCount
            };

            if (feed.message == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            feedback.message = feed.message;

            bool result = await _feedbackApi.UpdateFeedback_API(feedback);

            if (result)
            {
                if (roleBack == "Admin")
                    return RedirectToAction("All_Feedback_List", "Feedback");
                else
                    return RedirectToAction("Feedback_List", "Feedback");
            }
            else
            {
                ViewBag.Error = "Update Feedback Error";
                return View(model);
            }
        }
    }
}
