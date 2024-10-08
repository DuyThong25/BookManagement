﻿using BookManager.Models.PaymentGate;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookManager.Models
{
	public class OrderHeader
	{
		public int Id { get; set; }

		public string ApplicationUserId { get; set; }
		public DateTime? OrderDate { get; set; }
		public DateTime? ShippingDate { get; set; }
        public DateTime? RefundOrderDate { get; set; }
        public DateTime? CancelOrderDate { get; set; }
        public double OrderTotal { get; set; }
		public string? OrderStatus { get; set; }
		public string? TrackingNumber { get; set; }
		public string? Carrier { get; set; }
		public DateTime? PaymentDate { get; set; }
		public DateOnly? PaymentDueDate { get; set; }
        public string? SessionId { get; set; }
		public string? PaymentStatus { get; set; }
        public string? PaymentIntentId { get; set; }

		public int? PaymentTransactionId { get; set; }
		[ForeignKey("PaymentTransactionId")]
		public PaymentTransaction PaymentTransaction { get; set; }

		[Required]
		public string Name { get; set; }
		[Required]

		public string Address { get; set; }
		[Required]

		public string Ward { get; set; }
		[Required]

		public string District { get; set; }
		[Required]

		public string City { get; set; }
		[Required]
		public string PhoneNumber { get; set; }

		[ForeignKey("ApplicationUserId")]
		[ValidateNever]
		public ApplicationUser ApplicationUser { get; set; }
	}
}
