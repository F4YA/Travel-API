using System;
using System.Collections.Generic;

namespace API.Models
{
    public class Publication
    {
        #nullable enable
        public string? Id { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? Locality { get; set; }
        public string? Rote{get; set;}
        public string? Business { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string[]? Images { get; set; }
        public Publication? Complement { get; set; }

        #nullable disable

    }
}