using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{
    public class SearchResult
    {       
        public string Domain { get; set; }
        public string Url { get; set; }
        public int UserBoost { get; set; }
        public int GlobalBoost { get; set; }
        public int OriginalPosition { get; set; }
    }

    public class PostedQueryModel
    {
        public string Q { get; set; }
    }

}