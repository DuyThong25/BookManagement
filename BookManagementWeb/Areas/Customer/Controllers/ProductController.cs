using BookManager.DataAccess.Data;
using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookManagementWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _db;
        public ProductController(IUnitOfWork unitOfWork, ApplicationDbContext db)
        {
            _unitOfWork = unitOfWork;
            _db = db;
        }

        public IActionResult Index()
        {
            // Lấy ra danh sách sản phẩm
            var listProduct = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            ProductFilterVM productFilterVM = new()
            {
                Products = listProduct,
                Categories = _unitOfWork.Category.GetAll()
            };
            return View(productFilterVM);
        }

        public IActionResult GetProductListByFilter(ProductFilterVM productFilterVM)
        {
            IQueryable<Product> query = _db.Products.AsNoTracking();
            List<int> selectedCategoryId = productFilterVM.SelectedCategoryIds;
            List<int> PriceRanges = productFilterVM.RangerPrice; // vị trí 0 là min price - vị trí 1 là max price
            if (selectedCategoryId != null)
            {
                query = query.Where(x => selectedCategoryId.Contains(x.CategoryId));
            }
            if (productFilterVM.SearchInput != null)
            {
                string searchInput = productFilterVM.SearchInput.Trim().ToLower();
                query = query.Where(x => x.Title.Trim().ToLower().Contains(searchInput));

            }
            if (productFilterVM.RangerPrice != null)
            {
                query = query.Where(x => x.Price100 >= PriceRanges[0] && x.Price <= PriceRanges[1]);
            }
            productFilterVM = new()
            {
                Products = query.Include("Category").Include("ProductImages").ToList(),
                Categories = _unitOfWork.Category.GetAll(),
                SelectedCategoryIds = selectedCategoryId
            };

            return PartialView("_ProductsFilterPartial", productFilterVM);
        }
    }
}
