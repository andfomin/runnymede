using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int SenderUserId { get; set; }
        public int RecipientUserId { get; set; }
        public string SenderDisplayName { get; set; }
        public string RecepientDisplayName { get; set; }
        public string Attribute { get; set; }
        public string Text { get; set; }

        private DateTime? _postTime = null;
        public DateTime? PostTime
        {
            get
            {
                return _postTime;
            }
            set
            {
                _postTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        private DateTime? _receiveTime = null;
        public DateTime? ReceiveTime
        {
            get
            {
                return _receiveTime;
            }
            set
            {
                _receiveTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }
    }
}