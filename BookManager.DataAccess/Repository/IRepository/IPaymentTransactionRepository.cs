
using BookManager.Models.PaymentGate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookManager.DataAccess.Repository.IRepository
{
    public interface IPaymentTransactionRepository : IRepository<PaymentTransaction>
    {
        void Update(PaymentTransaction obj);
    }
}
