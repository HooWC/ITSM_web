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
        private readonly User_api _userApi;
        private readonly Todo_api _todoApi;
        private readonly Feedback_api _feedbackApi;
        private readonly Incident_api _incApi;
        private readonly Knowledge_api _knowledgeApi;
        private readonly Request_api _reqApi;
        private readonly Department_api _depApi;
        private readonly Role_api _roleApi;

        public FeedbackController(IHttpContextAccessor httpContextAccessor)
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
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var feedTask = _feedbackApi.GetAllFeedback_API();
            var userTask = _userApi.GetAllUser_API();
            var roleTask = _roleApi.GetAllRole_API();
            await Task.WhenAll(feedTask, userTask, roleTask);

            var allFeed = await feedTask;
            var allUsers = await userTask;
            var allRoles = await roleTask;

            var feedList = currentUser.role_id == allRoles.Where(x => x.role == "User").FirstOrDefault()?.id
                ? allFeed.Where(x => x.user_id == currentUser.id).OrderByDescending(y => y.id).ToList()
                : allFeed.OrderByDescending(y => y.id).ToList();

            foreach (var feedback in feedList)
                feedback.User = allUsers.FirstOrDefault(x => x.id == feedback.user_id);

            return new AllModelVM
            {
                user = currentUser,
                FeedbackList = feedList
            };
        }

        public async Task<IActionResult> Feedback_Create()
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            ViewBag.CurrentUser = currentUser.fullname;
            ViewBag.Photo = currentUser.photo;
            ViewBag.PhotoType = currentUser.photo_type;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Feedback_Create(Feedback feed)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            ViewBag.CurrentUser = currentUser.fullname;
            ViewBag.Photo = currentUser.photo;
            ViewBag.PhotoType = currentUser.photo_type;

            if (feed.message == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View();
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
                return View();
            }
        }

        public async Task<IActionResult> Feedback_Info(int id, string role)
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            ViewBag.roleBack = role;

            // Get Feedback
            var feedback = await _feedbackApi.FindByIDFeedback_API(id);
            var feedbackUser = await _userApi.FindByIDUser_API(feedback.user_id);
            feedback.User = feedbackUser;

            ViewBag.Photo = currentUser.photo;
            ViewBag.PhotoType = currentUser.photo_type;

            return View(feedback);
        }

        [HttpPost]
        public async Task<IActionResult> Feedback_Info(Feedback feed, string roleBack)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            // Get Feedback
            var feedback = await _feedbackApi.FindByIDFeedback_API(feed.id);
            var feedbackUser = await _userApi.FindByIDUser_API(feedback.user_id);
            feedback.User = feedbackUser;

            ViewBag.Photo = currentUser.photo;
            ViewBag.PhotoType = currentUser.photo_type;

            if (feed.message == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(feedback);
            }

            // Update New Feedback
            feedback.message = feed.message;

            // API requests
            bool result = await _feedbackApi.UpdateFeedback_API(feedback);

            if (result)
            {
                if (roleBack == "Admin")
                    return RedirectToAction("All", "Request");
                else if (roleBack == "Group")
                    return RedirectToAction("Assigned_To_Us", "Request");
                else
                    return RedirectToAction("Feedback_List", "Feedback");
            }
            else
            {
                ViewBag.Error = "Update Feedback Error";
                return View(feedback);
            }
        }
    }
}
