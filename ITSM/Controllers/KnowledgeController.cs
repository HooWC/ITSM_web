using System.Security.Cryptography.Xml;
using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class KnowledgeController : Controller
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
        private readonly Knowledge_api _kbApi;
        private readonly Category_api _categoryApi;

        public KnowledgeController(IHttpContextAccessor httpContextAccessor, UserService userService)
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
            _kbApi = new Knowledge_api(httpContextAccessor);
            _categoryApi = new Category_api(httpContextAccessor);
            _userService = userService;
        }

        public async Task<IActionResult> KB_Home()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var CategoryTask = _categoryApi.GetAllCategory_API();
            var UserTask = _userApi.GetAllUser_API();
            var KBTask = _kbApi.GetAllKnowledge_API();

            await Task.WhenAll(CategoryTask, UserTask, KBTask);

            var allCategory = CategoryTask.Result;
            var allUser = UserTask.Result;
            var allKB = KBTask.Result;

            foreach (var i in allKB)
            {
                i.Category = allCategory.FirstOrDefault(x => x.id == i.category_id);
                i.Author = allUser.FirstOrDefault(x => x.id == i.author);
            }

            var model = new AllModelVM
            {
                user = currentUser,
                KnowledgeList = allKB.OrderByDescending(X => X.id).ToList(),
                CategoryList = allCategory
                
            };

            return View(model);
        }

        public async Task<IActionResult> KB_List()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var CategoryTask = _categoryApi.GetAllCategory_API();
            var UserTask = _userApi.GetAllUser_API();
            var KBTask = _kbApi.GetAllKnowledge_API();

            await Task.WhenAll(CategoryTask, UserTask, KBTask);

            var allCategory = CategoryTask.Result;
            var allUser = UserTask.Result;
            var allKB = KBTask.Result;

            foreach (var i in allKB)
            {
                i.Category = allCategory.FirstOrDefault(x => x.id == i.category_id);
                i.Author = allUser.FirstOrDefault(x => x.id == i.author);
            }

            var model = new AllModelVM
            {
                user = currentUser,
                KnowledgeList = allKB.OrderByDescending(X => X.id).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> KB_List_User()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var CategoryTask = _categoryApi.GetAllCategory_API();
            var UserTask = _userApi.GetAllUser_API();
            var KBTask = _kbApi.GetAllKnowledge_API();

            await Task.WhenAll(CategoryTask, UserTask, KBTask);

            var allCategory = CategoryTask.Result;
            var allUser = UserTask.Result;
            var allKB = KBTask.Result;

            var KB_info_list = allKB.Where(x => x.author == currentUser.id).ToList();

            foreach (var i in KB_info_list)
            {
                i.Category = allCategory.FirstOrDefault(x => x.id == i.category_id);
                i.Author = allUser.FirstOrDefault(x => x.id == i.author);
            }

            var model = new AllModelVM
            {
                user = currentUser,
                KnowledgeList = KB_info_list.OrderByDescending(X => X.id).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> KB_Info(int id, string type, string role)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var userTask = _userApi.GetAllUser_API();
            var categoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(userTask, categoryTask);

            var allUsers = userTask.Result;
            var allCategorys = categoryTask.Result;

            var kb_info = await _kbApi.FindByIDKnowledge_API(id);

            kb_info.Author = allUsers.FirstOrDefault(x => x.id == kb_info.author);

            var model = new AllModelVM()
            {
                user = currentUser,
                CategoryList = allCategorys,
                knowledge = kb_info,
                roleBack = role
            };

            if(type == "word") return View(model);
            else return View("KB_Import_Info", model);
        }

        [HttpPost]
        public async Task<IActionResult> KB_Info(IFormFile file, string type, Knowledge kb, string roleBack)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var userTask = _userApi.GetAllUser_API();
            var categoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(userTask, categoryTask);

            var allUsers = userTask.Result;
            var allCategorys = categoryTask.Result;

            var kb_info = await _kbApi.FindByIDKnowledge_API(kb.id);

            kb_info.Author = allUsers.FirstOrDefault(x => x.id == kb_info.author);

            var model = new AllModelVM()
            {
                user = currentUser,
                CategoryList = allCategorys,
                knowledge = kb_info,
                roleBack = roleBack
            };

            if (kb.title != null &&
                kb.short_description != null)
            {
                var u_kb_info = await _kbApi.FindByIDKnowledge_API(kb.id);

                if (type == "word")
                {
                    if(kb.article == null)
                    {
                        ViewBag.Error = "Please fill in all required fields";
                        return View(model);
                    }
                    else
                    {
                        u_kb_info.article = kb.article;
                    }
                }

                byte[] fileBytes = null;
                if (type == "file")
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

                if(type == "file")
                {
                    if (fileBytes != null)
                    {
                        u_kb_info.kb_file = fileBytes;
                        u_kb_info.kb_type = GetMimeTypeFromFileSignature(fileBytes);
                    }
                }

                u_kb_info.title = kb.title;
                u_kb_info.short_description = kb.short_description;
                u_kb_info.active = kb.active;
                u_kb_info.category_id = kb.category_id;

                bool result = await _kbApi.UpdateKnowledge_API(u_kb_info);

                if (result)
                {
                    if (roleBack == "admin")
                        return RedirectToAction("KB_List", "Knowledge");
                    else
                        return RedirectToAction("KB_List_User", "Knowledge");
                }
                else
                {
                    ViewBag.Error = "Update Knowledge Error";
                    if (type == "word") return View(model);
                    else return View("KB_Import_Info", model);
                }
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                if (type == "word") return View(model);
                else return View("KB_Import_Info", model);
            }
        }

        public async Task<IActionResult> KB_Import_Info()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var model = new AllModelVM()
            {
                user = currentUser
            };
            return View(model);
        }

        public async Task<IActionResult> KB_Search(int categorytitle, string kb_search_word, string showall)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var CategoryTask = _categoryApi.GetAllCategory_API();
            var UserTask = _userApi.GetAllUser_API();
            var KBTask = _kbApi.GetAllKnowledge_API();

            await Task.WhenAll(CategoryTask, UserTask, KBTask);

            var allCategory = CategoryTask.Result;
            var allUser = UserTask.Result;
            var allKB = KBTask.Result;

            var category_info = new Category();
            if (categorytitle != null)
                category_info  = allCategory.FirstOrDefault(x => x.id == categorytitle);

            List<Knowledge> kbs;
            if (kb_search_word != null)
                kbs = allKB.Where(x => x.title.ToLower().Contains(kb_search_word.ToLower())).ToList();
            else if (showall != null)
                kbs = allKB;
            else if (category_info != null)
                kbs = allKB.Where(x => x.category_id == category_info.id).ToList();
            else
                kbs = allKB;

            if (kbs.Count > 0)
            {
                foreach (var i in kbs)
                {
                    i.Category = allCategory.FirstOrDefault(x => x.id == i.category_id);
                    i.Author = allUser.FirstOrDefault(x => x.id == i.author);
                }
            }

            var model = new AllModelVM
            {
                user = currentUser,
                KnowledgeList = kbs,
                category = category_info != null ? category_info : null,
                kb_search_word = kb_search_word
            };

            return View(model);
        }

        public async Task<IActionResult> KB_Read(int kbid, string kbsearchword, string showall)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var CategoryTask = _categoryApi.GetAllCategory_API();
            var UserTask = _userApi.GetAllUser_API();
            var KBTask = _kbApi.GetAllKnowledge_API();

            await Task.WhenAll(CategoryTask, UserTask, KBTask);

            var allCategory = CategoryTask.Result;
            var allUser = UserTask.Result;
            var allKB = KBTask.Result;

            var kb_info = new Knowledge();
            if (kbid != null)
                kb_info = allKB.Where(x => x.id == kbid).FirstOrDefault();

            var category_info = allCategory.FirstOrDefault(x => x.id == kb_info.category_id);

            kb_info.Category = allCategory.FirstOrDefault(x => x.id == kb_info.category_id);
            kb_info.Author = allUser.FirstOrDefault(x => x.id == kb_info.author);

            var model = new AllModelVM
            {
                user = currentUser,
                knowledge = kb_info,
                category = category_info,
                kb_search_word = kbsearchword != null ? kbsearchword : null,
                kb_search_all = showall != null ? showall : null,
                KnowledgeList = allKB.Where(x => x.category_id == kb_info.category_id && x.id != kb_info.id).Take(5).OrderByDescending(x => x.id).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> KB_Create()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var CategoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(CategoryTask);

            var allCategory = CategoryTask.Result;

            var model = new AllModelVM()
            {
                user = currentUser,
                CategoryList = allCategory
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> KB_Create(Knowledge kb)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var CategoryTask = _categoryApi.GetAllCategory_API();
            var KBTask = _kbApi.GetAllKnowledge_API();
            await Task.WhenAll(CategoryTask, KBTask);

            var allCategory = CategoryTask.Result;
            var allKB = KBTask.Result;

            var model = new AllModelVM()
            {
                user = currentUser,
                CategoryList = allCategory
            };

            if (
                kb.title != null &&
                kb.short_description != null &&
                kb.article != null
                )
            {

                string newId = "";
                if (allKB.Count > 0)
                {
                    var last_pro = allKB.Last();
                    string p_id_up = last_pro.kb_number;
                    string prefix = new string(p_id_up.TakeWhile(char.IsLetter).ToArray());
                    string numberPart = new string(p_id_up.SkipWhile(char.IsLetter).ToArray());
                    int number = int.Parse(numberPart);
                    newId = prefix + (number + 1);
                }
                else
                    newId = "KB1";

                Knowledge new_kb = new Knowledge()
                {
                    kb_number = newId,
                    author = currentUser.id,
                    article = kb.article,
                    short_description = kb.short_description,
                    category_id = kb.category_id,
                    title = kb.title
                };

                // create new KB data
                bool result = await _kbApi.CreateKnowledge_API(new_kb);

                if (result)
                    return RedirectToAction("KB_List_User", "Knowledge");
                else
                {
                    ViewBag.Error = "Create Knowledge Error";
                    return View(model);
                }
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }
        }

        public async Task<IActionResult> KB_Import()
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var CategoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(CategoryTask);

            var allCategory = CategoryTask.Result;

            var model = new AllModelVM()
            {
                user = currentUser,
                CategoryList = allCategory
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> KB_Import(IFormFile file, Knowledge kb)
        {
            var currentUser = await _userService.GetCurrentUserAsync();

            var CategoryTask = _categoryApi.GetAllCategory_API();
            var KBTask = _kbApi.GetAllKnowledge_API();
            await Task.WhenAll(CategoryTask, KBTask);

            var allCategory = CategoryTask.Result;
            var allKB = KBTask.Result;

            var model = new AllModelVM()
            {
                user = currentUser,
                CategoryList = allCategory
            };

            if (kb.title != null &&
                kb.short_description != null &&
                file != null)
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

                string newId = "";
                if (allKB.Count > 0)
                {
                    var last_pro = allKB.Last();
                    string p_id_up = last_pro.kb_number;
                    string prefix = new string(p_id_up.TakeWhile(char.IsLetter).ToArray());
                    string numberPart = new string(p_id_up.SkipWhile(char.IsLetter).ToArray());
                    int number = int.Parse(numberPart);
                    newId = prefix + (number + 1);
                }
                else
                    newId = "KB1";

                Knowledge new_kb = new Knowledge()
                {
                    kb_number = newId,
                    author = currentUser.id,
                    short_description = kb.short_description,
                    category_id = kb.category_id,
                    title = kb.title
                };

                if (fileBytes != null)
                {
                    new_kb.kb_file = fileBytes;
                    string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    new_kb.kb_type = GetMimeTypeFromFileSignature(fileBytes, fileExtension);
                }

                // create new knowledge data
                bool result = await _kbApi.CreateKnowledge_API(new_kb);

                if (result)
                    return RedirectToAction("KB_List_User", "Knowledge");
                else
                {
                    ViewBag.Error = "Create Knowledge Error";
                    return View(model);
                }
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }
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
                var kb = await _kbApi.FindByIDKnowledge_API(id);
                
                if (kb == null || kb.kb_file == null || kb.kb_file.Length == 0)
                {
                    return NotFound("File does not exist");
                }

                string fileExtension = GetFileExtensionFromMimeType(kb.kb_type);
                string fileName = $"KB_{kb.kb_number}{fileExtension}";

                return File(kb.kb_file, kb.kb_type, fileName);
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
