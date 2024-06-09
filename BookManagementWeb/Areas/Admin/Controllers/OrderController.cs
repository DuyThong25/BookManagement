using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe.Climate;
using System.Collections;

namespace BookManagementWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetail.Role_Admin + "," + StaticDetail.Role_Employee)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
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
            OrderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(x => x.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == orderId, includeProperties: "Product").ToList()
            };
            return View(OrderVM);
        }

        [HttpPost]
        public IActionResult UpdateOrderDetails()
        {
            var orderHeaderFromDB = _unitOfWork.OrderHeader.Get(x => x.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            if (orderHeaderFromDB != null)
            {
                orderHeaderFromDB.Name = OrderVM.OrderHeader.Name;
                orderHeaderFromDB.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
                orderHeaderFromDB.Address = OrderVM.OrderHeader.Address;
                orderHeaderFromDB.Ward = OrderVM.OrderHeader.Ward;
                orderHeaderFromDB.District = OrderVM.OrderHeader.District;
                orderHeaderFromDB.City = OrderVM.OrderHeader.City;
                if (OrderVM.OrderHeader.SessionId == null)
                {
                    orderHeaderFromDB.PaymentDueDate = OrderVM.OrderHeader.PaymentDueDate;
                }
                else
                {
                    orderHeaderFromDB.ShippingDate = OrderVM.OrderHeader.ShippingDate;
                    orderHeaderFromDB.Carrier = OrderVM.OrderHeader.Carrier;
                    orderHeaderFromDB.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
                }
                _unitOfWork.OrderHeader.Update(orderHeaderFromDB);
                _unitOfWork.Save();
                TempData["Success"] = "Pickup Information Update Successfully.";

                return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDB.Id });
            }
            else
            {
                return NotFound();
            }
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
            return Json(new { data = listOrderHeader });
        }

        //[HttpDelete]
        //public IActionResult Delete(int id)
        //{
        //    //OrderVM? orderVM = _unitOfWork.Product.Get(x => x.ProductId == id);
        //    OrderVM? orderVM = new()
        //    {
        //        OrderHeader = _unitOfWork.OrderHeader.Get(x => x.Id == id, includeProperties: "ApplicationUser"),
        //        OrderDetails = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == id).ToList()
        //    };

        //    if (orderVM != null)
        //    {
        //        //_unitOfWork.Product.Remove(product);
        //        //_unitOfWork.Save();
        //        return Json(new { success = true, message = "Delete Succesful" });
        //    }
        //    else
        //    {
        //        return Json(new { success = false, message = "Delete Fail" });
        //    }
        //}
        #endregion
    }
}
