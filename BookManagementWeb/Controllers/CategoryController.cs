using BookManager.DataAccess.Data;
using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookManagementWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
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
            if(category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "Category Name không được trùng với Display Order");
            }
            if(ModelState.IsValid)
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
            Category? category = _unitOfWork.Category.Get(x => x.CategoryId == id);
            if(category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "Category Name không được trùng với Display Order");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(category);
                _unitOfWork.Save();
                TempData["success"] = "Category edit successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int id)
        {
            Category? category = _unitOfWork.Category.Get(x => x.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int id)
        {
            Category? category = _unitOfWork.Category.Get(x => x.CategoryId == id);
            if (category != null)
            {
                _unitOfWork.Category.Remove(category);
                _unitOfWork.Save();
                TempData["success"] = "Category delete successfully";
                return RedirectToAction("Index");

            }else
            {
                return NotFound();
            }
        }
    }
}
