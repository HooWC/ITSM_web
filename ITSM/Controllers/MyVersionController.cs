using System.Runtime.Intrinsics.Arm;
using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class MyVersionController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserService _userService;
        private readonly Myversion_api _myversionApi;

        public MyVersionController(IHttpContextAccessor httpContextAccessor, UserService userService)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _myversionApi = new Myversion_api(httpContextAccessor);
        }

        public async Task<IActionResult> All()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

                var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var versionTask = _myversionApi.GetAllMyversion_API();
            await Task.WhenAll(versionTask);

            var allVersion = versionTask.Result;

            var versionList = allVersion.OrderByDescending(y => y.id).ToList();

            var model = new AllModelVM
            {
                user = currentUser,
                MyversionList = versionList,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Myversion version)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
            };

            if (version.version_num == null && version.message == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var versionTask = _myversionApi.GetAllMyversion_API();
            await Task.WhenAll(versionTask);

            var allVersion = versionTask.Result;

            bool b = allVersion.Where(x => x.version_num ==  version.version_num).Any();
            if (b)
            {
                ViewBag.Error = "This version number already exist";
                return View(model);
            }

            Myversion new_version = new Myversion()
            {
                version_num = version.version_num,
                message = version.message
            };

            bool result = await _myversionApi.CreateMyversion_API(new_version);

            if (result)
                return RedirectToAction("All", "MyVersion");
            else
            {
                ViewBag.Error = "Create Version Error";
                return View(model);
            }
        }

        public async Task<IActionResult> Info(int id)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var myversion = await _myversionApi.FindByIDMyversion_API(id);

            var model = new AllModelVM
            {
                user = currentUser,
                myversion = myversion,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Info(Myversion version)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var myversion = await _myversionApi.FindByIDMyversion_API(version.id);

            var model = new AllModelVM
            {
                user = currentUser,
                myversion = myversion,
                incCount = incCount,
                reqCount = reqCount
            };

            if (version.version_num == null && version.message == null)
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }

            var versionTask = _myversionApi.GetAllMyversion_API();
            await Task.WhenAll(versionTask);

            var allVersion = versionTask.Result;

            bool b = allVersion.Where(x => x.version_num == version.version_num && x.id != version.id).Any();
            if (b)
            {
                ViewBag.Error = "This version number already exist";
                return View(model);
            }

            myversion.version_num = version.version_num;
            myversion.message = version.message;

            bool result = await _myversionApi.UpdateMyversion_API(myversion);

            if (result)
                return RedirectToAction("All", "MyVersion");
            else
            {
                ViewBag.Error = "Update Version Error";
                return View(model);
            }
        }

        public async Task<IActionResult> View_Page()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var allVersion = await _myversionApi.GetAllMyversion_API();

            var versionList = allVersion.OrderByDescending(y => y.id).ToList();

            var model = new AllModelVM
            {
                user = currentUser,
                MyversionList = versionList,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }
    }
}
