using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ITSM.Controllers
{
    public class CMDBController : Controller
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
        private readonly Announcement_api _announApi;
        private readonly CMDB_api _cmdbApi;

        public CMDBController(IHttpContextAccessor httpContextAccessor, UserService userService)
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
            _cmdbApi = new CMDB_api(httpContextAccessor);
            _userService = userService;
        }

        public async Task<IActionResult> CMDB_List()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var CMDBTask = _cmdbApi.GetAllCMDB_API();
            var DepartmentTask = _depApi.GetAllDepartment_API();
            await Task.WhenAll(CMDBTask, DepartmentTask);

            var allCMDB = CMDBTask.Result;
            var allDep = DepartmentTask.Result;

            foreach (var i in allCMDB)
                i.Department = allDep.FirstOrDefault(x => x.id == i.department_id);

            var model = new AllModelVM
            {
                user = currentUser,
                CMDBList = allCMDB.OrderByDescending(X => X.id).ToList(),
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        public async Task<IActionResult> CMDB_Create()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var DepartmentTask = _depApi.GetAllDepartment_API();
            await Task.WhenAll(DepartmentTask);

            var allDep = DepartmentTask.Result;

            var model = new AllModelVM()
            {
                user = currentUser,
                DepartmentList = allDep,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CMDB_Create(CMDB cmdb_info)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var DepartmentTask = _depApi.GetAllDepartment_API();
            await Task.WhenAll(DepartmentTask);

            var allDep = DepartmentTask.Result;

            var model = new AllModelVM()
            {
                user = currentUser,
                DepartmentList = allDep,
                incCount = incCount,
                reqCount = reqCount
            };

            if (
                cmdb_info.full_name != null &&
                cmdb_info.windows_version != null &&
                cmdb_info.microsoft_office != null &&
                cmdb_info.hostname != null &&
                cmdb_info.antivirus != null &&
                cmdb_info.ip_address != null &&
                cmdb_info.motherboard != null &&
                cmdb_info.monitor_led != null &&
                cmdb_info.hard_disk != null &&
                cmdb_info.dvdrw != null &&
                cmdb_info.processor != null &&
                cmdb_info.erp_system != null
                )
            {

                CMDB new_cmdb = new CMDB()
                {
                    full_name = cmdb_info.full_name,
                    department_id = cmdb_info.department_id,
                    device_type = cmdb_info.device_type,
                    windows_version = cmdb_info.windows_version,
                    hostname = cmdb_info.hostname,
                    microsoft_office = cmdb_info.microsoft_office,
                    antivirus = cmdb_info.antivirus,
                    ip_address = cmdb_info.ip_address,
                    erp_system = cmdb_info.erp_system,
                    sql_account = cmdb_info.sql_account,
                    processor = cmdb_info.processor,
                    motherboard = cmdb_info.motherboard,
                    ram = cmdb_info.ram,
                    monitor_led = cmdb_info.monitor_led,
                    keyboard = cmdb_info.keyboard,
                    mouse = cmdb_info.mouse,
                    hard_disk = cmdb_info.hard_disk,
                    dvdrw = cmdb_info.dvdrw,
                    ms_office = cmdb_info.ms_office
                };

                // create new CMDB data
                bool result = await _cmdbApi.CreateCMDB_API(new_cmdb);

                if (result)
                    return RedirectToAction("CMDB_List", "CMDB");
                else
                {
                    ViewBag.Error = "Create CMDB Error";
                    return View(model);
                }
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }
        }

        public async Task<IActionResult> CMDB_Info(int id)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var departmentTask = _depApi.GetAllDepartment_API();
            await Task.WhenAll(departmentTask);

            var allDeps = departmentTask.Result;

            var cmdb_info = await _cmdbApi.FindByIDCMDB_API(id);

            cmdb_info.Department = allDeps.FirstOrDefault(x => x.id == cmdb_info.department_id);

            var model = new AllModelVM()
            {
                user = currentUser,
                CMDB = cmdb_info,
                DepartmentList = allDeps,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CMDB_Info(CMDB cmdb_if)
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var departmentTask = _depApi.GetAllDepartment_API();
            await Task.WhenAll(departmentTask);

            var allDeps = departmentTask.Result;

            var cmdb_info = await _cmdbApi.FindByIDCMDB_API(cmdb_if.id);

            cmdb_info.Department = allDeps.FirstOrDefault(x => x.id == cmdb_info.department_id);

            var model = new AllModelVM()
            {
                user = currentUser,
                CMDB = cmdb_info,
                DepartmentList = allDeps,
                incCount = incCount,
                reqCount = reqCount
            };

            if (cmdb_if.full_name != null &&
                cmdb_if.windows_version != null &&
                cmdb_if.microsoft_office != null &&
                cmdb_if.hostname != null &&
                cmdb_if.antivirus != null &&
                cmdb_if.ip_address != null &&
                cmdb_if.motherboard != null &&
                cmdb_if.monitor_led != null &&
                cmdb_if.hard_disk != null &&
                cmdb_if.dvdrw != null &&
                cmdb_if.processor != null &&
                cmdb_if.erp_system != null)
            {
                cmdb_info.full_name = cmdb_if.full_name;
                cmdb_info.department_id = cmdb_if.department_id;
                cmdb_info.device_type = cmdb_if.device_type;
                cmdb_info.windows_version = cmdb_if.windows_version;
                cmdb_info.hostname = cmdb_if.hostname;
                cmdb_info.microsoft_office = cmdb_if.microsoft_office;
                cmdb_info.antivirus = cmdb_if.antivirus;
                cmdb_info.ip_address = cmdb_if.ip_address;
                cmdb_info.erp_system = cmdb_if.erp_system;
                cmdb_info.sql_account = cmdb_if.sql_account;
                cmdb_info.processor = cmdb_if.processor;
                cmdb_info.motherboard = cmdb_if.motherboard;
                cmdb_info.ram = cmdb_if.ram;
                cmdb_info.monitor_led = cmdb_if.monitor_led;
                cmdb_info.keyboard = cmdb_if.keyboard;
                cmdb_info.mouse = cmdb_if.mouse;
                cmdb_info.hard_disk = cmdb_if.hard_disk;
                cmdb_info.dvdrw = cmdb_if.dvdrw;
                cmdb_info.ms_office = cmdb_if.ms_office;

                bool result = await _cmdbApi.UpdateCMDB_API(cmdb_info);

                if (result)
                    return RedirectToAction("CMDB_List", "CMDB");
                else
                {
                    ViewBag.Error = "Update CMDB Error";
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
