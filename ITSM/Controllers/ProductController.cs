using ITSM_DomainModelEntity.Models;
using ITSM_DomainModelEntity.ViewModels;
using ITSM_Insfrastruture.Repository.Api;
using ITSM_Insfrastruture.Repository.Token;
using Microsoft.AspNetCore.Mvc;

namespace ITSM.Controllers
{
    public class ProductController : Controller
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
        private readonly Category_api _categoryApi;
        private readonly Product_api _productApi;
        private readonly Department_api _departmentApi;

        public ProductController(IHttpContextAccessor httpContextAccessor, UserService userService)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _userApi = new User_api(httpContextAccessor);
            _todoApi = new Todo_api(httpContextAccessor);
            _feedbackApi = new Feedback_api(httpContextAccessor);
            _incApi = new Incident_api(httpContextAccessor);
            _knowledgeApi = new Knowledge_api(httpContextAccessor);
            _reqApi = new Request_api(httpContextAccessor);
            _depApi = new Department_api(httpContextAccessor);
            _roleApi = new Role_api(httpContextAccessor);
            _categoryApi = new Category_api(httpContextAccessor);
            _productApi = new Product_api(httpContextAccessor);
            _departmentApi = new Department_api(httpContextAccessor);
        }

        public async Task<IActionResult> Product_List()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var productTask = _productApi.GetAllProduct_API();
            var categoryTask = _categoryApi.GetAllCategory_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            await Task.WhenAll(productTask, categoryTask, departmentTask);

            var allProduct = productTask.Result;
            var allCategory = categoryTask.Result;
            var allDepartment = departmentTask.Result;

            var productList = allProduct.OrderByDescending(y => y.id).ToList();

            foreach(var product in productList)
            {
                product.Category = allCategory.FirstOrDefault(x => x.id == product.category_id);
                product.ResponsibleDepartment = allDepartment.FirstOrDefault(x => x.id == product.responsible);
            }

            var model = new AllModelVM
            {
                user = currentUser,
                ProductList = productList,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        public async Task<IActionResult> Product_Create()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var categoryTask = _categoryApi.GetAllCategory_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            await Task.WhenAll(categoryTask, departmentTask);

            var allCategory = categoryTask.Result;
            var allDepartment = departmentTask.Result;

            var model = new AllModelVM()
            {
                user = currentUser,
                CategoryList = allCategory,
                DepartmentList = allDepartment,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Product_Create(IFormFile file, Product product)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var categoryTask = _categoryApi.GetAllCategory_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            var productTask = _productApi.GetAllProduct_API();
            await Task.WhenAll(categoryTask, departmentTask, productTask);

            var allCategory = categoryTask.Result;
            var allDepartment = departmentTask.Result;
            var allProduct = productTask.Result;

            var model = new AllModelVM()
            {
                user = currentUser,
                CategoryList = allCategory,
                DepartmentList = allDepartment,
                noteMessageCount = noteMessageCount
            };

            if (product.item_title != null && product.description != null && product.quantity >= 0)
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
                if (allProduct.Count > 0)
                {
                    var last_pro = allProduct.Last();
                    string p_id_up = last_pro.pro_number;
                    string prefix = new string(p_id_up.TakeWhile(char.IsLetter).ToArray());
                    string numberPart = new string(p_id_up.SkipWhile(char.IsLetter).ToArray());
                    int number = int.Parse(numberPart);
                    newId = prefix + (number + 1);
                }
                else
                    newId = "PRO1";

                Product new_product = new Product()
                {
                    pro_number = newId,
                    category_id = product.category_id,
                    item_title = product.item_title,
                    description = product.description,
                    quantity = product.quantity,
                    responsible = product.responsible,
                    product_type = product.product_type,
                };

                if (fileBytes != null)
                {
                    new_product.photo = fileBytes;
                    new_product.photo_type = GetMimeTypeFromFileSignature(fileBytes);
                }

                // create new product data
                bool result = await _productApi.CreateProduct_API(new_product);

                if (result)
                    return RedirectToAction("Product_List", "Product");
                else
                {
                    ViewBag.Error = "Create Product Error";
                    return View(model);
                }
            }
            else
            {
                ViewBag.Error = "Please fill in all required fields";
                return View(model);
            }
        }

        public async Task<IActionResult> Product_Info(int id)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var categoryTask = _categoryApi.GetAllCategory_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            await Task.WhenAll(categoryTask, departmentTask);

            var allCategory = categoryTask.Result;
            var allDepartment = departmentTask.Result;

            var pro_info = await _productApi.FindByIDProduct_API(id);
            
            pro_info.Category = allCategory.Where(x => x.id == pro_info.category_id).FirstOrDefault();
            pro_info.ResponsibleDepartment = allDepartment.Where(x => x.id == pro_info.responsible).FirstOrDefault();

            var model = new AllModelVM()
            {
                user = currentUser,
                product = pro_info,
                CategoryList = allCategory,
                DepartmentList = allDepartment,
                noteMessageCount = noteMessageCount
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Product_Info(IFormFile file, Product product)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            var noteMessageCount = await _userService.GetNoteAsync();

            var categoryTask = _categoryApi.GetAllCategory_API();
            var departmentTask = _departmentApi.GetAllDepartment_API();
            await Task.WhenAll(categoryTask, departmentTask);

            var allCategory = categoryTask.Result;
            var allDepartment = departmentTask.Result;

            var pro_info = await _productApi.FindByIDProduct_API(product.id);
            pro_info.Category = allCategory.Where(x => x.id == pro_info.category_id).FirstOrDefault();
            pro_info.ResponsibleDepartment = allDepartment.Where(x => x.id == pro_info.responsible).FirstOrDefault();

            var model = new AllModelVM()
            {
                user = currentUser,
                product = pro_info,
                CategoryList = allCategory,
                DepartmentList = allDepartment,
                noteMessageCount = noteMessageCount
            };

            if (product.item_title != null && product.description != null && product.quantity >= 0)
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

                pro_info.item_title = product.item_title;
                pro_info.description = product.description;
                pro_info.quantity = product.quantity;
                pro_info.category_id = product.category_id;
                pro_info.responsible = product.responsible;
                pro_info.active = product.active;
                pro_info.product_type = product.product_type;

                if (fileBytes != null)
                {
                    pro_info.photo = fileBytes;
                    pro_info.photo_type = GetMimeTypeFromFileSignature(fileBytes);
                }

                bool result = await _productApi.UpdateProduct_API(pro_info);

                if (result)
                    return RedirectToAction("Product_List", "Product");
                else
                {
                    ViewBag.Error = "Update Product Error";
                    return View(categoryTask);
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

            // HEIF (需要更多字节检查)
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

            // 默认
            return "application/octet-stream";
        }
    }
}
