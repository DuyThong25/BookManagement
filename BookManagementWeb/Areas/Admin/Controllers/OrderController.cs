using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
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
            return View(_unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser", orderByDescending: x => x.Id));
        }

        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(x => x.Id == orderId, includeProperties: "ApplicationUser,PaymentTransaction"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == orderId, includeProperties: "Product").ToList()
            };
            return View(OrderVM);
        }

        [HttpPost]
        public IActionResult UpdateOrderDetails()
        {
            var orderHeaderFromDB = _unitOfWork.OrderHeader.Get(x => x.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser,PaymentTransaction");
            if (orderHeaderFromDB != null)
            {
                orderHeaderFromDB.Name = OrderVM.OrderHeader.Name;
                orderHeaderFromDB.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
                orderHeaderFromDB.Address = OrderVM.OrderHeader.Address;
                orderHeaderFromDB.Ward = OrderVM.OrderHeader.Ward;
                orderHeaderFromDB.District = OrderVM.OrderHeader.District;
                orderHeaderFromDB.City = OrderVM.OrderHeader.City;
                orderHeaderFromDB.Carrier = OrderVM.OrderHeader.Carrier;
                if (OrderVM.OrderHeader.PaymentTransaction.SessionId == null)
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

        [HttpPost]
        public IActionResult StartProcessing()
        {
            try
            {
                _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, StaticDetail.OrderStatus_Processing);
                _unitOfWork.Save();
                TempData["Success"] = "Start Processing Successfully.";

                return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });

            }
            catch (Exception ex)
            {
                TempData["Success"] = "Something wrong...";
                return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
            }
        }

        [HttpPost]
        public IActionResult StartShipOrder()
        {
            try
            {
                var orderHeaderFromDB = _unitOfWork.OrderHeader.Get(x => x.Id == OrderVM.OrderHeader.Id);
                orderHeaderFromDB.Carrier = OrderVM.OrderHeader.Carrier;
                orderHeaderFromDB.ShippingDate = DateTime.Now;
                orderHeaderFromDB.TrackingNumber = Guid.NewGuid().ToString();
                orderHeaderFromDB.OrderStatus = StaticDetail.OrderStatus_Shipped;
                if (orderHeaderFromDB.PaymentStatus == StaticDetail.PaymentStatus_ApprovedForDelayedPayment)
                {
                    orderHeaderFromDB.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
                }
                _unitOfWork.OrderHeader.Update(orderHeaderFromDB);
                _unitOfWork.Save();

                TempData["Success"] = "Start Ship Order Successfully.";

                return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
            }
            catch (Exception ex)
            {
                TempData["Success"] = "Something wrong...";
                return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
            }
        }

        #region API Method
        [HttpPost("admin/order/CancelOrRefundOrder/{orderHeaderId}")]
        public IActionResult CancelOrRefundOrder(int orderHeaderId)
        {
            var orderHeaderFromDB = _unitOfWork.OrderHeader.Get(x => x.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser,PaymentTransaction");
            if (orderHeaderFromDB.PaymentStatus == StaticDetail.PaymentStatus_Approved && orderHeaderFromDB.PaymentTransaction.PaymentIntentId != null)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDB.PaymentTransaction.PaymentIntentId,
                };

                var services = new RefundService();
                Refund refund = services.Create(options);

                orderHeaderFromDB.RefundOrderDate = DateTime.Now;

                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDB.Id, StaticDetail.OrderStatus_Refunded);
                TempData["Success"] = "Refund Order Successfully.";
            }
            else
            {
                orderHeaderFromDB.CancelOrderDate = DateTime.Now;

                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDB.Id, StaticDetail.OrderStatus_Cancelled);
                TempData["Success"] = "Cancel Order Successfully.";

            }
            _unitOfWork.Save();
            return Json(new { orderId = orderHeaderId });
        }
        [HttpGet("admin/order/statusorder/{status}")]
        public IActionResult StatusOrder(string status)
        {
            IEnumerable<OrderHeader> listOrderHeader;
            var statusMapping = new Dictionary<string, string>()
            {
                { "approved", StaticDetail.OrderStatus_Approved },
                { "completed", StaticDetail.OrderStatus_Shipped },
                { "inprocess", StaticDetail.OrderStatus_Processing },
                { "pending", StaticDetail.OrderStatus_Pending }
            };
            if (statusMapping.TryGetValue(status, out string orderStatus))
            {
                listOrderHeader = _unitOfWork.OrderHeader
                                .GetAll(x => x.OrderStatus == orderStatus, includeProperties: "ApplicationUser", orderByDescending: x => x.Id);
            }
            else
            {
                listOrderHeader = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser", orderByDescending: x => x.Id);
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
