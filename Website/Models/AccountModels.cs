﻿using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Runnymede.Website.Models
{

    public class SignupBindingModel
    {
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)] // UserManager.CreateAsync() enforces the minimum length 6 characters internally.
        public string Password { get; set; }

        [StringLength(100)]
        public string Name { get; set; }
    }

    public class ExternalLoginBindingModel
    {
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(100)]
        public string Name { get; set; }
    }

    public class BalanceEntryDto
    {
        private DateTime? observedTime;
        public DateTime? ObservedTime
        {
            get
            {
                return observedTime;
            }
            set
            {
                observedTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        public string Description { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal Balance { get; set; }
    }

    public class IncomingPayPalPayment
    {
        public string PaymentStatus { get; set; }
        public string TxnId { get; set; }
        public string OptionSelection2 { get; set; }
        public string ReceiverEmail { get; set; }
        public string McCurrency { get; set; }
        public decimal? McGross { get; set; }
        public decimal? McFee { get; set; }
        public decimal? Tax { get; set; }
        public string ResidenceCountry { get; set; }
        public string Memo { get; set; }
        public string PayerId { get; set; }
    }


}
