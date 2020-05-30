using System;

namespace TableSettings
{
    public class TemplateModel
    {
        public int Id  { get; set; }
        public string TemplateName { get; set; }
        public Guid ClientId { get; set; }
        public string ScoreFrom { get; set; }
        public string ScoreTo { get; set; }
        public float HowMuch { get; set; }
        public string TakerName { get; set; }
    }
}