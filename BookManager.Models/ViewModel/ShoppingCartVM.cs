using BookManager.Models.PaymentGate;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookManager.Models.ViewModel
{
    public class ShoppingCartVM
    {
        public ShoppingCartVM()
        {
            OrderHeader = new();
        }

        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; }
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<SelectListItem> PaymentTypeList { get; set; }
        public PaymentTransaction PaymentTransaction { get; set; }
    }
}
