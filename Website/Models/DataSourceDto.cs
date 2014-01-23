using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{
    public class DataSourceDto<T>
    {
        public int TotalCount { get; set; }
        public IEnumerable<T> Items { get; set; }
    }

    //public class DataSourceDto2<T>
    //{
    //    public int Count { get; set; }
    //    public IEnumerable<T> Results { get; set; }
    //}
}