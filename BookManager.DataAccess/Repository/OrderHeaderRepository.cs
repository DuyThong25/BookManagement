using BookManager.DataAccess.Data;
using BookManager.DataAccess.Repository.IRepository;
using BookManager.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookManager.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderHeaderFromDB = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (orderHeaderFromDB != null)
            {
                orderHeaderFromDB.OrderStatus = orderStatus;
                if (!String.IsNullOrEmpty(paymentStatus))
                {
                    orderHeaderFromDB.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntendId)
        {
            var orderHeaderFromDB = _db.OrderHeaders.Include(x => x.PaymentTransaction).FirstOrDefault(x => x.Id == id);
            //_db.OrderHeaders.FirstOrDefault(x => x.Id == id, includeProperties: "ApplicationUser,PaymentTransaction"),
            if (orderHeaderFromDB != null)
            {
                if (!String.IsNullOrEmpty(sessionId))
                {
                    orderHeaderFromDB.PaymentTransaction.SessionId = sessionId;
                }
                if (!String.IsNullOrEmpty(paymentIntendId)) // Khi thanh toan thanh cong roi moi tao ra paymentIntendId
                {
                    orderHeaderFromDB.PaymentTransaction.PaymentIntentId = paymentIntendId;
                    orderHeaderFromDB.PaymentDate = DateTime.Now;
                }
            }
        }
    }
}
