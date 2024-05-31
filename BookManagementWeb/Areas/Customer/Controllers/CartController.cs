using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookManagementWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private ShoppingCartVM _shoppingCartVM;

        public CartController(IUnitOfWork unitOfWork)
        {
            _shoppingCartVM = new();
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            _shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeProperties: "Product"),
            };


            foreach (var cart in _shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetBasePrice(cart);
                _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }
            return View(_shoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            _shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeProperties: "Product");
            _shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(x => x.Id == userId);
            foreach (var cart in _shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetBasePrice(cart);
                _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            _shoppingCartVM.OrderHeader.Name = _shoppingCartVM.OrderHeader.ApplicationUser.Name;
            _shoppingCartVM.OrderHeader.Address = _shoppingCartVM.OrderHeader.ApplicationUser.Address;
            _shoppingCartVM.OrderHeader.Ward = _shoppingCartVM.OrderHeader.ApplicationUser.Ward;
            _shoppingCartVM.OrderHeader.District = _shoppingCartVM.OrderHeader.ApplicationUser.District;
            _shoppingCartVM.OrderHeader.City = _shoppingCartVM.OrderHeader.ApplicationUser.City;
            _shoppingCartVM.OrderHeader.PhoneNumber = _shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;


            return View(_shoppingCartVM);
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

            if (isDelete == true)
            {
                _shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId && x.Id != cartUpdate.Id, includeProperties: "Product");
            }
            else
            {
                _shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeProperties: "Product");
            }

            foreach (var cart in _shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetBasePrice(cart);
                if (cart.Id == cartUpdate.Id)
                {
                    // This is a cart being update count
                    _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cartUpdate.Count);
                }
                else
                {
                    _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }
            }
            return _shoppingCartVM.OrderHeader.OrderTotal;
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
