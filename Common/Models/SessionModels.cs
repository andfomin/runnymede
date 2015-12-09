using Itenso.TimePeriod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace Runnymede.Common.Models
{

    public class ItalkiTeacher
    {
        public int UserId { get; set; } // In our system
        public string DisplayName { get; set; }
        public int ItalkiUserId { get; set; }
        public int CourseId { get; set; }
        public string ScheduleUrl { get; set; }
        public decimal Rate { get; set; }
    }

    public class TeacherTimeRange : TimeRange
    {
        public int UserId { get; set; }
        public decimal Rate { get; set; }
    }

    public class FullCalendarEventDto
    {
        //<fc> These properties correspond to the ones in FullCalendar.
        public int? Id { get; set; }

        private DateTime start;
        public DateTime Start
        {
            get
            {
                return start;
            }
            set
            {
                start = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }

        private DateTime end;
        public DateTime End
        {
            get
            {
                return end;
            }
            set
            {
                end = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }
        //</fc>
    }

    public class SessionDto : FullCalendarEventDto
    {
        public int? TeacherUserId { get; set; }
        public int? LearnerUserId { get; set; }
        public decimal? Cost { get; set; }
        public decimal Price { get; set; }
        public byte? Rating { get; set; }

        private DateTime? bookingTime = null;
        public DateTime? BookingTime
        {
            get
            {
                return bookingTime;
            }
            set
            {
                bookingTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        private DateTime? confirmationTime = null;
        public DateTime? ConfirmationTime
        {
            get
            {
                return confirmationTime;
            }
            set
            {
                confirmationTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        private DateTime? cancellationTime = null;
        public DateTime? CancellationTime
        {
            get
            {
                return cancellationTime;
            }
            set
            {
                cancellationTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }
    }

    public class UserMessageEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        // PartitionKey extId
        // RowKey String.Empty
        public string Text { get; set; } // Max 64kB
    }

    public class ExternalSessionLogEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        // PartitionKey KeyUtils.GetCurrentTimeKey()
        // RowKey {"Request" | "Response"}
        public string Data { get; set; } // Max 64kB
        public int HttpStatus { get; set; } // HTTP status
    }


}