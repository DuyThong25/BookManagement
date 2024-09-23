using BookManager.DataAccess.Data;
using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.PaymentGate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookManager.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        private ICategoryRepository _category;
        private IProductRepository _product;
        private ICompanyRepository _company;
        private IShoppingCartRepository _shoppingCart;
        private IApplicationUser _applicationUser;
        private IOrderHeaderRepository _orderHeader;
        private IOrderDetailRepository _orderDetail;
        private IProductImageRepository _productImage;
        private IPaymentTypeRepository _paymentType;
        private IPaymentTransactionRepository _paymentTransaction;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
        }

        public ICategoryRepository Category
        {
            get
            {
                return _category ??= new CategoryRepository(_db);
            }
        }

        public IProductRepository Product
        {
            get
            {
                return _product ??= new ProductRepository(_db);
            }
        }

        public ICompanyRepository Company
        {
            get
            {
                return _company ??= new CompanyRepository(_db);
            }
        }

        public IShoppingCartRepository ShoppingCart
        {
            get
            {
                return _shoppingCart ??= new ShoppingCartRepository(_db);
            }
        }

        public IApplicationUser ApplicationUser
        {
            get
            {
                return _applicationUser ??= new ApplicationUserRepository(_db);
            }
        }

        public IOrderHeaderRepository OrderHeader
        {
            get
            {
                return _orderHeader ??= new OrderHeaderRepository(_db);
            }
        }

        public IOrderDetailRepository OrderDetail
        {
            get
            {
                return _orderDetail ??= new OrderDetailRepository(_db);
            }
        }

        public IProductImageRepository ProductImage
        {
            get
            {
                return _productImage ??= new ProductImageRepository(_db);
            }
        }

        public IPaymentTypeRepository PaymentType
        {
            get
            {
                return _paymentType ??= new PaymentTypeRepository(_db);
            }
        }

        public IPaymentTransactionRepository PaymentTransaction
        {
            get
            {
                return _paymentTransaction ??= new PaymentTransactionRepository(_db);
            }
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }

}
