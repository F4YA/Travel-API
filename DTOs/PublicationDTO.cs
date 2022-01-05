using System;
using System.Collections.Generic;
using API.Models;

namespace API.DTOs
{
    public class PublicationDto
    {
        public UserDto Author { get; set; }

        #nullable enable
        public string? Id { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? Locality { get; set; }
        public string? Rote{get; set;}
        public string? Business { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public IEnumerable<string>? Images { get; set; }
        public PublicationDto? Complement { get; set; }
        public RateDto? Evaluation { get; set; }
        public int? TotalComments { get; set; }
        public int? TotalFiveStars { get; set; }
        public int? TotalFourStars { get; set; }
        public int? TotalThreeStars { get; set; }
        public int? TotalTwoStars { get; set; }
        public int? TotalOneStars { get; set; }
    }
}