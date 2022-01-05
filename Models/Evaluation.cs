using System;

namespace API.Models
{
    public class Evaluation
    {
        public string UserId { get; set; }
        public string PubId { get; set; }
        
        #nullable enable
        public DateTime? EvaluatedAt { get; set; }
        #nullable disable
        public int Rate { get; set; }
    }
}