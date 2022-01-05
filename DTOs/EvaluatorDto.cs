using API.Models;

namespace API.DTOs
{
    public class EvaluatorDto
    {
        #nullable enable
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? ProfileImageUrl { get; set; }
        public Evaluation? Evaluation { get; set; }
    }
}