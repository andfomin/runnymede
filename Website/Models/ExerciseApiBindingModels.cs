using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Runnymede.Website.Models
{
    public class ExerciseSaveTopicModel
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string[] Lines { get; set; }
    }

}