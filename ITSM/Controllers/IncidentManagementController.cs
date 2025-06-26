using System.Reflection;
using System.Xml.Linq;
using Humanizer;
using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class IncidentManagementController : Controller
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
        private readonly Incident_Photos_api _incphotosApi;
        private readonly Subcategory_api _subcategoryApi;
        private readonly Incident_Category_api _inccategoryApi;

        public IncidentManagementController(IHttpContextAccessor httpContextAccessor, UserService userService)
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
            _incphotosApi = new Incident_Photos_api(httpContextAccessor);
            _subcategoryApi = new Subcategory_api(httpContextAccessor);
            _inccategoryApi = new Incident_Category_api(httpContextAccessor);
            _userService = userService;
        }

        public async Task<AllModelVM> get_Inc_Data(string type)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var inc = _incApi.GetAllIncident_API();
            var dep = _depApi.GetAllDepartment_API();
            var user = _userApi.GetAllUser_API();

            var inc_categoryTask = _inccategoryApi.GetAllIncidentcategory_API();
            var sucategoryTask = _subcategoryApi.GetAllSubcategory_API();

            await Task.WhenAll(inc, dep, user, inc_categoryTask, sucategoryTask);

            var allInc = inc.Result;
            var incList = new List<Incident>();
            if (type == "User_All")
                incList = allInc.Where(x => x.sender == currentUser.id).OrderByDescending(y => y.id).ToList();
            else if (type == "All")
                incList = allInc.OrderByDescending(y => y.id).ToList();
            else if(type == "Assigned_To_Group")
                incList = allInc.Where(x => x.assignment_group == currentUser.department_id).OrderByDescending(y => y.id).ToList();

            var allDepartments = dep.Result;
            var allUsers = user.Result;
            var allIncCategory = inc_categoryTask.Result;
            var allSucategory = sucategoryTask.Result;

            foreach (var incident in incList)
            {
                incident.AssignmentGroup = allDepartments.FirstOrDefault(d => d.id == incident.assignment_group);
                incident.IncidentcategoryData = allIncCategory.FirstOrDefault(x => x.id == incident.category);
                incident.SubcategoryData = allSucategory.FirstOrDefault(x => x.id == incident.subcategory);
            }

            var model = new AllModelVM
            {
                user = currentUser,
                IncidentList = incList,
                incCount = incCount,
                reqCount = reqCount
            };

            return model;
        }

        public async Task<IActionResult> All()
        {
            var model = await get_Inc_Data("All");

            return View(model);
        }

        public async Task<IActionResult> User_All()
        {
            var model = await get_Inc_Data("User_All");

            return View(model);
        }

        public async Task<IActionResult> Create_Form()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var inc_categoryTask = _inccategoryApi.GetAllIncidentcategory_API();
            var sucategoryTask = _subcategoryApi.GetAllSubcategory_API();

            await Task.WhenAll(inc_categoryTask, sucategoryTask);

            var allIncCategory = inc_categoryTask.Result;
            var allSucategory = sucategoryTask.Result;

            var model = new AllModelVM
            {
                user = currentUser,
                Incident_Category_List = allIncCategory,
                Subcategory_List = allSucategory,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create_Form(List<IFormFile> files, Incident inc)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var inc_categoryTask = _inccategoryApi.GetAllIncidentcategory_API();
            var sucategoryTask = _subcategoryApi.GetAllSubcategory_API();

            await Task.WhenAll(inc_categoryTask, sucategoryTask);

            var allIncCategory = inc_categoryTask.Result;
            var allSucategory = sucategoryTask.Result;

            var model = new AllModelVM
            {
                user = currentUser,
                Incident_Category_List = allIncCategory,
                Subcategory_List = allSucategory,
                incCount = incCount,
                reqCount = reqCount
            };

            if (!string.IsNullOrEmpty(inc.describe))
            {
                var allIncident = await _incApi.GetAllIncident_API();

                List<byte[]> fileBytesList = new List<byte[]>();

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 50_000_000) // 50MB
                        {
                            ViewBag.Error = "One of the files exceeds the 50MB limit.";
                            return View(model);
                        }

                        if (file.Length > 0)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await file.CopyToAsync(memoryStream);
                                fileBytesList.Add(memoryStream.ToArray());
                            }
                        }
                    }
                }

                string newId = "";
                if (allIncident.Count > 0)
                {
                    var incidentLast = allIncident.Last();
                    string i_id_up = incidentLast.inc_number;
                    string prefix = new string(i_id_up.TakeWhile(char.IsLetter).ToArray());
                    string numberPart = new string(i_id_up.SkipWhile(char.IsLetter).ToArray());
                    int number = int.Parse(numberPart);
                    newId = prefix + (number + 1);
                }
                else
                    newId = "INC1";

                var new_inc = new Incident()
                {
                    inc_number = newId,
                    describe = inc.describe,
                    sender = currentUser.id,
                    urgency = inc.urgency,
                    state = inc.state,
                    category = inc.category,
                    subcategory = inc.subcategory,
                    updated_by = currentUser.id
                };

                new_inc.assignment_group = allSucategory.FirstOrDefault(x => x.id == inc.subcategory).department_id;

                bool result = await _incApi.CreateIncident_API(new_inc);

                if (result)
                {
                    var createdIncident = await _incApi.FindByIncIDIncident_API(newId);

                    if (createdIncident != null && fileBytesList != null && fileBytesList.Count > 0)
                    {
                        foreach (var fileBytes in fileBytesList)
                        {
                            var incidentPhoto = new IncidentPhotos
                            {
                                incident_id = createdIncident.id, 
                                photo = fileBytes,
                                photo_type = GetMimeTypeFromFileSignature(fileBytes)
                            };
                            await _incphotosApi.CreateIncidentPhotos_API(incidentPhoto);
                        }
                    }

                    return RedirectToAction("User_All", "IncidentManagement");
                }
                else
                {
                    ViewBag.Error = "Create Incident Error";
                    return View(model);
                }
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }
        }

        public async Task<IActionResult> Inc_Info_Form(int id, string type)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var departmentTask = _depApi.GetAllDepartment_API();
            var userTask = _userApi.GetAllUser_API();
            var incphotosTask = _incphotosApi.GetAllIncidentPhotos_API();

            var inc_categoryTask = _inccategoryApi.GetAllIncidentcategory_API();
            var sucategoryTask = _subcategoryApi.GetAllSubcategory_API();

            await Task.WhenAll(departmentTask, userTask, incphotosTask, inc_categoryTask, sucategoryTask);

            var allDepartment = departmentTask.Result;
            var allUser = userTask.Result;
            var allInc_Photos = incphotosTask.Result;
            var allIncCategory = inc_categoryTask.Result;
            var allSucategory = sucategoryTask.Result;

            var incData = await _incApi.FindByIDIncident_API(id);

            incData.AssignmentGroup = allDepartment.Where(x => x.id == incData.assignment_group).FirstOrDefault();
            incData.IncidentcategoryData = allIncCategory.FirstOrDefault(x => x.id == incData.category);
            incData.SubcategoryData = allSucategory.FirstOrDefault(x => x.id == incData.subcategory);
            incData.Sender = allUser.FirstOrDefault(x => x.id == incData.sender);

            var allRelatedPhotos = allInc_Photos.Where(x => x.incident_id == incData.id).ToList();

            if (allRelatedPhotos.Count == 0)
                allRelatedPhotos = null;

            var model = new AllModelVM()
            {
                user = currentUser,
                incident = incData,
                roleBack = type,
                Incident_Photos_List = allRelatedPhotos,
                Incident_Category_List = allIncCategory,
                Subcategory_List = allSucategory,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Inc_Info_Form(List<IFormFile> files, Incident inc, string roleBack)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var departmentTask = _depApi.GetAllDepartment_API();
            var userTask = _userApi.GetAllUser_API();
            var incphotosTask = _incphotosApi.GetAllIncidentPhotos_API();

            var inc_categoryTask = _inccategoryApi.GetAllIncidentcategory_API();
            var sucategoryTask = _subcategoryApi.GetAllSubcategory_API();

            await Task.WhenAll(departmentTask, userTask, incphotosTask, inc_categoryTask, sucategoryTask);

            var allDepartment = departmentTask.Result;
            var allUser = userTask.Result;
            var allInc_Photos = incphotosTask.Result;
            var allIncCategory = inc_categoryTask.Result;
            var allSucategory = sucategoryTask.Result;

            var incData = await _incApi.FindByIDIncident_API(inc.id);

            incData.AssignmentGroup = allDepartment.FirstOrDefault(x => x.id == incData.assignment_group);
            incData.IncidentcategoryData = allIncCategory.FirstOrDefault(x => x.id == incData.category);
            incData.SubcategoryData = allSucategory.FirstOrDefault(x => x.id == incData.subcategory);

            var allRelatedPhotos = allInc_Photos.Where(x => x.incident_id == incData.id).ToList();

            if (allRelatedPhotos.Count == 0)
                allRelatedPhotos = null;

            var model = new AllModelVM()
            {
                user = currentUser,
                incident = incData,
                roleBack = roleBack,
                Incident_Photos_List = allRelatedPhotos,
                Incident_Category_List = allIncCategory,
                Subcategory_List = allSucategory,
                incCount = incCount,
                reqCount = reqCount
            };

            if (inc.describe != null)
            {
                List<byte[]> fileBytesList = new List<byte[]>();

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 50_000_000) // 50MB
                        {
                            ViewBag.Error = "One of the files exceeds the 50MB limit.";
                            return View(model);
                        }

                        if (file.Length > 0)
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await file.CopyToAsync(memoryStream);
                                fileBytesList.Add(memoryStream.ToArray());
                            }
                        }
                    }
                }

                if (incData != null)
                {
                    incData.describe = inc.describe;
                    incData.state = inc.state;
                    incData.updated_by = currentUser.id;

                    if (fileBytesList != null && fileBytesList.Count > 0)
                    {
                        foreach (var fileBytes in fileBytesList)
                        {
                            var incidentPhoto = new IncidentPhotos
                            {
                                incident_id = incData.id,  
                                photo = fileBytes,
                                photo_type = GetMimeTypeFromFileSignature(fileBytes)
                            };

                            await _incphotosApi.CreateIncidentPhotos_API(incidentPhoto);
                        }
                    }

                    bool result = await _incApi.UpdateIncident_API(incData);

                    if (result)
                    {
                        if (roleBack == "Admin")
                            return RedirectToAction("All", "IncidentManagement");
                        else if (roleBack == "ToGroup")
                            return RedirectToAction("Assigned_To_Group", "IncidentManagement");
                        else
                            return RedirectToAction("User_All", "IncidentManagement");
                    }
                    else
                    {
                        ViewBag.Error = "Update event failed";
                        return View(model);
                    }
                }
                else
                {
                    ViewBag.Error = "Update Incident Error";
                    return View(model);
                }
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }
        }

        public async Task<IActionResult> Assigned_To_Me()
        {
            var model = await get_Inc_Data("Assigned_To_Me");

            return View(model);
        }

        public async Task<IActionResult> Assigned_To_Group()
        {
            var model = await get_Inc_Data("Assigned_To_Group");

            return View(model);
        }

        public async Task<IActionResult> Resolved_Assigned_To_Me()
        {
            var model = await get_Inc_Data("Resolved_Assigned_To_Me");

            return View(model);
        }

        public async Task<IActionResult> Closed_Assigned_To_Me()
        {
            var model = await get_Inc_Data("Closed_Assigned_To_Me");

            return View(model);
        }

        public async Task<IActionResult> Manager_Assign_Work()
        {
            var model = await get_Inc_Data("Manager_Assign_Work");

            return View(model);
        }

        public async Task<AllModelVM> get_Manager_Assign_Work_Info(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var departmentTask = _depApi.GetAllDepartment_API();
            var userTask = _userApi.GetAllUser_API();
            var incphotosTask = _incphotosApi.GetAllIncidentPhotos_API();

            var inc_categoryTask = _inccategoryApi.GetAllIncidentcategory_API();
            var sucategoryTask = _subcategoryApi.GetAllSubcategory_API();

            await Task.WhenAll(departmentTask, userTask, incphotosTask, inc_categoryTask, sucategoryTask);

            var allDepartment = departmentTask.Result;
            var allUser = userTask.Result;
            var allInc_Photos = incphotosTask.Result;
            var allIncCategory = inc_categoryTask.Result;
            var allSucategory = sucategoryTask.Result;

            var incData = await _incApi.FindByIDIncident_API(id);

            incData.AssignmentGroup = allDepartment.Where(x => x.id == incData.assignment_group).FirstOrDefault();
            incData.IncidentcategoryData = allIncCategory.FirstOrDefault(x => x.id == incData.category);
            incData.SubcategoryData = allSucategory.FirstOrDefault(x => x.id == incData.subcategory);
            incData.Sender = allUser.FirstOrDefault(x => x.id == incData.sender);

            var allRelatedPhotos = allInc_Photos.Where(x => x.incident_id == incData.id).ToList();

            if (allRelatedPhotos.Count == 0)
                allRelatedPhotos = null;

            var model = new AllModelVM()
            {
                user = currentUser,
                incident = incData,
                Incident_Photos_List = allRelatedPhotos,
                Incident_Category_List = allIncCategory,
                Subcategory_List = allSucategory,
                incCount = incCount,
                reqCount = reqCount
            };

            return model;
        }

        public async Task<IActionResult> Manager_Assign_Work_Info(int id)
        {
            var model = await get_Manager_Assign_Work_Info(id);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Manager_Assign_Work_Info(Incident inc)
        {
            var model = await get_Manager_Assign_Work_Info(inc.id);

            var incData = await _incApi.FindByIDIncident_API(inc.id);

            if (inc.describe != null)
            {
                if (incData != null)
                {
                    incData.assigned_to = inc.assigned_to;

                    bool result = await _incApi.UpdateIncident_API(incData);

                    if (result)
                        return RedirectToAction("Manager_Assign_Work", "IncidentManagement");
                    else
                    {
                        ViewBag.Error = "Update event failed";
                        return View(model);
                    }
                }
                else
                {
                    ViewBag.Error = "Update Incident Error";
                    return View(model);
                }
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }
        }

        private string GetMimeTypeFromFileSignature(byte[] fileBytes)
        {
            if (fileBytes.Length < 4) return "application/octet-stream";

            // PNG
            if (fileBytes[0] == 0x89 && fileBytes[1] == 0x50 &&
                fileBytes[2] == 0x4E && fileBytes[3] == 0x47)
                return "image/png";

            // JPEG/JPG
            if (fileBytes[0] == 0xFF && fileBytes[1] == 0xD8 && fileBytes[2] == 0xFF)
                return "image/jpeg";

            // GIF
            if (fileBytes[0] == 0x47 && fileBytes[1] == 0x49 && fileBytes[2] == 0x46)
                return "image/gif";

            // WebP
            if (fileBytes.Length >= 12 &&
                fileBytes[0] == 0x52 && fileBytes[1] == 0x49 &&
                fileBytes[2] == 0x46 && fileBytes[3] == 0x46 &&
                fileBytes[8] == 0x57 && fileBytes[9] == 0x45 &&
                fileBytes[10] == 0x42 && fileBytes[11] == 0x50)
                return "image/webp";

            // BMP
            if (fileBytes[0] == 0x42 && fileBytes[1] == 0x4D)
                return "image/bmp";

            // TIFF (little endian)
            if (fileBytes[0] == 0x49 && fileBytes[1] == 0x49 &&
                fileBytes[2] == 0x2A && fileBytes[3] == 0x00)
                return "image/tiff";

            // TIFF (big endian)
            if (fileBytes[0] == 0x4D && fileBytes[1] == 0x4D &&
                fileBytes[2] == 0x00 && fileBytes[3] == 0x2A)
                return "image/tiff";

            // ICO
            if (fileBytes[0] == 0x00 && fileBytes[1] == 0x00 &&
                fileBytes[2] == 0x01 && fileBytes[3] == 0x00)
                return "image/x-icon";

            // HEIF
            if (fileBytes.Length >= 12 &&
                ((fileBytes[4] == 0x66 && fileBytes[5] == 0x74 &&
                  fileBytes[6] == 0x79 && fileBytes[7] == 0x70 &&
                  fileBytes[8] == 0x68 && fileBytes[9] == 0x65 &&
                  fileBytes[10] == 0x69 && fileBytes[11] == 0x63) || // heic
                 (fileBytes[4] == 0x66 && fileBytes[5] == 0x74 &&
                  fileBytes[6] == 0x79 && fileBytes[7] == 0x70 &&
                  fileBytes[8] == 0x6D && fileBytes[9] == 0x69 &&
                  fileBytes[10] == 0x66 && fileBytes[11] == 0x31)))   // heif
                return "image/heif";

            // AVIF
            if (fileBytes.Length >= 12 &&
                fileBytes[4] == 0x66 && fileBytes[5] == 0x74 &&
                fileBytes[6] == 0x79 && fileBytes[7] == 0x70 &&
                fileBytes[8] == 0x61 && fileBytes[9] == 0x76 &&
                fileBytes[10] == 0x69 && fileBytes[11] == 0x66)
                return "image/avif";

            // Basic
            return "application/octet-stream";
        }

    }
}
