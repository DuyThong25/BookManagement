using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookManager.Models.PaymentGate
{
    public class PaymentTransaction
    {
        [Key]
        public int Id { get; set; }
        public string? TransactionId { get; set; }
        public string? TransactionStatus { get; set; }  // e.g., 'Completed', 'Pending'
        public string? Currency { get; set; }  // e.g., 'USD'

        public int PaymentTypeId { get; set; }
        [ForeignKey("PaymentTypeId")]
        public PaymentType PaymentType { get; set; }
    }
}
