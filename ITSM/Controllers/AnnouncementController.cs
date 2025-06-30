using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ITSM.Controllers
{
    public class AnnouncementController : Controller
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

        public AnnouncementController(IHttpContextAccessor httpContextAccessor, UserService userService)
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
            _userService = userService;
        }

        public async Task<IActionResult> Ann_List()
        {
            var checkResult = await _userService.checkIsAdmin();
            if (checkResult is RedirectToActionResult)
                return checkResult;

            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var AnnounTask = _announApi.GetAllAnnouncement_API();
            var UserTask = _userApi.GetAllUser_API();
            await Task.WhenAll(AnnounTask, UserTask);

            var allAnnoun = AnnounTask.Result;
            var allUser = UserTask.Result;

            foreach(var i in allAnnoun)
                i.User = allUser.FirstOrDefault(x => x.id == i.create_by);

            var model = new AllModelVM
            {
                user = currentUser,
                AnnouncementList = allAnnoun.OrderByDescending(X => X.id).ToList(),
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        public async Task<IActionResult> View_Ann_List()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var AnnounTask = _announApi.GetAllAnnouncement_API();
            var UserTask = _userApi.GetAllUser_API();
            await Task.WhenAll(AnnounTask, UserTask);

            var allAnnoun = AnnounTask.Result;
            var allUser = UserTask.Result;

            foreach (var i in allAnnoun)
                i.User = allUser.FirstOrDefault(x => x.id == i.create_by);

            var model = new AllModelVM
            {
                user = currentUser,
                AnnouncementList = allAnnoun.OrderByDescending(X => X.id).ToList(),
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        public async Task<IActionResult> View_Ann_Info(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var userTask = _userApi.GetAllUser_API();
            await Task.WhenAll(userTask);

            var allUser = userTask.Result;

            var ann_info = await _announApi.FindByIDAnnouncement_API(id);

            ann_info.User = allUser.FirstOrDefault(x => x.id == ann_info.create_by);

            var model = new AllModelVM()
            {
                user = currentUser,
                announcement = ann_info,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        public async Task<IActionResult> Ann_Create()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM()
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Ann_Create(Announcement ann)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM()
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
            };

            if (ann.message != null && ann.ann_title != null)
            {

                var annTask = _announApi.GetAllAnnouncement_API();
                await Task.WhenAll(annTask);

                var allAnn = annTask.Result;

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

        public async Task<IActionResult> Ann_Info(int id, string type)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var userTask = _userApi.GetAllUser_API();
            await Task.WhenAll(userTask);

            var allUser = userTask.Result;

            var ann_info = await _announApi.FindByIDAnnouncement_API(id);

            ann_info.User = allUser.FirstOrDefault(x => x.id == ann_info.create_by);

            var model = new AllModelVM()
            {
                user = currentUser,
                announcement = ann_info,
                incCount = incCount,
                reqCount = reqCount
            };

            if (type == "info") return View(model);
            else return View("Ann_Import_Info", model);
        }

        [HttpPost]
        public async Task<IActionResult> Ann_Info(IFormFile file, string type, Announcement ann)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var userTask = _userApi.GetAllUser_API();
            await Task.WhenAll(userTask);

            var allUser = userTask.Result;

            var ann_info = await _announApi.FindByIDAnnouncement_API(ann.id);

            ann_info.User = allUser.FirstOrDefault(x => x.id == ann_info.create_by);

            var model = new AllModelVM()
            {
                user = currentUser,
                announcement = ann_info,
                incCount = incCount,
                reqCount = reqCount
            };

            if (ann.ann_title != null)
            {
                if(type == "info")
                {
                    if(ann.message == null)
                    {
                        ViewBag.Error = "Please fill in all required fields";
                        return View(model);
                    }
                    ann_info.message = ann.message;
                }

                byte[] fileBytes = null;
                if (type == "import")
                {
                    if (file != null && file.Length > 100_000_000) // 100MB
                    {
                        ViewBag.Error = "File size exceeds 100MB limit";
                        return View(model);
                    }

                    if (file != null && file.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await file.CopyToAsync(memoryStream);
                            fileBytes = memoryStream.ToArray();
                        }
                    }
                }

                if (type == "import")
                {
                    if (fileBytes != null)
                    {
                        ann_info.ann_file = fileBytes;
                        ann_info.ann_type = GetMimeTypeFromFileSignature(fileBytes);
                    }
                }

                ann_info.ann_title = ann.ann_title;
                ann_info.create_by = currentUser.id;

                bool result = await _announApi.UpdateAnnouncement_API(ann_info);

                if (result)
                    return RedirectToAction("Ann_List", "Announcement");
                else
                {
                    ViewBag.Error = "Update Announcement Error";
                    if (type == "info") return View(model);
                    else return View("Ann_Import_Info", model);
                }
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                if (type == "info") return View(model);
                else return View("Ann_Import_Info", model);
            }
        }

        public async Task<IActionResult> Ann_Import()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM()
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Ann_Import(IFormFile file, Announcement ann)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM()
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
            };

            if (ann.ann_title != null && file != null)
            {
                byte[] fileBytes = null;

                if (file != null && file.Length > 100_000_000) // 100MB
                {
                    ViewBag.Error = "File size exceeds 100MB limit";
                    return View(model);
                }

                if (file != null && file.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                }

                var annTask = _announApi.GetAllAnnouncement_API();
                await Task.WhenAll(annTask);

                var allAnn = annTask.Result;

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
                };

                if (fileBytes != null)
                {
                    new_ann.ann_file = fileBytes;
                    string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    new_ann.ann_type = GetMimeTypeFromFileSignature(fileBytes, fileExtension);
                }

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

        public async Task<IActionResult> Ann_Import_Info()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var incCount = await _userService.GetIncidentTeamCount();
            var reqCount = await _userService.GetRequestToMeCount();

            var model = new AllModelVM()
            {
                user = currentUser,
                incCount = incCount,
                reqCount = reqCount
            };
            return View(model);
        }

        private string GetMimeTypeFromFileSignature(byte[] fileBytes, string fileExtension = "")
        {
            if (fileBytes.Length < 4) return "application/octet-stream";

            // PDF
            if (fileBytes[0] == 0x25 && fileBytes[1] == 0x50 &&
                fileBytes[2] == 0x44 && fileBytes[3] == 0x46)
                return "application/pdf";

            if (fileBytes.Length >= 8 &&
                fileBytes[0] == 0xD0 && fileBytes[1] == 0xCF &&
                fileBytes[2] == 0x11 && fileBytes[3] == 0xE0 &&
                fileBytes[4] == 0xA1 && fileBytes[5] == 0xB1 &&
                fileBytes[6] == 0x1A && fileBytes[7] == 0xE1)
            {
                switch (fileExtension)
                {
                    case ".doc":
                        return "application/msword";
                    case ".xls":
                        return "application/vnd.ms-excel";
                    case ".ppt":
                        return "application/vnd.ms-powerpoint";
                    case ".vsd":
                        return "application/vnd.visio";
                    case ".mdb":
                        return "application/vnd.ms-access";
                    default:
                        return "application/vnd.ms-office";
                }
            }

            if (fileBytes.Length >= 4 &&
                fileBytes[0] == 0x50 && fileBytes[1] == 0x4B &&
                fileBytes[2] == 0x03 && fileBytes[3] == 0x04)
            {
                switch (fileExtension)
                {
                    case ".docx":
                        return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    case ".xlsx":
                        return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    case ".pptx":
                        return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                    case ".ppsx":
                        return "application/vnd.openxmlformats-officedocument.presentationml.slideshow";
                    case ".vsdx":
                        return "application/vnd.ms-visio.drawing";
                    default:
                        return "application/vnd.openxmlformats-officedocument";
                }
            }

            // RTF
            if (fileBytes.Length >= 5 &&
                fileBytes[0] == 0x7B && fileBytes[1] == 0x5C &&
                fileBytes[2] == 0x72 && fileBytes[3] == 0x74 &&
                fileBytes[4] == 0x66)
                return "application/rtf";

            // txt
            if (fileExtension == ".txt")
                return "text/plain";

            // CSV
            if (fileExtension == ".csv")
                return "text/csv";

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

            switch (fileExtension)
            {
                case ".xml":
                    return "application/xml";
                case ".zip":
                    return "application/zip";
                case ".rar":
                    return "application/x-rar-compressed";
                case ".7z":
                    return "application/x-7z-compressed";
                case ".tar":
                    return "application/x-tar";
                case ".gz":
                case ".gzip":
                    return "application/gzip";
                case ".html":
                case ".htm":
                    return "text/html";
            }

            return "application/octet-stream";
        }

        public async Task<IActionResult> DownloadKBFile(int id)
        {
            try
            {
                var ann = await _announApi.FindByIDAnnouncement_API(id);

                if (ann == null || ann.ann_file == null || ann.ann_file.Length == 0)
                {
                    return NotFound("File does not exist");
                }

                string fileExtension = GetFileExtensionFromMimeType(ann.ann_type);
                string fileName = $"ANN_{ann.at_number}{fileExtension}";

                return File(ann.ann_file, ann.ann_type, fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred while downloading the file: {ex.Message}");
            }
        }

        private string GetFileExtensionFromMimeType(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType))
                return ".bin";

            return mimeType.ToLower() switch
            {
                "application/pdf" => ".pdf",
                "application/msword" => ".doc",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => ".docx",
                "application/vnd.ms-excel" => ".xls",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => ".xlsx",
                "application/vnd.ms-powerpoint" => ".ppt",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation" => ".pptx",
                "application/vnd.openxmlformats-officedocument.presentationml.slideshow" => ".ppsx",
                "text/plain" => ".txt",
                "text/csv" => ".csv",
                "application/zip" => ".zip",
                "application/x-rar-compressed" => ".rar",
                "application/rtf" => ".rtf",
                _ => ".bin"
            };
        }
    }
}
