using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models.ViewModel;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace BookManagementWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetail.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View(_unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser"));
        }

        public IActionResult Details(int orderId)
        {
            OrderVM? orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(x => x.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == orderId).ToList()
            };

            return View(orderVM);
        }

        #region API Method
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            //OrderVM? orderVM = _unitOfWork.Product.Get(x => x.ProductId == id);
            OrderVM? orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(x => x.Id == id, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == id).ToList()
            };

            if (orderVM != null)
            {
                //_unitOfWork.Product.Remove(product);
                //_unitOfWork.Save();
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
