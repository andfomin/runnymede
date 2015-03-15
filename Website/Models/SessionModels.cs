using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{

    public class FullCalendarEventDto
    {
        //<fc> These properties correspond to the ones in FullCalendar.
        public int? Id { get; set; }

        private DateTime? start = null;
        public DateTime? Start
        {
            get
            {
                return start;
            }
            set
            {
                start = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        private DateTime? end = null;
        public DateTime? End
        {
            get
            {
                return end;
            }
            set
            {
                end = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }
        //</fc>
    }

    public class ScheduleEventDto : FullCalendarEventDto
    {
        public string Type { get; set; }
        public int UserId { get; set; }
    }
    // TODO: Depricated ?
    public class ScheduleEventGroupDto : FullCalendarEventDto
    {
        public decimal MinPrice { get; set; }
    }

    public class SessionDto : FullCalendarEventDto
    {
        public int ProposedTeacherUserId { get; set; }
        public int TeacherUserId { get; set; }
        public int LearnerUserId { get; set; }
        public decimal Cost { get; set; }
        public decimal Price { get; set; }
        public byte Rating { get; set; }

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
    }

    // TODO: Depricated ?
    public class SessionUserDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Announcement { get; set; }
        public IEnumerable<ScheduleEventDto> ScheduleEvents { get; set; }
    }

    public class UserMessageEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        // PartitionKey extId
        // RowKey String.Empty
        public string Text { get; set; } // Max 64kB
    }




}