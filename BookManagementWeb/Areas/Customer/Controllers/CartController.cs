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
            if(shoppingCart.Count < 50)
            {
                return shoppingCart.Product.Price;
            }else
            {
                if(shoppingCart.Count < 100)
                {
                    return shoppingCart.Product.Price50;
                }else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}
