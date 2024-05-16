using BookManagementWeb.Data;
using BookManagementWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookManagementWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            this._db = db;
        }
        public IActionResult Index()
        {

            return View(_db.Categories.OrderBy(x => x.DisplayOrder).ToList());
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
                _db.Categories.Add(category);
                _db.SaveChanges();
                TempData["success"] = "Category add successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int id)
        {
            Category? category = _db.Categories.Find(id);
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
                _db.Categories.Update(category);
                _db.SaveChanges();
                TempData["success"] = "Category edit successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int id)
        {
            Category? category = _db.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirm(int id)
        {
            Category? category = _db.Categories.Find(id);
            if(category != null)
            {
                _db.Categories.Remove(category);
                _db.SaveChanges();
                TempData["success"] = "Category delete successfully";
                return RedirectToAction("Index");

            }else
            {
                return NotFound();
            }
        }
    }
}
