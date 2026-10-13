namespace Restaurant.Api.DTOs
{
    public class ReviewSummaryDto
    {
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<ReviewDto> LatestReviews { get; set; } = new();
    }
}
