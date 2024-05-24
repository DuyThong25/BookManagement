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

        //VND format
        public static string VndCurrency()
        {
            return "#,##0.00 VNĐ";
        }
    }
}
