using Itenso.TimePeriod;
using Runnymede.Common.Models;
using Runnymede.Common.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Runnymede.Website.Utils
{
    public class SessionHelper
    {

        public static TimeSpan SessionDuration = TimeSpan.FromMinutes(60); // minutes;
        private static TimeSpan CacheDuration = TimeSpan.FromMinutes(5); // minutes

//        public static ItalkiTeacher[] ItalkiTeachers0 = new ItalkiTeacher[]  {
//#if DEBUG
            //new ItalkiTeacher
            //{
            //    UserId = 1449340898,
            //    DisplayName = "01 test Tom",
            //    ItalkiUserId = 658229, // Tom Emery
            //    CourseId = 12030,
            //    ScheduleUrl = "https://secure.italki.com/sessions/request.htm?cid=12030&stype=1&tlen=4",  
            //    Rate = 17.0m,
            //},
            //new ItalkiTeacher
            //{
            //    UserId = 1282608589,
            //    DisplayName = "03 test Neil",
            //    ItalkiUserId = 1753811, // 
            //    CourseId = 32836,
            //    ScheduleUrl = "https://secure.italki.com/sessions/request.htm?cid=32836&stype=1&tlen=4",  
            //    Rate = 17.0m,
            //},
//#else
//            new ItalkiTeacher
//            {
//                UserId = 2024063771,
//                DisplayName = "Tom",
//                ItalkiUserId = 658229, // Tom Emery
//                CourseId = 12030,
//                ScheduleUrl = "https://secure.italki.com/sessions/request.htm?cid=12030&stype=1&tlen=4",
//                Rate = 17.0m,
//            },
//#endif
//        };

        private static Lazy<IEnumerable<ItalkiTeacher>> lazyItalkiTeachers = new Lazy<IEnumerable<ItalkiTeacher>>(InitItalkiTeachers, true);

        public static IEnumerable<ItalkiTeacher> ItalkiTeachers
        {
            get
            {
                return lazyItalkiTeachers.Value;
            }
        }

        private static IEnumerable<ItalkiTeacher> InitItalkiTeachers()
        {
            var sql = @"
select UserId, DisplayName, ItalkiUserId, CourseId, ScheduleUrl, Rate 
from dbo.sesGetItalkiTeachers();
";
            return DapperHelper.QueryResiliently<ItalkiTeacher>(sql);
        }

        private static Lazy<MemoryCache> lazyItalkiSchedulesCache = new Lazy<MemoryCache>(() => new MemoryCache("ItalkiSchedules"), true);

        private static MemoryCache ItalkiSchedulesCache
        {
            get
            {
                return lazyItalkiSchedulesCache.Value;
            }
        }

        private static ConcurrentDictionary<int, SemaphoreSlim> Semaphores = new ConcurrentDictionary<int, SemaphoreSlim>();

        /// <summary>
        /// Get vacant time slots from Italki, shredded as sessions
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<SessionDto>> GetOfferedSchedules(DateTime startDate, DateTime endDate, bool forceRealTime)
        {
            // Load all the HTML pages simultaneously.
            var tasks = ItalkiTeachers.Select(i => GetTeacherPeriods(i.UserId, forceRealTime));
            var teacherPeriodsArray = await Task.WhenAll(tasks);

            // italki does not allow booking in less than 24 hour.
            var threshold = GetBookingTimeThreshold();

            var allPeriods = teacherPeriodsArray
                // Flatten all lists
                .SelectMany(i => i)
                // Filter 
                .Where(i =>
                    (i.Start < endDate) &&
                    (i.End > startDate) &&
                        // There may be 30-minute "left over" slots available. We filter them out. Our sessions are 60 minutes.
                    (i.Duration >= SessionDuration) &&
                    (i.End >= threshold + SessionDuration)
                    )
                ;

            // Merge all periods for display.
            ITimePeriodCollection periodCollection = new TimePeriodCollection();
            var periodCombiner = new TimePeriodCombiner<TeacherTimeRange>();
            periodCollection.AddAll(allPeriods);
            periodCollection = periodCombiner.CombinePeriods(periodCollection);

            var offeredSessions = periodCollection
                .Select(i => new SessionDto
                {
                    Start = i.Start >= threshold ? i.Start : threshold,
                    End = i.End,
                });

            return offeredSessions;
        }

        public static async Task<IEnumerable<SessionDto>> GetOfferedSessions(DateTime startDate, DateTime endDate)
        {
            // Load all the HTML pages simultaneously.
            var tasks = ItalkiTeachers.Select(i => GetTeacherPeriods(i.UserId, false));
            var teacherPeriodsArray = await Task.WhenAll(tasks);

            var threshold = GetBookingTimeThreshold();

            var sessions = teacherPeriodsArray
                .SelectMany(i => i.Where(j => (j.Start < endDate) && (j.End > startDate)))
                // Shred every period to slots.
                .SelectMany(i =>
                {
                    var slotCount = (int)Math.Floor((i.End - SessionDuration - i.Start).TotalMinutes / ItalkiHelper.ItalkiTimeSlotDuration.TotalMinutes) + 1;
                    return Enumerable.Range(0, slotCount)
                        .Select(j =>
                        {
                            var start = i.Start.AddMinutes(j * ItalkiHelper.ItalkiTimeSlotDuration.TotalMinutes);
                            return new SessionDto
                            {
                                Start = start,
                                End = start + SessionDuration,
                                TeacherUserId = i.UserId,
                                Price = i.Rate * (decimal)SessionDuration.TotalHours,
                            };
                        });
                })
                .Where(i => (i.Start < endDate) && (i.End > startDate) && (i.Start >= threshold))
                ;
            return sessions;
        }

        /// <summary>
        /// Get vacant time slots from Italki. Cache results for some time.
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<TeacherTimeRange>> GetTeacherPeriods(int teacherUserId, bool forceRealTime)
        {
            // Validate the teacherUserId;
            var teacher = ItalkiTeachers.First(i => i.UserId == teacherUserId);         

            // First look up in the cache. If the cached data expired, request a fresh one.
            var key = teacherUserId.ToString();

            if (forceRealTime)
            {
                ItalkiSchedulesCache.Remove(key);
            }

            var periods = ItalkiSchedulesCache.Get(key) as IEnumerable<TeacherTimeRange>;

            if (periods == null)
            {
                // Awoid duplicate requests to italki. Block asynchronously.
                var semaphore = Semaphores.GetOrAdd(teacherUserId, (i) => new SemaphoreSlim(1, 1));
                await semaphore.WaitAsync();
                try
                {
                    periods = ItalkiSchedulesCache.Get(key) as IEnumerable<TeacherTimeRange>;

                    if (periods == null)
                    {
                        var helper = new ItalkiHelper();
#if DEBUG
                        var html = File.ReadAllText(String.Format(@"C:\Users\Andrey\Desktop\italki{0}.html", teacher.CourseId));
                        //var html = await helper.LoadPage(teacher.ScheduleUrl);
#else
                        var html = await helper.LoadPage(teacher.ScheduleUrl);
#endif
                        var vacantPeriods = helper.GetVacantPeriods(html);

                        periods = vacantPeriods
                             .Select(i => new TeacherTimeRange
                             {
                                 Start = i.Start,
                                 End = i.End,
                                 UserId = teacherUserId,
                             })
                             ;

                        // Avoid all pages to expire at the same moment. Add random delay from 0 upto 2 minutes.
                        var random = TimeSpan.FromMinutes(new Random(Guid.NewGuid().GetHashCode()).NextDouble() * 2);
                        CacheItemPolicy cacheItemPolicy = new CacheItemPolicy()
                        {
                            AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.Add(CacheDuration + random))
                        };
                        ItalkiSchedulesCache.Set(key, periods, cacheItemPolicy);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
            return periods;
        }

        private static DateTime GetBookingTimeThreshold()
        {
            // italki allows the user to book a session which starts at least in 24 hours. Round up to ItalkiTimeSlotDuration minutes;
            // Add an extra minute to avoid rejected requests from slaggish users.
            var now = DateTime.UtcNow.AddMinutes(1); // new DateTime(2015, 07, 21, 16, 55, 00, DateTimeKind.Utc);
            long slotTicks = ItalkiHelper.ItalkiTimeSlotDuration.Ticks;
            long ticks = (now.Ticks + slotTicks - 1) / slotTicks;
            var threshold = new DateTime(ticks * slotTicks).AddDays(1);
            return threshold;
        }

        public static async Task<string> SaveMessageAndGetExtId(string message)
        {
            string extId = null;
            // We cannot use a transaction in heterogeneous storage mediums. An orphaned message on Azure Table is not a problem. The main message metadata is in the database anyway.
            if (!String.IsNullOrWhiteSpace(message))
            {
                extId = KeyUtils.GetTwelveBase32Digits();
                var entity = new UserMessageEntity
                {
                    PartitionKey = extId,
                    RowKey = String.Empty,
                    Text = message,
                };
                await AzureStorageUtils.InsertEntityAsync(AzureStorageUtils.TableNames.UserMessages, entity);
            }
            return extId;
        }

        public static void ConvertFullCalendarDates(string start, string end, string localTime, int localTimezoneOffset, out DateTime startDate, out DateTime endDate)
        {
            /* FullCalendar passes Start and End as midnights without a timezone. 
             * In other words, for clients in different time zones, it passes the same values indicating only the calendar date, but not the exact midnight local time.
             * We use the client-side TimezoneOffset to calculate times */
            TimeUtils.ThrowIfClientTimeIsAmbiguous(localTime, localTimezoneOffset);

            DateTime startDay, endDay;
            if ((!DateTime.TryParse(start, null, DateTimeStyles.RoundtripKind, out startDay))
                ||
                (!DateTime.TryParse(end, null, DateTimeStyles.RoundtripKind, out endDay)))
            {
                throw new ArgumentException("Date is wrong.");
            }

            startDate = startDay.AddMinutes(localTimezoneOffset);
            endDate = endDay.AddMinutes(localTimezoneOffset);
        }


    }
}
