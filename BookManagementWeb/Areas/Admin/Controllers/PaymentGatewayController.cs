using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models.PaymentGate;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookManagementWeb.Areas.Admin.Controllers
{
    [Area(StaticDetail.Role_Admin)]
    [Authorize(Roles = StaticDetail.Role_Admin + "," + StaticDetail.Role_Employee)]
    public class PaymentGatewayController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentGatewayController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var listPaymentType = _unitOfWork.PaymentType.GetAll();
            return View(listPaymentType);
        }

        public IActionResult ChangeDisplayStatus(int paymentTypeId, bool status)
        {
            try
            {
                PaymentType paymentType = _unitOfWork.PaymentType.Get(x => x.Id == paymentTypeId);
                paymentType.IsActive = status;
                _unitOfWork.PaymentType.Update(paymentType);
                _unitOfWork.Save();
                return Json(new { success = true, message = "Chuyển trạng thái thành công" });
            }
            catch
            {
                return Json(new { success = false, message = "Lỗi truy xuất dữ liệu" });
            }
        }
    }
}
