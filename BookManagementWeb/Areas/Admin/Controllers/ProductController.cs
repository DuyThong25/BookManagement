using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;

namespace BookManagementWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetail.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment ;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.Product.GetAll(includeProperties: "Category"));
        }

        public IActionResult CreateOrUpdate(int? id)
        {
            IEnumerable<SelectListItem> selectListItems =
                _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.CategoryId.ToString()
                });
            /*ViewBag.SelectListItems = selectListItems;*/
            /*TempData["SelectListItems"] = selectListItems;*/ /*phai ep kieu khi su dung*/

            ProductVM productVM = new ProductVM
            {
                Product = new Product(),
                CategoryList = selectListItems
            };
            if (id != null && id != 0) // update
            {
                productVM.Product = _unitOfWork.Product.Get(x => x.ProductId == id, includeProperties: "Category");
            }
            return View(productVM);

        }
        [HttpPost]
        public IActionResult CreateOrUpdate(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {   
                // Handle imgae
                if(file != null)
                {
                    productVM.Product.ImageUrl = HandleToGetFileImage(productVM, file);
                }

                if(productVM.Product.ProductId == 0) // Create
                {
                    _unitOfWork.Product.Add(productVM.Product);
                    _unitOfWork.Save();
                    TempData["success"] = "Product add successfully";
                }else // update
                {
                    _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.Save();
                    TempData["success"] = "Product edit successfully";
                }
                return RedirectToAction("Index");
            }
            else
            {
                IEnumerable<SelectListItem> selectListItems =
                _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.CategoryId.ToString()
                });
                productVM.CategoryList = selectListItems;
                TempData["error"] = "Something went wrong..";
                return View(productVM);
            }
        }
        public void HandleDeleteFileImage(Product product,string wwwRootPath)
        {
            if (!String.IsNullOrEmpty(product.ImageUrl)) // update -> delete file cu
            {
                string fileDelete = Path.Combine(wwwRootPath, product.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(fileDelete))
                {
                    System.IO.File.Delete(fileDelete);
                }
            }
        }
        public string HandleToGetFileImage(ProductVM productVM, IFormFile file)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            string productPath = Path.Combine(wwwRootPath, @"images\product");
            string fileName = Guid.NewGuid().ToString() + Path.GetFileName(file.FileName);

            HandleDeleteFileImage(productVM.Product, wwwRootPath);
            using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
            {
                file.CopyTo(fileStream);
            }
            return @"\images\product\" + fileName;
        }

        #region Api Method

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> listProduct = _unitOfWork.Product.GetAll().ToList();
            return Json(new { data = listProduct });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Product? product = _unitOfWork.Product.Get(x => x.ProductId == id);
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            if (product != null)
            {
                _unitOfWork.Product.Remove(product);
                _unitOfWork.Save();
                HandleDeleteFileImage(product, wwwRootPath);
                return Json(new { success = true, messag = "Delete Succesful" });
            }
            else
            {
                return Json(new { success = false, message = "Delete Fail" });
            }
        }

        #endregion

    }
}
