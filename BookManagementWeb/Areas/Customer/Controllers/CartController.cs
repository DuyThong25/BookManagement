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
        private ShoppingCartVM ShoppingCartVM;

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
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId, includeProperties: "Product")
            };


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetBasePrice(cart);
                ShoppingCartVM.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
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

        private double GetOrderTotal(ShoppingCart cartUpdate, bool isDelete)
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new ShoppingCartVM();
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
                    ShoppingCartVM.OrderTotal += (cart.Price * cartUpdate.Count);
                }
                else
                {
                    ShoppingCartVM.OrderTotal += (cart.Price * cart.Count);
                }
            }
            return ShoppingCartVM.OrderTotal;
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
                double orderTotal = GetOrderTotal(cartFromDb, false);
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
                    orderTotal = GetOrderTotal(cartFromDb, isDelete: true);
                    _unitOfWork.ShoppingCart.Remove(cartFromDb);
                    _unitOfWork.Save();
                    return Json(new { cart = cartFromDb, total = orderTotal });
                }
                else
                {
                    orderTotal = GetOrderTotal(cartFromDb, isDelete: false);
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
                double orderTotal = GetOrderTotal(cartFromDb, isDelete: true);
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
                _unitOfWork.Save();
                return Json(new { cart = cartFromDb, total = orderTotal });
            }
            return NotFound();
        }

        #endregion
    }
}
