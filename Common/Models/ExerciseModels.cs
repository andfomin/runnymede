using Runnymede.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Runnymede.Common.Models
{

    // Corresponds to dbo.appTypes ('AR....') and app.ArtifactType (in app/shared/utils.ts) 
    public static class ArtifactType
    {
        public const string Mp3 = "ARMP3_";
        public const string Jpeg = "ARJPEG";
    }

    // Corresponds to dbo.appTypes
    public static class ServiceType
    {

        public class Properties
        {
            public string Title { get; set; }
            public string AnswerSheetFileUrl { get; set; }
            public string AnswerSheetImageUrl { get; set; }

            public Properties(string title, string answerSheetFileUrl, string answerSheetImageUrl)
            {
                Title = title;
                AnswerSheetFileUrl = answerSheetFileUrl;
                AnswerSheetImageUrl = answerSheetImageUrl;
            }
        }

        public const string IeltsWritingTask1 = "SVRIW1";
        public const string IeltsWritingTask2 = "SVRIW2";
        public const string IeltsSpeaking = "SVRIS_";
        public const string IeltsReading = "SVRIR_";
        public const string IeltsListening = "SVRIL_";

        public static string GetTitle(string serviceType)
        {
            switch (serviceType)
            {
                case IeltsWritingTask1:
                    return "Writing Task 1";
                case IeltsWritingTask2:
                    return "Writing Task 2";
                case IeltsSpeaking:
                    return "Speaking";
                case IeltsReading:
                    return "Reading";
                case IeltsListening:
                    return "Listening";
                default:
                    return null;
            }
        }
    }

    // Corresponds to dbo.appTypes
    public static class CardType
    {
        public const string IeltsSpeaking = "CDIS__";
        public const string IeltsWritingTask1Academic = "CDIW1A";
        public const string IeltsWritingTask1General = "CDIW1G";
        public const string IeltsWritingTask2 = "CDIW2_";

        public static string GetCardType(string serviceType, string cardType)
        {
            switch (serviceType)
            {
                case ServiceType.IeltsSpeaking:
                    return CardType.IeltsSpeaking;
                case ServiceType.IeltsWritingTask1:
                    return new string[] { CardType.IeltsWritingTask1Academic, CardType.IeltsWritingTask1General }.FirstOrDefault(i => i == cardType);
                case ServiceType.IeltsWritingTask2:
                    return CardType.IeltsWritingTask2;
                default:
                    return null;
            }
        }
    }

    public class ExerciseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ServiceType { get; set; }
        public string ArtifactType { get; set; }
        public string Artifact { get; set; }
        public string Title { get; set; }
        public int? Length { get; set; }
        public virtual IEnumerable<ReviewDto> Reviews { get; set; }
        public Guid? CardId { get; set; }
        public string Comment { get; set; }

        private DateTime? creationTime;
        public DateTime? CreationTime
        {
            get
            {
                return creationTime;
            }
            set
            {
                creationTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

    } // end of class ExerciseDto

    public class ReviewDto
    {
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        public string ServiceType { get; set; } // No column in the table. Used on the client. Joined on the fly.
        public string ArtifactType { get; set; } // No column in the table. Used on the client. Joined on the fly.
        public string Title { get; set; } // No column in the table. Used on the client. Joined on the fly.
        //public int? ExerciseLength { get; set; } // No column in the table. Used on the client. Joined on the fly.
        public int? UserId { get; set; }
        public string ReviewerName { get; set; } // No column in the table. Used on the client. Joined on the fly.

        private DateTime? requestTime;
        public DateTime? RequestTime
        {
            get
            {
                return requestTime;
            }
            set
            {
                requestTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        private DateTime? startTime;
        public DateTime? StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                startTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
            }
        }

        private DateTime? finishTime;
        public DateTime? FinishTime
        {
            get
            {
                return finishTime;
            }
            set
            {
                finishTime = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
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
            // Corresponds to app.reviews.PieceTypes in app/shared/exercises.ts .
            public const string Editor = "E";
            public const string Viewer = "V";
            public const string Remark = "R";
            //public const string Performance = "P";
            //public const string Suggestion = "S";
            //public const string Comment = "C";
            //public const string Video = "Y";
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

        public static int GetReviewId(string rowKey)
        {
            return Convert.ToInt32(rowKey.Substring(0, 10));
        }

        public static string GetType(string rowKey)
        {
            return rowKey.Substring(10, 1);
        }

        public static int GetPieceId(string rowKey)
        {
            return Convert.ToInt32(rowKey.Substring(11, 10));
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

    public class CardDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public IEnumerable<CardItemDto> Items { get; set; }
    }

    public class CardItemDto
    {
        //public int Id { get; set; }
        public Guid CardId { get; set; }
        public string Position { get; set; }
        public string Contents { get; set; }
    }


} // end of namespace