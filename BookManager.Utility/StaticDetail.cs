using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookManager.Utility
{
    public static class StaticDetail
    {
        public const string Role_Customer = "Customer";
        public const string Role_Company = "Company";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

        public static string OrderStatus_Pending = "Pending";
        public static string OrderStatus_Approved = "Approved";
        public static string OrderStatus_Processing = "Processing";
        public static string OrderStatus_Shipped = "Shipped";
        public static string OrderStatus_Refunded = "Refunded";
        public static string OrderStatus_Cancelled = "Cancelled";


        public static string PaymentStatus_Pending = "Pending";
        public static string PaymentStatus_Approved = "Approved";
        public static string PaymentStatus_ApprovedForDelayedPayment = "ApprovedForDelayedPayment";

        public static string SessionCart = "SessionShoppingCart";

        //VND format
        public static string VndCurrency()
        {
            return "#,##0.00 VNĐ";
        }
    }
}
