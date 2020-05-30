using System;

namespace TableSettings
{
    public class MessageModel
    {
                public int Id { get; set; }
                public Guid ClientId { get; set; }
                public Guid AuthorId { get; set; }
                public DateTime SentTime { get; set; }
                public string Message { get; set; }
    }
}