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

        public KnowledgeController(IHttpContextAccessor httpContextAccessor)
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
        }

        public IActionResult KB_Home()
        {
            return View();
        }

        public async Task<IActionResult> KB_List()
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var CategoryTask = _categoryApi.GetAllCategory_API();
            var UserTask = _userApi.GetAllUser_API();
            var KBTask = _kbApi.GetAllKnowledge_API();

            await Task.WhenAll(CategoryTask, UserTask, KBTask);

            var allCategory = await CategoryTask;
            var allUser = await UserTask;
            var allKB = await KBTask;

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

        public IActionResult KB_Info()
        {
            return View();
        }

        public IActionResult KB_Search()
        {
            return View();
        }

        public IActionResult KB_Read()
        {
            return View();
        }

        public async Task<IActionResult> KB_Create()
        {
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var CategoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(CategoryTask);

            var allCategory = await CategoryTask;

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
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var CategoryTask = _categoryApi.GetAllCategory_API();
            var KBTask = _kbApi.GetAllKnowledge_API();
            await Task.WhenAll(CategoryTask, KBTask);

            var allCategory = await CategoryTask;
            var allKB = await KBTask;

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
                    return RedirectToAction("KB_List", "Knowledge");
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
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var CategoryTask = _categoryApi.GetAllCategory_API();
            await Task.WhenAll(CategoryTask);

            var allCategory = await CategoryTask;

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
            // current user info
            var tokenService = new TokenService(_httpContextAccessor);
            var currentUser_token = tokenService.GetUserInfo();

            var currentUser = await _userApi.FindByIDUser_API(currentUser_token.id);

            var CategoryTask = _categoryApi.GetAllCategory_API();
            var KBTask = _kbApi.GetAllKnowledge_API();
            await Task.WhenAll(CategoryTask, KBTask);

            var allCategory = await CategoryTask;
            var allKB = await KBTask;

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

                if (file != null && file.Length > 50_000_000) // 50MB
                {
                    ViewBag.Error = "File size exceeds 50MB limit";
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
                    return RedirectToAction("KB_List", "Knowledge");
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
    }
}
