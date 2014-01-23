namespace Runnymede.Website.Models
{
    public class RemarkDto
    {
        public RemarkDto()
        {
        }
        
        public int ReviewId { get; set; } // PartitionKey. Originally string. Use ToString("D10")        
        public string Id { get; set; } // RowKey
        public int Start { get; set; }
        public int Finish { get; set; }
        public string Tags { get; set; }
        public string Text { get; set; }
        public bool Starred { get; set; }
    }
}