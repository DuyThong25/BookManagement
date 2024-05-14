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

            return View(_db.Categories.ToList());
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
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
