using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace BookManagementWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            // Lấy ra danh sách sản phẩm
            var listProduct = _unitOfWork.Product.GetAll();
            ProductFilterVM productFilterVM = new()
            {
                Products = listProduct,
                Categories = _unitOfWork.Category.GetAll()
            };
            return View(productFilterVM);
        }

        public IActionResult GetProductListByFilter(ProductFilterVM productFilterVM)
        {


            return PartialView();
        }
    }
}
