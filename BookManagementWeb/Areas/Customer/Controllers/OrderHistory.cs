using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookManagementWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class OrderHistory : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderHistory(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(_unitOfWork.OrderHeader.GetAll(x => x.ApplicationUserId == userId, includeProperties: "ApplicationUser"));
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
                TempData["error"] = "Not Found...";
                return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDB.Id });
            }
        }
        #region API METHOD

        [HttpGet("customer/orderhistory/statusorder/{status}")]
        public IActionResult StatusOrder(string status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IEnumerable<OrderHeader> listOrderHeader;
            switch (status)
            {
                case "approved":
                    listOrderHeader = _unitOfWork.OrderHeader
                        .GetAll(x => x.OrderStatus == StaticDetail.OrderStatus_Approved && x.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();
                    break;
                case "completed":
                    listOrderHeader = _unitOfWork.OrderHeader
                        .GetAll(x => x.OrderStatus == StaticDetail.OrderStatus_Shipped && x.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();
                    break;
                case "inprocess":
                    listOrderHeader = _unitOfWork.OrderHeader
                        .GetAll(x => x.OrderStatus == StaticDetail.OrderStatus_Processing && x.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();
                    break;
                case "pending":
                    listOrderHeader = _unitOfWork.OrderHeader
                        .GetAll(x => x.OrderStatus == StaticDetail.OrderStatus_Pending && x.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();
                    break;
                default:
                    // All
                    listOrderHeader = _unitOfWork.OrderHeader
                        .GetAll(x => x.ApplicationUserId == userId, includeProperties: "ApplicationUser").ToList();
                    break;
            }
            return Json(new { data = listOrderHeader });
        }
        #endregion

    }
}
