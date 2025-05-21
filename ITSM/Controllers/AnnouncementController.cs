using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class AnnouncementController : Controller
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
        private readonly Announcement_api _announApi;

        public AnnouncementController(IHttpContextAccessor httpContextAccessor)
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
            _announApi = new Announcement_api(httpContextAccessor);
        }

        public async Task<IActionResult> Ann_List()
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var AnnounTask = _announApi.GetAllAnnouncement_API();
            var UserTask = _userApi.GetAllUser_API();
            await Task.WhenAll(AnnounTask, UserTask);

            var allAnnoun = await AnnounTask;
            var allUser = await UserTask;

            foreach(var i in allAnnoun)
                i.User = allUser.FirstOrDefault(x => x.id == i.create_by);

            var model = new AllModelVM
            {
                user = currentUser,
                AnnouncementList = allAnnoun.OrderByDescending(X => X.id).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> View_Ann_List()
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var AnnounTask = _announApi.GetAllAnnouncement_API();
            var UserTask = _userApi.GetAllUser_API();
            await Task.WhenAll(AnnounTask, UserTask);

            var allAnnoun = await AnnounTask;
            var allUser = await UserTask;

            foreach (var i in allAnnoun)
                i.User = allUser.FirstOrDefault(x => x.id == i.create_by);

            var model = new AllModelVM
            {
                user = currentUser,
                AnnouncementList = allAnnoun.OrderByDescending(X => X.id).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> View_Ann_Info(int id)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var userTask = _userApi.GetAllUser_API();
            await Task.WhenAll(userTask);

            var allUser = await userTask;

            var ann_info = await _announApi.FindByIDAnnouncement_API(id);

            ann_info.User = allUser.FirstOrDefault(x => x.id == ann_info.create_by);

            var model = new AllModelVM()
            {
                user = currentUser,
                announcement = ann_info
            };

            return View(model);
        }

        public async Task<IActionResult> Ann_Create()
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var model = new AllModelVM()
            {
                user = currentUser
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Ann_Create(Announcement ann)
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var model = new AllModelVM()
            {
                user = currentUser
            };

            if (ann.message != null && ann.ann_title != null)
            {

                var annTask = _announApi.GetAllAnnouncement_API();
                await Task.WhenAll(annTask);

                var allAnn = await annTask;

                string newId = "";
                if (allAnn.Count > 0)
                {
                    var last_pro = allAnn.Last();
                    string p_id_up = last_pro.at_number;
                    string prefix = new string(p_id_up.TakeWhile(char.IsLetter).ToArray());
                    string numberPart = new string(p_id_up.SkipWhile(char.IsLetter).ToArray());
                    int number = int.Parse(numberPart);
                    newId = prefix + (number + 1);
                }
                else
                    newId = "ANN1";

                Announcement new_ann = new Announcement()
                {
                    at_number = newId,
                    create_by = currentUser.id,
                    ann_title = ann.ann_title,
                    message = ann.message
                };

                // create new Announcement data
                bool result = await _announApi.CreateAnnouncement_API(new_ann);

                if (result)
                    return RedirectToAction("Ann_List", "Announcement");
                else
                {
                    ViewBag.Error = "Create Announcement Error";
                    return View(model);
                }
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }
        }

        public async Task<IActionResult> Ann_Info(int id)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var userTask = _userApi.GetAllUser_API();
            await Task.WhenAll(userTask);

            var allUser = await userTask;

            var ann_info = await _announApi.FindByIDAnnouncement_API(id);

            ann_info.User = allUser.FirstOrDefault(x => x.id == ann_info.create_by);

            var model = new AllModelVM()
            {
                user = currentUser,
                announcement = ann_info
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Ann_Info(Announcement ann)
        {
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var userTask = _userApi.GetAllUser_API();
            await Task.WhenAll(userTask);

            var allUser = await userTask;

            var ann_info = await _announApi.FindByIDAnnouncement_API(ann.id);

            ann_info.User = allUser.FirstOrDefault(x => x.id == ann_info.create_by);

            var model = new AllModelVM()
            {
                user = currentUser,
                announcement = ann_info
            };

            if (ann.ann_title != null && ann.message != null)
            {
                ann_info.ann_title = ann.ann_title;
                ann_info.message = ann.message;
                ann_info.create_by = currentUser.id;

                bool result = await _announApi.UpdateAnnouncement_API(ann_info);

                if (result)
                    return RedirectToAction("Ann_List", "Announcement");
                else
                {
                    ViewBag.Error = "Update Announcement Error";
                    return View(model);
                }
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }
        }
    }
}
