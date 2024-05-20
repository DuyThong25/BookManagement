using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BookManagementWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.Product.GetAll());
        }

        public IActionResult Create()
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

            return View(productVM);
        }
        [HttpPost]
        public IActionResult Create(ProductVM productVM)
        {       
            if (ModelState.IsValid)
            {   
                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product add successfully";
                return RedirectToAction("Index");
            }else
            {
                IEnumerable<SelectListItem> selectListItems =
                _unitOfWork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.CategoryId.ToString()
                });
                productVM.CategoryList = selectListItems;
                return View(productVM);
            }
        }

        public IActionResult Edit(int id)
        {
            Product? product = _unitOfWork.Product.Get(x => x.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            /*if (product.Name == product.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "Product Name không được trùng với Display Order");
            }*/
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();
                TempData["success"] = "Product edit successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int id)
        {
            Product? product = _unitOfWork.Product.Get(x => x.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int id)
        {
            Product? product = _unitOfWork.Product.Get(x => x.ProductId == id);
            if (product != null)
            {
                _unitOfWork.Product.Remove(product);
                _unitOfWork.Save();
                TempData["success"] = "Product delete successfully";
                return RedirectToAction("Index");

            }
            else
            {
                return NotFound();
            }
        }
    }
}
