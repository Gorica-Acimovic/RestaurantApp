using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.DTOs;
using Restaurant.Api.Infrastructure;
using Restaurant.Api.Interfaces;
using System.Security.Claims;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ApiControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }


        [HttpPost]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> CreateReview(CreateReviewRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var (review, error, statusCode) = await _reviewService.CreateReviewAsync(request, userId);
            if (error is not null)
                return Error(statusCode, error);

            return Ok(review);
        }

        [HttpGet("summary")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSummary([FromQuery] int take = 5)
        {
            take = Math.Clamp(take, 1, 20);
            var summary = await _reviewService.GetSummaryAsync(take);
            return Ok(summary);
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetReviews([FromQuery] int? rating, [FromQuery] bool unanswered = false)
        {
            var reviews = await _reviewService.GetReviewsAsync(rating, unanswered);
            return Ok(reviews);
        }

        [HttpPut("{id}/response")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> RespondToReview(int id, RespondToReviewRequest request)
        {
            var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var (review, error, statusCode) = await _reviewService.RespondAsync(id, request, managerId);
            if (error is not null)
                    return Error(statusCode, error);

            return Ok(review);
        }
    }
}

