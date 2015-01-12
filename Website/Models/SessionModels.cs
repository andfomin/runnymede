using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{

    public class ScheduleEventDto
    {
        public string Type { get; set; }
        public int UserId { get; set; }

        //<fc> These properties correspond to the ones in FullCalendar.
        public int Id { get; set; }

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

    public class SessionUserDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Announcement { get; set; }
        public decimal? SessionRate { get; set; }
        public IEnumerable<ScheduleEventDto> ScheduleEvents { get; set; }
    }

    public class UserMessageEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        // PartitionKey extId
        // RowKey String.Empty
        public string Text { get; set; } // Max 64kB
    }




}