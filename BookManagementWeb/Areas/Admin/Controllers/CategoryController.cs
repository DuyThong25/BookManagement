using BookManager.DataAccess.Data;
using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookManagementWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetail.Role_Admin)]

    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View(_unitOfWork.Category.GetAll());
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "Category Name không được trùng với Display Order");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(category);
                _unitOfWork.Save();
                TempData["success"] = "Category add successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int id)
        {
            Category? Category = _unitOfWork.Category.Get(x => x.CategoryId == id);
            if (Category == null)
            {
                return NotFound();
            }

            return View(Category);
        }
        [HttpPost]
        public IActionResult Edit(Category Category)
        {
            if (Category.Name == Category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "Category Name can not match Display Order");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(Category);
                _unitOfWork.Save();
                TempData["success"] = "Category edit successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int id)
        {
            Category? Category = _unitOfWork.Category.Get(x => x.CategoryId == id);
            if (Category == null)
            {
                return NotFound();
            }

            return View(Category);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int id)
        {
            Category? Category = _unitOfWork.Category.Get(x => x.CategoryId == id);
            if (Category != null)
            {
                _unitOfWork.Category.Remove(Category);
                _unitOfWork.Save();
                TempData["success"] = "Category delete successfully";
                return RedirectToAction("Index");

            }
            else
            {
                return NotFound();
            }
        }
    }
}
