using BookManager.DataAccess.Data;
using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using BookManager.Models.PaymentGate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookManager.DataAccess.Repository
{
    public class PaymentTransactionRepository : Repository<PaymentTransaction>, IPaymentTransactionRepository
    {
        private ApplicationDbContext _db;
        public PaymentTransactionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(PaymentTransaction obj)
        {
           _db.PaymentTransactions.Update(obj);
        }
    }
}
