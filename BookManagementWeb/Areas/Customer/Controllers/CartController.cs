using BookManager.DataAccess.Data;
using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnitOfWork unitOfWork, ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _db = db;
        }

        public IActionResult Index()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (TempData["orderHeaderID"] != null)
            {
                var orderHeaderFromDB = _unitOfWork.OrderHeader.Get(x => x.Id == TempData["orderHeaderID"] as int?, includeProperties: "ApplicationUser");
                if (orderHeaderFromDB != null && orderHeaderFromDB.ApplicationUser.CompanyID.GetValueOrDefault() == 0)
                {
                    // normal customer
                    _unitOfWork.OrderHeader.Remove(orderHeaderFromDB);
                    _unitOfWork.Save();
                }
            }
            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeProperties: "Product"),
            };

            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Product.ProductImages = productImages.Where(x => x.ProductId == cart.ProductId).ToList();
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
            ShoppingCartVM.PaymentTypeList = _unitOfWork.PaymentType.GetAll(x => x.IsActive == true).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            });
            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeProperties: "Product");
            if (!ShoppingCartVM.ShoppingCartList.IsNullOrEmpty())
            {
                ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(x => x.Id == userId);
                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    cart.Price = GetBasePrice(cart);
                    ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }
                if (ShoppingCartVM.OrderHeader.ApplicationUser.CompanyID.GetValueOrDefault() == 0)
                {
                    //Normal Customer
                    ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
                    ShoppingCartVM.OrderHeader.Address = ShoppingCartVM.OrderHeader.ApplicationUser.Address;
                    ShoppingCartVM.OrderHeader.Ward = ShoppingCartVM.OrderHeader.ApplicationUser.Ward;
                    ShoppingCartVM.OrderHeader.District = ShoppingCartVM.OrderHeader.ApplicationUser.District;
                    ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
                    ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
                }
                else
                {
                    //Company customer
                    var companyFromDB = _unitOfWork.Company.Get(x => x.Id == ShoppingCartVM.OrderHeader.ApplicationUser.CompanyID);
                    ShoppingCartVM.OrderHeader.Name = companyFromDB.Name;
                    ShoppingCartVM.OrderHeader.Address = companyFromDB.Address;
                    ShoppingCartVM.OrderHeader.Ward = companyFromDB.Ward;
                    ShoppingCartVM.OrderHeader.District = companyFromDB.District;
                    ShoppingCartVM.OrderHeader.City = companyFromDB.City;
                    ShoppingCartVM.OrderHeader.PhoneNumber = companyFromDB.PhoneNumber;
                }

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
            try
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
                    var domain = Request.Scheme + "://" + Request.Host.Value + "/" /*"https://localhost:7121/"*/;
                    var options = new Stripe.Checkout.SessionCreateOptions
                    {
                        SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                        CancelUrl = domain + "customer/cart/index",
                        LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                        Mode = "payment",
                    };
                    TempData["orderHeaderID"] = ShoppingCartVM.OrderHeader.Id;
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
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult OrderConfirmation(int id)
        {
            using var transaction = _db.Database.BeginTransaction();
            // Cap nhat OrderStatus va PaymentIntendID
            var orderHeaderFromDB = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            try
            {
                if (orderHeaderFromDB != null)
                {
                    if (!String.IsNullOrEmpty(orderHeaderFromDB.SessionId))
                    {
                        var service = new SessionService();
                        Session session = service.Get(orderHeaderFromDB.SessionId);
                        if (session.PaymentStatus.ToLower() == "paid")
                        {
                            _unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                            _unitOfWork.OrderHeader.UpdateStatus(id, StaticDetail.OrderStatus_Approved, StaticDetail.PaymentStatus_Approved);
                            _unitOfWork.Save();

                            //Send Mail After Payment APPROVED
                            string teamplate = TemplateEmailConfirmOrder(orderHeaderFromDB);
                            _emailSender.SendEmailAsync(orderHeaderFromDB.ApplicationUser.Email
                                , $"Thank you for your order - Your Order is {orderHeaderFromDB.Id}"
                                , teamplate);
                            //Clear cart
                            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
                                .GetAll(x => x.ApplicationUserId == orderHeaderFromDB.ApplicationUserId).ToList();
                            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                            _unitOfWork.Save();
                            //Clear session
                            HttpContext.Session.Remove(StaticDetail.SessionCart);
                            transaction.Commit();
                        }
                    }
                }

                return View(orderHeaderFromDB);
            }
            catch
            {
                transaction.Rollback();
                return View(orderHeaderFromDB);
            }
        }

        private string TemplateEmailConfirmOrder(OrderHeader orderHeader)
        {
            var orderDetailFormDb = _unitOfWork.OrderDetail.GetAll(x => x.OrderHeaderId == orderHeader.Id, includeProperties: "Product").ToList();
            string wwwRootPath = _webHostEnvironment.WebRootPath;

            string productList = string.Empty;
            double totalPrice = 0.0;
            double TotalPriceAfterSurcharge = 0.0; // The Final Total Price
            string CustomerAddress = string.Empty;
            foreach (var item in orderDetailFormDb)
            {
                productList += "<tr>";
                productList += "<td>" + item.Product.Title + "</td>";
                productList += "<td>" + item.Count + "</td>";
                productList += "<td>" + item.Price.ToString("c") + "</td>";
                productList += "</tr>";

                totalPrice += item.Price * item.Count;
            }

            // Neu co giam gia thi se lay totalPrice - Discount và cuoi cung thi gan
            // TotalPriceAfterSurcharge = totalPrice
            TotalPriceAfterSurcharge = totalPrice;
            CustomerAddress = $"{orderHeader.Address.Replace("đường", "").Replace("Đường", "")} street, Ward {orderHeader.Ward}, {orderHeader.District} District, {orderHeader.City} City";

            string contentEmailConfirmOrder = System.IO.File.ReadAllText(Path.Combine(wwwRootPath, "template\\emails\\EmailConfirmOrder.html"));
            contentEmailConfirmOrder = contentEmailConfirmOrder.Replace("{{OrderID}}", orderHeader.Id.ToString());
            contentEmailConfirmOrder = contentEmailConfirmOrder.Replace("{{TrangThaiDon}}", orderHeader.PaymentStatus);
            contentEmailConfirmOrder = contentEmailConfirmOrder.Replace("{{OrderDate}}", orderHeader.OrderDate.ToString());
            contentEmailConfirmOrder = contentEmailConfirmOrder.Replace("{{CustomerName}}", orderHeader.Name);
            contentEmailConfirmOrder = contentEmailConfirmOrder.Replace("{{CustomerPhone}}", orderHeader.PhoneNumber);
            contentEmailConfirmOrder = contentEmailConfirmOrder.Replace("{{CustomerAddress}}", CustomerAddress);
            contentEmailConfirmOrder = contentEmailConfirmOrder.Replace("{{CustomerEmail}}", orderHeader.ApplicationUser.Email);
            contentEmailConfirmOrder = contentEmailConfirmOrder.Replace("{{Note}}", string.Empty);
            contentEmailConfirmOrder = contentEmailConfirmOrder.Replace("{{ProductList}}", productList);
            contentEmailConfirmOrder = contentEmailConfirmOrder.Replace("{{TotalPrice}}", totalPrice.ToString("c"));
            contentEmailConfirmOrder = contentEmailConfirmOrder.Replace("{{Discount}}", "0");
            contentEmailConfirmOrder = contentEmailConfirmOrder.Replace("{{TotalPriceAfterSurcharge}}", TotalPriceAfterSurcharge.ToString("c"));

            return contentEmailConfirmOrder;
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
