using System;
using System.Collections.Generic;

namespace API.DTOs.Responses
{
    public class CommentResponse
    {
        #nullable enable
        public string? UserId { get; set; }
        public string? Name { get; set; }
        public string? ProfileImageUrl { get; set; }
        public int? Rate { get; set; }     
        public string? Id{ get; set; }
        public DateTime? CommentedAt {get; set;}
        public string? Content { get; set; }
        public int UpVotes { get; set; }
        public int DownVotes { get; set; }
        public IEnumerable<CommentResponse>? Replies { get; set; }
    }
}