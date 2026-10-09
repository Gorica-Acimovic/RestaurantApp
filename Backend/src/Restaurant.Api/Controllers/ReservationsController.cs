using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Api.DTOs;
using Restaurant.Api.Interfaces;
using System.Security.Claims;

namespace Restaurant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }


        [HttpPost]
        [AllowAnonymous]

        public async Task<IActionResult> MakeReservation(ReservationRequest request)
        {

            int? userId = null;
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (claim is not null)
            {
                userId = int.Parse(claim);
            }

            var (reservation, error, conflict) = await _reservationService.CreateReservationAsync(request, userId);

            if (conflict)
                return Conflict(new { message = error });

            if (error is not null)
                return BadRequest(new { message = error });

            return Ok(reservation);
        }


        [HttpGet]
        [Authorize(Roles = "Manager, Admin, Waiter")]

        public async Task<IActionResult> GetReservations([FromQuery] DateTime? date)
        {
            var reservations = await _reservationService.GetReservationsAsync(date);
            return Ok(reservations);
        }

        [HttpPut("{id}/status")]
        [Authorize]

        public async Task<IActionResult> ChangeReservationStatus (int id, ChangeReservationStatusRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isStaff = User.IsInRole("Manager") || User.IsInRole("Admin") || User.IsInRole("Waiter");

            var (reservation, error, statusCode) = await _reservationService.ChangeStatusAsync(id, request, userId, isStaff);

            if (error is not null)
                return StatusCode(statusCode, new { message = error });

            return Ok(reservation);
        }
    }
}
