using BookManager.DataAccess.Repository;
using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookManagementWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetail.Role_Admin + "," + StaticDetail.Role_Employee)]
    public class OrderStatisticController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderStatisticController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region apimethod
        [HttpPost]
        public IActionResult StatusOrderStatistic(string status)
        {
            IEnumerable<OrderHeader> listOrderFilter;
            switch (status)
            {
                case "weekly":
                    List<string> daysInWeek = new();
                    // Tính ngày thứ 2 của tuần hiện tại
                    int delta = DayOfWeek.Monday - DateTime.Today.DayOfWeek;
                    // Nếu hôm nay là chủ nhật, lùi lại một tuần để lấy ngày thứ 2 của tuần trước
                    if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday)
                    {
                        delta -= 7;
                    }
                    DateTime startOfWeek = DateTime.Today.AddDays(delta);
                    DateTime endOfWeek = startOfWeek.AddDays(7);
                    for (DateTime date = startOfWeek; date < endOfWeek; date = date.AddDays(1))
                    {
                        daysInWeek.Add(date.ToShortDateString());
                    }

                    listOrderFilter = _unitOfWork.OrderHeader.GetAll(x => x.OrderStatus == StaticDetail.OrderStatus_Approved &&
                            x.OrderDate >= startOfWeek && x.OrderDate <= endOfWeek).ToList();
                    return Json(new { success = true, data = listOrderFilter, dateInWeek = daysInWeek });
                case "inMonth":
                    List<string> daysInMonth = new();

                    // Tính ngày đầu tiên và ngày cuối cùng của tháng
                    DateTime startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                    // Tạo danh sách các ngày trong tháng
                    for (DateTime date = startOfMonth; date <= endOfMonth; date = date.AddDays(1))
                    {
                        daysInMonth.Add(date.ToShortDateString());
                    }

                    listOrderFilter = _unitOfWork.OrderHeader.GetAll(x => x.OrderStatus == StaticDetail.OrderStatus_Approved
                           && x.PaymentStatus == StaticDetail.PaymentStatus_Approved
                           && x.OrderDate >= startOfMonth && x.OrderDate <= endOfMonth).ToList();

                    return Json(new { success = true, data = listOrderFilter, dateInMonth = daysInMonth });
                case "yearly":
                    List<string> years = new();

                    // Tạo danh sách các năm từ dữ liệu đơn hàng
                    listOrderFilter = _unitOfWork.OrderHeader.GetAll(x => x.OrderStatus == StaticDetail.OrderStatus_Approved
                           && x.PaymentStatus == StaticDetail.PaymentStatus_Approved).ToList();

                    // Nhóm đơn hàng theo năm và tính tổng doanh thu
                    var groupedByYear = listOrderFilter
                        .GroupBy(order => order.OrderDate.Value.Year)
                        .Select(group => new
                        {
                            Year = group.Key,
                            TotalOrder = group.Sum(order => order.OrderTotal)
                        })
                        .OrderBy(group => group.Year)
                        .ToList();

                    // Tạo danh sách các năm để làm nhãn
                    years = groupedByYear.Select(g => g.Year.ToString()).ToList();

                    return Json(new { success = true, data = groupedByYear, dateInYear = years });
                case "monthly":
                    // Yearly
                    List<string> monthInYear = new();
                    List<OrderHeader> allOrdersInYear = new List<OrderHeader>();

                    // Lặp qua từng tháng trong năm
                    for (int month = 1; month <= 12; month++)
                    {
                        // Tính ngày đầu tiên và ngày cuối cùng của tháng
                        DateTime startOfMonthInYear = new DateTime(DateTime.Now.Year, month, 1);
                        DateTime endOfMonthInYear = startOfMonthInYear.AddMonths(1).AddDays(-1);

                        // Lấy tên tháng
                        string monthName = startOfMonthInYear.ToString("MMMM");
                        monthInYear.Add(monthName);

                        // Lấy danh sách đơn hàng trong tháng
                        listOrderFilter = _unitOfWork.OrderHeader.GetAll(x =>
                            x.OrderStatus == StaticDetail.OrderStatus_Approved
                            && x.PaymentStatus == StaticDetail.PaymentStatus_Approved
                            && x.OrderDate >= startOfMonthInYear
                            && x.OrderDate <= endOfMonthInYear).ToList();

                        allOrdersInYear.AddRange(listOrderFilter);
                    }
                    return Json(new { success = true, data = allOrdersInYear, monthInYear = monthInYear });
                default:
                    return Json(new { success = false });
            }
            #endregion
        }
    }
}
