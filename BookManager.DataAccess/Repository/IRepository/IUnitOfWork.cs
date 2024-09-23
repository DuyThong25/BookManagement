using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookManager.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IShoppingCartRepository ShoppingCart  { get; }
        IApplicationUser ApplicationUser { get; }
		IOrderHeaderRepository OrderHeader { get; }
		IOrderDetailRepository OrderDetail { get; }
		IProductImageRepository ProductImage { get; }
		IPaymentTypeRepository PaymentType { get; }
		IPaymentTransactionRepository PaymentTransaction { get; }

		void Save();

    }
}
