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
    public class PaymentTypeRepository : Repository<PaymentType>, IPaymentType
    {
        private ApplicationDbContext _db;
        public PaymentTypeRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(PaymentType obj)
        {
           _db.PaymentTypes.Update(obj);
        }
    }
}
