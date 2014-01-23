using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{
    public class BalanceEntryDto
    {
        public BalanceEntryDto()
        {
        }

        private DateTime? _observedTime = null;

        public DateTime? ObservedTime
        {
            get
            {
                return _observedTime;
            }
            set
            {
                _observedTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        public string Description { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal Balance { get; set; }
    }
}



