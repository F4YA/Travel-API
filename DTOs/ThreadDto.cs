namespace API.DTOs
{
    public class ThreadDto
    {
        public PublicationDto Publication { get; set; }
        public decimal? TotalRate { get; set; }
        public int? TotalAvaliations { get; set; }
        public int? TotalComments { get; set; }
        public int? TotalFiveStars { get; set; }
        public int? TotalFourStars { get; set; }
        public int? TotalThreeStars { get; set; }
        public int? TotalTwoStars { get; set; }
        public int? TotalOneStars { get; set; }
    }
}