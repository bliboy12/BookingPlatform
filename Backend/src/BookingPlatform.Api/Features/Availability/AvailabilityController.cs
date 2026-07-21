

using System.Security.Claims;
using BookingPlatform.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/availability")]
[Authorize(Roles = "Provider")]
public class AvailabilityController : ControllerBase
{
    private readonly AppDbContext _db;
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public AvailabilityController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyAvailability(SetAvailabilityRequest request)
    {
        var profile = await _db.ConsultantProfiles.FirstOrDefaultAsync(cp => cp.UserId == CurrentUserId);

        if (profile is not null)
            return NotFound("Consultant profile not found. Create your first profile ");

        // -- Here I validate each slot individually -- 
        foreach (var slot in request.Slots)
        {
            if (slot.StartTime >= slot.EndTime)
                return BadRequest($"Invalid slot on {slot.DayOfWeek}: start time must be before end time");
        }

        // check for overlaps within the set
        var byDay = request.Slots.GroupBy(s => s.DayOfWeek);
        foreach (var dayGroup in byDay)
        {
            var ordered = dayGroup.OrderBy(s => s.StartTime).ToList();
            for (int i = 0; i < ordered.Count - 1; i++)
            {
                if (ordered[i].EndTime > ordered[i + 1].StartTime)
                    return BadRequest($"Overlapping slots on {dayGroup.Key}");
            }
        }

        // -- don't allow changing availability of the provider if the have a booked appointment outside the times the wish to change to --
        var confirmingBookings = await _db.Bookings.Where(b => b.ConsultantProfileId == profile!.Id && b.Status == BookingStatus.Confirmed).ToListAsync();

        foreach (var booking in confirmingBookings)
        {
            var bookingDay = booking.StartTime.DayOfWeek;
            var bookingStart = TimeOnly.FromDateTime(booking.StartTime);
            var bookingEnd = TimeOnly.FromDateTime(booking.EndTime);

            // check if the booked appointments fall under the providers availability for the day
            bool stillCovered = request.Slots.Any(s => s.DayOfWeek == bookingDay && s.StartTime <= bookingStart && s.EndTime >= bookingEnd);

            if (!stillCovered)
                return BadRequest($"Cannot update availability: existing confirmed booking on {booking.StartTime:f} would no longer fall within your availability. Cancel or reschedule it first");
        }

        // -- Deleting all the old values to be replaced with the weeks new changes
        // This is instead of seeing what has changed and replacing only that, which would take more code then necessary --
        await using var transaction = await _db.Database.BeginTransactionAsync();

        var existing = await _db.Availabilities.Where(a => a.ConsultantProfileId == profile!.Id).ToListAsync();
        _db.Availabilities.RemoveRange(existing);

        var newSlots = request.Slots.Select(s => new Availability
        {
            ConsultantProfileId = profile!.Id,
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime = s.EndTime
        }).ToList();

        _db.Availabilities.AddRange(newSlots);

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return Ok(newSlots);
    }
}