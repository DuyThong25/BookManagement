using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
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
                _unitOfWork.Save();
                return Json(new { cart = cartFromDb, total = orderTotal });
            }
            return NotFound();
        }

        #endregion
    }
}
