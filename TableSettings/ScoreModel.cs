using System;

namespace TableSettings
{
    public class ScoreModel
    {
        public int Id { get; set; }
        public string NumScore { get; set; }
        public float Cash { get; set; }
        public Guid ClientId { get; set; }
        public bool Exist { get; set; }
    }
}