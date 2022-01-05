using System.Collections.Generic;

namespace API.DTOs
{
    public class RateDto
    {
        #nullable enable
        public decimal? TotalRate { get; set; }
        public int? EvaluationsCount { get; set; }
        public IEnumerable<int>? Rates { get; set; }
    }
}