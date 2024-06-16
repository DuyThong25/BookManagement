using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookManagementWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeProperties: "Product"),
            };


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetBasePrice(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM();

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeProperties: "Product");
            if (!ShoppingCartVM.ShoppingCartList.IsNullOrEmpty())
            {
                ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(x => x.Id == userId);
                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    cart.Price = GetBasePrice(cart);
                    ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }

                ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
                ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
                ShoppingCartVM.OrderHeader.Ward = ShoppingCartVM.OrderHeader.ApplicationUser.Ward;
                ShoppingCartVM.OrderHeader.District = ShoppingCartVM.OrderHeader.ApplicationUser.District;
                ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
                ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
                return View(ShoppingCartVM);
            }
            else
            {
                TempData["error"] = "Cart is null";
                return RedirectToAction(nameof(Index));
            }

        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeProperties: "Product");
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(x => x.Id == userId);
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetBasePrice(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;

            if (applicationUser.CompanyID.GetValueOrDefault() == 0)
            {
                // GetValueOrDefault trả về giá trị 0 khi value trong db = null
                // Normal Custommer
                ShoppingCartVM.OrderHeader.OrderStatus = StaticDetail.OrderStatus_Pending;
                ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetail.PaymentStatus_Pending;
            }
            else
            {
                // Company Customer
                ShoppingCartVM.OrderHeader.OrderStatus = StaticDetail.OrderStatus_Approved;
                ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetail.PaymentStatus_ApprovedForDelayedPayment;
            }
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            // Save in table OrderDetail
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    ProductId = cart.ProductId,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            // qua trinh thanh toan cua normal customer
            if (applicationUser.CompanyID.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:7121/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };
                // configure san pham trong gio hang
                foreach (var cart in ShoppingCartVM.ShoppingCartList)
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
                _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId); // PaymentIntentId = null vì chua hoan tat thanh toan
                _unitOfWork.Save();

                // Chuyen den trang checkout
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }
            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            // Cap nhat OrderStatus va PaymentIntendID
            var orderHeaderFromDB = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if (orderHeaderFromDB != null)
            {
                if (orderHeaderFromDB.PaymentStatus != StaticDetail.PaymentStatus_ApprovedForDelayedPayment)
                {
                    var service = new SessionService();
                    Session session = service.Get(orderHeaderFromDB.SessionId);
                    if (session.PaymentStatus.ToLower() == "paid")
                    {
                        _unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                        _unitOfWork.OrderHeader.UpdateStatus(id, StaticDetail.OrderStatus_Approved, StaticDetail.PaymentStatus_Approved);
                        _unitOfWork.Save();

                        //Clear cart
                        List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
                            .GetAll(x => x.ApplicationUserId == orderHeaderFromDB.ApplicationUserId).ToList();
                        _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                        _unitOfWork.Save();
                    }
                }
            }
            return View(id);
        }

        private double GetBasePrice(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count < 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count < 100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }

        private double GetOrderTotalAPI(ShoppingCart cartUpdate, bool isDelete)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new();

            if (isDelete == true)
            {
                ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId && x.Id != cartUpdate.Id, includeProperties: "Product");
            }
            else
            {
                ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeProperties: "Product");
            }

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetBasePrice(cart);
                if (cart.Id == cartUpdate.Id)
                {
                    // This is a cart being update count
                    ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cartUpdate.Count);
                }
                else
                {
                    ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }
            }
            return ShoppingCartVM.OrderHeader.OrderTotal;
        }
        #region Api Method

        //[HttpGet]
        //public IActionResult GetAll(int cartId)
        //{
        //    var cartFromDb = _unitOfWork.ShoppingCart.GetAll(x => x.Id == cartId);
        //    return Json(new { cart = cartFromDb });
        //}
        [HttpPut("customer/cart/plus/{cartId}")]
        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId, includeProperties: "Product");

            if (cartFromDb != null)
            {
                cartFromDb.Price = GetBasePrice(cartFromDb);
                cartFromDb.Count += 1;
                double orderTotal = GetOrderTotalAPI(cartFromDb, false);
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
                return Json(new { cart = cartFromDb, total = orderTotal });
            }
            return NotFound();
        }

        [HttpPut("customer/cart/minus/{cartId}")]
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId, includeProperties: "Product");

            if (cartFromDb != null)
            {
                cartFromDb.Price = GetBasePrice(cartFromDb);
                cartFromDb.Count -= 1;
                double orderTotal;
                if (cartFromDb.Count == 0)
                {
                    // Remove cart
                    orderTotal = GetOrderTotalAPI(cartFromDb, isDelete: true);
                    _unitOfWork.ShoppingCart.Remove(cartFromDb);          
                    _unitOfWork.Save();
                    return Json(new { cart = cartFromDb, total = orderTotal });
                }
                else
                {
                    orderTotal = GetOrderTotalAPI(cartFromDb, isDelete: false);
                    _unitOfWork.ShoppingCart.Update(cartFromDb);
                    _unitOfWork.Save();
                    return Json(new { cart = cartFromDb, total = orderTotal });
                }
            }
            return NotFound();
        }

        [HttpDelete("customer/cart/delete/{cartId}")]
        public IActionResult Delete(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(x => x.Id == cartId, includeProperties: "Product");
            if (cartFromDb != null)
            {
                double orderTotal = GetOrderTotalAPI(cartFromDb, isDelete: true);
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
                //Update Session
                HttpContext.Session.SetInt32(StaticDetail.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
                _unitOfWork.Save();
                return Json(new { cart = cartFromDb, total = orderTotal });
            }
            return NotFound();
        }

        #endregion
    }
}
