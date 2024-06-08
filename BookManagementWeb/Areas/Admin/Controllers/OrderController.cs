using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

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
            OrderVM orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(x => x.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == orderId).ToList()
            };

            return View(orderVM);
        }

        #region API Method
        [HttpGet("admin/order/statusorder/{status}")]
        public IActionResult StatusOrder(string status)
        {
            IEnumerable<OrderHeader> listOrderHeader;
            switch (status)
            {
                case "approved":
                    listOrderHeader = _unitOfWork.OrderHeader
                        .GetAll(x => x.OrderStatus == StaticDetail.OrderStatus_Approved, includeProperties: "ApplicationUser").ToList();
                    break;
                case "completed":
                    listOrderHeader = _unitOfWork.OrderHeader
                        .GetAll(x => x.OrderStatus == StaticDetail.OrderStatus_Shipped, includeProperties: "ApplicationUser").ToList();
                    break;
                case "inprocess":
                    listOrderHeader = _unitOfWork.OrderHeader
                        .GetAll(x => x.OrderStatus == StaticDetail.OrderStatus_Processing, includeProperties: "ApplicationUser").ToList();
                    break;
                case "pending":
                    listOrderHeader = _unitOfWork.OrderHeader
                        .GetAll(x => x.OrderStatus == StaticDetail.OrderStatus_Pending, includeProperties: "ApplicationUser").ToList();
                    break;
                default:
                    // All
                    listOrderHeader = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
                    break;
            }
            return Json(new {data = listOrderHeader });
        }

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
