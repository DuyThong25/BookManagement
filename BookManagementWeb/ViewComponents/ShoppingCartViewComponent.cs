using BookManager.DataAccess.Repository.IRepository;
using BookManager.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace BookManagementWeb.ViewComponents
{
    public class ShoppingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                if (String.IsNullOrEmpty(HttpContext.Session.GetInt32(StaticDetail.SessionCart).ToString()))
                {
                    HttpContext.Session.SetInt32(StaticDetail.SessionCart,
                        _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == userId).Count());
                }
                return View(HttpContext.Session.GetInt32(StaticDetail.SessionCart));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }

        }
    }
}
