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

        public static ItalkiTeacher[] ItalkiTeachers = new ItalkiTeacher[]  {
#if DEBUG
            new ItalkiTeacher {
                UserId = 1449340898,
                DisplayName = "John Smith",
                ItalkiUserId = 1149255, // John
                CourseId = 24905,
                ScheduleUrl = "https://secure.italki.com/sessions/request.htm?cid=24905&stype=1&tlen=4",
                Rate = 1.0m,
                TestFilePath = @"C:\Users\Andrey\Desktop\ex01_John.html",
            },
            new ItalkiTeacher
            {
                UserId = 1583803389,
                DisplayName = "test_jude",
                ItalkiUserId = 1179253, // Jude
                CourseId = 29669,
                ScheduleUrl = "https://secure.italki.com/sessions/request.htm?cid=29669&stype=1&tlen=4",
                Rate = 2.0m,
                TestFilePath =  @"C:\Users\Andrey\Desktop\ex01_Jude.html",
            },
#else
            new ItalkiTeacher
            {
                UserId = 2024063771,
                DisplayName = "Tom",
                ItalkiUserId = 658229, // Tom Emery
                CourseId = 12030,
                ScheduleUrl = "https://secure.italki.com/sessions/request.htm?cid=12030&stype=1&tlen=4",
                Rate = 17.0m,
            },
#endif
        };

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

            // italki allows the user to book a session which starts at least in 24 hours. Round up to ItalkiTimeSlotDuration minutes;
            long slotTicks = ItalkiHelper.ItalkiTimeSlotDuration.Ticks;
            long ticks = (DateTime.UtcNow.Ticks + slotTicks - 1) / slotTicks;
            var threshold = new DateTime(ticks * slotTicks).AddDays(1);

            var allPeriods = teacherPeriodsArray
                // Flatten all lists
                .SelectMany(i => i)
                // Filter by parameter dates
                .Where(i => (i.Start < endDate) && (i.End > startDate) && (i.End > threshold + SessionDuration))
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

            //var periodsByRate = teacherPeriodsArray
            //    // Flatten all periods
            //    .SelectMany(i => i)
            //    // Filter by parameter dates
            //    .Where(i => (i.Start <= endDate) && (i.End >= startDate))
            //    // Group by rate. 
            //    .GroupBy(i => i.Rate)
            //    // Merge periods with similar rates from all teachers. We show periods for all teachers in the week view.
            //    .Select(i => {
            //        periodCollection.Clear();
            //        periodCollection.AddAll(i.Select(j => j));
            //        return periodCombiner.CombinePeriods(periodCollection).Select(j => new SessionDto { Start = j.Start, End = j.End, Price = i.Key, });
            //    })
            //    .SelectMany(i => i)
            //    ;

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
                    var slotCount = (int)Math.Floor((i.End - SessionDuration - i.Start).TotalMinutes / ItalkiHelper.ItalkiTimeSlotDuration.TotalMinutes);
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
        public static async Task<IEnumerable<TeacherTimeRange>> GetTeacherPeriods(int userId, bool forceRealTime)
        {
            // First look up in the cache. If the cached data expired, request a fresh one.
            var key = userId.ToString();

            if (forceRealTime)
            {
                ItalkiSchedulesCache.Remove(key);
            }

            var periods = ItalkiSchedulesCache.Get(key) as IEnumerable<TeacherTimeRange>;
            if (periods == null)
            {
                // Awoid duplicate requests to italki. Block asynchronously.
                var semaphore = Semaphores.GetOrAdd(userId, (i) => new SemaphoreSlim(1, 1));
                await semaphore.WaitAsync();
                try
                {
                    periods = ItalkiSchedulesCache.Get(key) as IEnumerable<TeacherTimeRange>;
                    if (periods == null)
                    {
                        var helper = new ItalkiHelper();
#if DEBUG
                        var html = File.ReadAllText(ItalkiTeachers.First(i => i.UserId == userId).TestFilePath);
#else
                        var html = await helper.LoadPage(teacher.ScheduleUrl);
#endif
                        var vacantPeriods = helper.GetVacantPeriods(html);

                        periods = vacantPeriods
                            // There may be 30-minute "left over" slots available. We filter them out. Our sessions are 60 minutes.
                             .Where(i => i.Duration >= SessionDuration)
                             .Select(i => new TeacherTimeRange
                             {
                                 Start = i.Start,
                                 End = i.End,
                                 UserId = userId,
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
            long slotTicks = ItalkiHelper.ItalkiTimeSlotDuration.Ticks;
            long ticks = (DateTime.UtcNow.Ticks + slotTicks - 1) / slotTicks;
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
