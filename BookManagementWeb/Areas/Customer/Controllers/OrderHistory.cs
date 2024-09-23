using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.PaymentGate;
using BookManager.Models.ViewModel;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
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
            return View(_unitOfWork.OrderHeader.GetAll(x => x.ApplicationUserId == userId, includeProperties: "ApplicationUser", orderByDescending: x => x.Id));
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
        [ActionName("Details")]
        public IActionResult DetailPOST()
        {
            var orederDetailFromDB = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "Product").ToList();
            var domain = Request.Scheme + "://" + Request.Host.Value + "/" /*"https://localhost:7121/"*/;
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + "customer/cart/index",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };
            // configure san pham trong gio hang
            foreach (var cart in orederDetailFromDB)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(cart.Price * 100), // $20.5 => 2500
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = cart.Product.Title,
                        }
                    },
                    Quantity = cart.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new Stripe.Checkout.SessionService();
            Session session = service.Create(options);
            //_unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId); // PaymentIntentId = null vì chua hoan tat thanh toan
            OrderHeader orderHeaderFromDB = _unitOfWork.OrderHeader.Get(x => x.Id == orederDetailFromDB.FirstOrDefault()!.OrderHeaderId, includeProperties: "PaymentTransaction");
            orderHeaderFromDB.PaymentTransaction.SessionId = session.Id;
            orderHeaderFromDB.PaymentTransaction.PaymentIntentId = session.PaymentIntentId;
            _unitOfWork.PaymentTransaction.Update(orderHeaderFromDB.PaymentTransaction);
            _unitOfWork.Save();

            // Chuyen den trang checkout
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
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
                TempData["error"] = "Not Found...";
                return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDB.Id });
            }
        }

        #region API METHOD
        [HttpPost("admin/orderhistory/CancelOrRefundOrder/{orderHeaderId}")]
        public IActionResult CancelOrRefundOrder(int orderHeaderId)
        {
            var orderHeaderFromDB = _unitOfWork.OrderHeader.Get(x => x.Id == orderHeaderId, includeProperties: "ApplicationUser,PaymentTransaction");

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
