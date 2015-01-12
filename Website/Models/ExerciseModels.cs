using Runnymede.Common.Utils;
using Runnymede.Website.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Runnymede.Website.Models
{

    // Corresponds to dbo.appTypes ('EX....') and app.exercises.ExerciseType (in app/shared/exercises.ts) 
    public static class ExerciseType
    {
        public const string AudioRecording = "EXAREC";
        public const string WritingPhoto = "EXWRPH";
    }

    public class ExerciseDto
    {

        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public string Artifact { get; set; }
        public string Title { get; set; }
        public int? Length { get; set; }
        public virtual IEnumerable<ReviewDto> Reviews { get; set; }

        private DateTime? _createTime;
        public DateTime? CreateTime
        {
            get
            {
                return _createTime;
            }
            set
            {
                _createTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

    } // end of class ExerciseDto

    public class ReviewDto
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public int? UserId { get; set; }
        public decimal? Price { get; set; }
        public string AuthorUserId { get; set; }
        public string AuthorName { get; set; }
        public string ReviewerName { get; set; }
        public int? ExerciseLength { get; set; }

        private DateTime? _requestTime;
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

        private DateTime? _cancelationTime;
        public DateTime? CancelationTime
        {
            get
            {
                return _cancelationTime;
            }
            set
            {
                _cancelationTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        private DateTime? _startTime;
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

        private DateTime? _finishTime;
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


    // Corresponds top app.reviews.IReviewPiece
    public class ReviewPiece : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {
        /* PartitionKey = Reversed ExerciseId, not padded. We reverse the number to distribute the load on the Azure Table randomly. 
         * Values for pieces come from app.reviews.Editor.getPartitionKey(). Values for user access entries come from ReviewPiece.GetPartitionKey() 
         */
        /* RowKey = 
         *   ReviewId pre-padded up to 10 digits 
         * + item type ("R" remark, "S" suggestion, "C" comment, "E" editor user, "V" viewer user) 
         * + (Piece.Id OR UserId) prepadded up to 10 digits
         * For example: 0000000038V0000000006
         * Values for pieces come from app.reviews.Editor.getRowKey(). Values for user access entries come from ReviewPiece.GetRowKey()
         */
        public string Json { get; set; } // JSON

        public class PieceTypes
        {
            // Remark = "R". Suggestion = "S". Comment = "C". Corresponds to app.reviews.PieceTypes .
            public const string Editor = "E";
            public const string Viewer = "V";
            public const string Remark = "R";
        }

        public static string GetPartitionKey(int exerciseId)
        {
            // Corresponds to app.reviews.Editor.getPartitionKey()
            return KeyUtils.IntToKey(exerciseId);
        }

        public static string GetRowKey(int reviewId, string pieceType, int id)
        {
            // Corresponds to app.reviews.Editor.getRowKey()
            return KeyUtils.IntToKey(reviewId) + pieceType + KeyUtils.IntToKey(id);
        }

        public static string GetType(string rowKey)
        {
            return rowKey.Substring(10, 1);
        }

        public static string GetReviewId(string rowKey)
        {
            return rowKey.Substring(0, 10);
        }

        public static string GetUserAccessCode(string rowKey)
        {
            return rowKey.Substring(10, 11);
        }

    }

    public class RemarkSpot
    {
        public int ReviewId { get; set; }
        public string Type { get; set; }
        public int Id { get; set; } 
        public int Start { get; set; }
        public int Finish { get; set; }
    }

} // end of namespace