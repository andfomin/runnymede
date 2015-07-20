using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Generic;

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

    public class PayPalCartItem
    {
        public string Name { get; set; } // In the case of a lump payment Name does not matter. In the case of particular items it originates in the Title column in dbo.appTypes.
        public decimal? Amount { get; set; }
        public int Quantity { get; set; }
    }

    public class IncomingPayPalPayment
    {
        public string PaymentStatus { get; set; }
        public string TxnId { get; set; }
        public string Custom { get; set; } // Pass-through variable for tracking purposes, which buyers do not see. We pass the encripted UserId in it.
        public string Invoice { get; set; } // Pass-through variable. Invoice must be unique for every payment. Otherwise PayPal says that the invoice has beeen payed and refuses to proceed.
        public string ReceiverEmail { get; set; }
        public string McCurrency { get; set; }
        public decimal? McGross { get; set; }
        public decimal? McFee { get; set; }
        public decimal? Tax { get; set; }
        public string ResidenceCountry { get; set; }
        public string PayerId { get; set; }
        public IEnumerable<PayPalCartItem> CartItems{ get; set; }        
    }


}
