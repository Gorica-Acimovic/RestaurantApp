using Restaurant.Api.DTOs;

namespace Restaurant.Api.Interfaces
{
    public interface IReviewService
    {
        Task<(ReviewDto? Review, string? Error, int StatusCode)> CreateReviewAsync(CreateReviewRequest request, int userId);
        Task<ReviewSummaryDto> GetSummaryAsync(int take);
        Task<List<ReviewDto>> GetReviewsAsync(int? rating, bool unansweredOnly);
        Task<(ReviewDto? Review, string? Error, int StatusCode)> RespondAsync(int id, RespondToReviewRequest request, int managerId);
    }
}
