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
    [Authorize(Roles = StaticDetail.Role_Admin + "," + StaticDetail.Role_Employee)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
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
                productVM.Product = _unitOfWork.Product.Get(x => x.ProductId == id, includeProperties: "Category,ProductImages");
            }
            return View(productVM);

        }
        [HttpPost]
        public IActionResult CreateOrUpdate(ProductVM productVM, List<IFormFile>? files)
        {
            if (ModelState.IsValid)
            {
                if (productVM.Product.ProductId == 0) // Create
                {
                    _unitOfWork.Product.Add(productVM.Product);
                    _unitOfWork.Save();
                    TempData["success"] = "Product add successfully";
                }
                else // update
                {
                    _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.Save();
                    TempData["success"] = "Product edit successfully";
                }

                // Handle image
                if (files != null)
                {
                    productVM.Product.ProductImages = HandleToGetFileImage(productVM, files);
                    _unitOfWork.Product.Update(productVM.Product);
                    _unitOfWork.Save();
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

        public List<ProductImage> HandleToGetFileImage(ProductVM productVM, List<IFormFile>? files)
        {
            List<ProductImage> reuslt = new();

            foreach (IFormFile file in files)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + "-" + Path.GetFileName(file.FileName);
                string productPath = Path.Combine("images", "products", "product-" + productVM.Product.ProductId.ToString());
                string finalPath = Path.Combine(wwwRootPath, productPath);

                // If folder are not created -> create
                if (!Directory.Exists(finalPath))
                {
                    Directory.CreateDirectory(finalPath);
                }
                // copy hinh vao folder
                using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                ProductImage productImage = new()
                {
                    ImageUrl = @"\" + productPath + @"\" + fileName,
                    ProductId = productVM.Product.ProductId,
                };

                if (productImage.ImageUrl == null)
                {
                    reuslt = new List<ProductImage>();
                }
                else
                {
                    reuslt.Add(productImage);
                }
                //HandleDeleteFileImage(productVM.Product, wwwRootPath);
            }
            return reuslt;
        }

        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = _unitOfWork.ProductImage.Get(x => x.ID == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!String.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    string fileDelete = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(fileDelete))
                    {
                        System.IO.File.Delete(fileDelete);
                    }
                }

                _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                _unitOfWork.Save();
                TempData["success"] = "Deleted Succesfully.";
            }
            return RedirectToAction(nameof(CreateOrUpdate), new {id = productId });
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
                //HandleDeleteFileImage(product, wwwRootPath);
                return Json(new { success = true, message = "Delete Succesful" });
            }
            else
            {
                return Json(new { success = false, message = "Delete Fail" });
            }
        }

        #endregion

    }
}
