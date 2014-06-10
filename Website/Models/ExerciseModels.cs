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
    } // end of class ExerciseDto

    public class ReviewDto
    {
        public ReviewDto()
        {
            Suggestions = new List<SuggestionDto>();
        }

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
        public int? ExerciseLength { get; set; }
        public string Comment { get; set; }
        public virtual IEnumerable<SuggestionDto> Suggestions { get; set; }

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
    } // end of class ReviewDto

    public class RemarkDto
    {
        public RemarkDto()
        {
        }

        // Initially remarks were stored in Azure Table Storage. Then they migrated to SQL Database.
        ////public int ReviewId { get; set; } // PartitionKey. Originally string. Use ToString("D10")        
        ////public string Id { get; set; } // RowKey

        public int ReviewId { get; set; }
        public int CreationTime { get; set; } // Comes from the client. Means milliseconds passed from the start of the review.
        public int Start { get; set; }
        public int Finish { get; set; }
        public string Text { get; set; }
        public string Keywords { get; set; }
    } // end of class RemarkDto

    // Todo. Eliminate RemarkEntity.
    public class RemarkEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        public RemarkEntity()
        {
        }

        // string PartitionKey = int ReviewId // Originally int formatted as ToString("D10") using AzureStorageUtils.IntToKey
        // string RowKey = string Id // Six Base36 digits pre-padded with zeros.
        public int Start { get; set; }
        public int Finish { get; set; }
        public string Tags { get; set; } // May hold many tags separated by comma.
        public string Text { get; set; }
        public bool Starred { get; set; }
    }

    public class SuggestionDto
    {
        public SuggestionDto()
        {
        }

        public int ReviewId { get; set; }
        public int CreationTime { get; set; } // Comes from the client. Means milliseconds passed from the start of the review.
        public string Text { get; set; }
    } // end of class SuggestionDto



} // end of namespace