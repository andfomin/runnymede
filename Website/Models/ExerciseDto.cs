using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Runnymede.Website.Models
{
    public class ExerciseDto
    {
        public ExerciseDto()
        {
            Reviews = new List<ReviewDto>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreateTime { get; set; }
        public string TypeId { get; set; }
        public string ArtefactId { get; set; }
        public string Title { get; set; }
        public int? Length { get; set; }

        public virtual IEnumerable<ReviewDto> Reviews { get; set; }
    }
}