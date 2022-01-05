using System;

namespace API.Models
{
    public class Reply
    {
        #nullable enable
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? PubId { get; set; }
        public string? CommentId { get; set; }
        public DateTime? CommentedAt { get; set; }
        public string? Content { get; set; }
        #nullable disable
    }
}