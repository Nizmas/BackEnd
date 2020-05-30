using System;

namespace TableSettings
{
    public class HistoryModel
    {
        public int Id { get; set; }
        public string ScoreFrom { get; set; }
        public string ScoreTo { get; set; }
        public DateTime SentTime { get; set; }
        public float HowMuch { get; set; }
        public bool Template { get; set; } 
        public Guid ClientId { get; set; }
        public Guid TakerId { get; set; }
    }
}