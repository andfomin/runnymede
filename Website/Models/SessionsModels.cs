using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{
    public class ScheduleEventDto
    {
        //<fc> These properties correspond to the ones in FullCalendar.
        public int Id { get; set; }
        public string Title { get; set; }

        private DateTime? _start = null;
        public DateTime? Start
        {
            get
            {
                return _start;
            }
            set
            {
                _start = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        private DateTime? _end = null;
        public DateTime? End
        {
            get
            {
                return _end;
            }
            set
            {
                _end = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }
        //</fc>

        // The following are custom properties
        public string Type { get; set; }
        public int? UserId { get; set; }
        public int? SecondUserId { get; set; }
        public decimal Price { get; set; }

        private DateTime? _creationTime = null;
        public DateTime? CreationTime
        {
            get
            {
                return _creationTime;
            }
            set
            {
                _creationTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        private DateTime? _confirmationTime = null;
        public DateTime? ConfirmationTime
        {
            get
            {
                return _confirmationTime;
            }
            set
            {
                _confirmationTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        private DateTime? _cancellationTime = null;
        public DateTime? CancellationTime
        {
            get
            {
                return _cancellationTime;
            }
            set
            {
                _cancellationTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        private DateTime? _closingTime = null;
        public DateTime? ClosingTime
        {
            get
            {
                return _closingTime;
            }
            set
            {
                _closingTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        // "Synthetic" properties. They do not correspond to database table fields.
        public string UserDisplayName { get; set; }
        public string UserSkype { get; set; }
        public string SecondUserDisplayName { get; set; }
        public string SecondUserSkype { get; set; }
    }

}