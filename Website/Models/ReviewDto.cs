using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Runnymede.Website.Models
{
    public class ReviewDto
    {
        private DateTime? _requestTime = null;
        private DateTime? _cancelTime = null;
        private DateTime? _startTime = null;
        private DateTime? _finishTime = null;

        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public int? UserId { get; set; }
        public decimal? Reward { get; set; }
        public string AuthorName { get; set; }
        public string ReviewerName { get; set; }

        public DateTime? RequestTime
        {
            get
            {
                return _requestTime;
            }
            set
            {
                _requestTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        public DateTime? CancelTime
        {
            get
            {
                return _cancelTime;
            }
            set
            {
                _cancelTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        public DateTime? StartTime
        {
            get
            {
                return _startTime;
            }
            set
            {
                _startTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        public DateTime? FinishTime
        {
            get
            {
                return _finishTime;
            }
            set
            {
                _finishTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }


    }
}