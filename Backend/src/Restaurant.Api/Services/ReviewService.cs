using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;

namespace Restaurant.Api.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;
        private readonly TimeProvider _timeProvider;

        public ReviewService(AppDbContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<(ReviewDto? Review, string? Error, int StatusCode)> CreateReviewAsync(CreateReviewRequest request, int userId)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == request.ReservationId);

            if (reservation == null)
            {
                return (null, "Reservation not found.", 404);
            }

            if (reservation.UserId != userId)
            {
                return (null, "You can only review your own reservations.", 403);
            }

            if (reservation.Status != ReservationStatus.Completed)
            {
                return (null, "Only completed reservations can be reviewed.", 400);
            }

            var alreadyReviewed = await _context.Reviews.AnyAsync(r => r.ReservationId == reservation.Id);
            if (alreadyReviewed)
            {
                return (null, "This reservation already has a review.", 409);
            }

            var review = new Review
            {
                ReservationId = reservation.Id,
                Rating = request.Rating,
                Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
                CreatedAt = _timeProvider.GetLocalNow().DateTime
            };

            _context.Reviews.Add(review);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                return (null, "This reservation already has a review.", 409);
            }

            review.Reservation = reservation;
            return (ToDto(review), null, 200);
        }

        public async Task<ReviewSummaryDto> GetSummaryAsync(int take)
        {
            var count = await _context.Reviews.CountAsync();
            var average = await _context.Reviews.AverageAsync(r => (double?)r.Rating);

            var latest = await _context.Reviews
                .Include(r => r.Reservation).ThenInclude(x => x.User)
                .Include(r => r.Responder)
                .OrderByDescending(r => r.CreatedAt)
                .Take(take)
                .ToListAsync();

            return new ReviewSummaryDto
            {
                AverageRating = Math.Round(average ?? 0, 1),
                ReviewCount = count,
                LatestReviews = latest.Select(ToDto).ToList()
            };
        }

        public async Task<List<ReviewDto>> GetReviewsAsync(int? rating, bool unansweredOnly)
        {
            var query = _context.Reviews
                .Include(r => r.Reservation).ThenInclude(x => x.User)
                .Include(r => r.Responder)
                .AsQueryable();

            if (rating.HasValue)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }

            if (unansweredOnly)
            {
                query = query.Where(r => r.ManagerResponse == null);
            }

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(ToDto).ToList();
        }

        public async Task<(ReviewDto? Review, string? Error, int StatusCode)> RespondAsync(int id, RespondToReviewRequest request, int managerId)
        {
            var review = await _context.Reviews
                .Include(r => r.Reservation).ThenInclude(x => x.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return (null, "Review not found.", 404);
            }

            if (review.ManagerResponse != null)
            {
                return (null, "Review already has a response.", 409);
            }

            try
            {
                _context.Entry(review).Property(r => r.RowVersion).OriginalValue = Convert.FromBase64String(request.RowVersion);
            }
            catch (FormatException)
            {
                return (null, "Invalid row version.", 400);
            }

            var manager = await _context.Users.FirstOrDefaultAsync(u => u.Id == managerId);

            review.ManagerResponse = request.Response.Trim();
            review.RespondedAt = _timeProvider.GetLocalNow().DateTime;
            review.ResponderId = managerId;
            review.Responder = manager;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return (null, "Review was changed by someone else. Reload and try again.", 409);
            }

            return (ToDto(review), null, 200);
        }

        private static ReviewDto ToDto(Review review)
        {
            return new ReviewDto
            {
                Id = review.Id,
                ReservationId = review.ReservationId,
                GuestName = GetDisplayName(review.Reservation.User),
                VisitDate = review.Reservation.StartsAt,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                ManagerResponse = review.ManagerResponse,
                RespondedAt = review.RespondedAt,
                ResponderName = review.Responder == null ? null : $"{review.Responder.FirstName} {review.Responder.LastName}",
                RowVersion = Convert.ToBase64String(review.RowVersion)
            };
        }

        private static string GetDisplayName(User? user)
        {
            if (user == null)
            {
                return "Guest";
            }

            var initial = user.LastName.Length > 0 ? $" {user.LastName[0]}." : string.Empty;
            return $"{user.FirstName}{initial}";
        }
    }
}