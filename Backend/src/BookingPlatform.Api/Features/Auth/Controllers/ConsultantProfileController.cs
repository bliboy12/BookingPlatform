

using System.Security.Claims;
using BookingPlatform.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/consultant-profile")]
[Authorize(Roles = "Provider")]
public class ConsultantProfileController : ControllerBase
{
    private readonly AppDbContext _db;
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public ConsultantProfileController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var profile = await _db.ConsultantProfiles.Include(cp => cp.ConsultantSectors).ThenInclude(cs => cs.Sector).FirstOrDefaultAsync(cp => cp.UserId == CurrentUserId);

        if (profile is null)
            return NotFound("Profile not yet Created");

        return Ok(profile);
    }
    [HttpPost("me")]
    public async Task<IActionResult> CreateMyProfile(CreateConsultantProfileRequest request)
    {
        var existing = await _db.ConsultantProfiles.FirstOrDefaultAsync(cp => cp.UserId == CurrentUserId);

        if (existing is not null)
            return BadRequest("Profile already exists");

        var profile = new ConsultantProfile
        {
            UserId = CurrentUserId,
            BankAccountNumber = request.BankAccountNumber,
            Address = new Address
            {
                StreetName = request.StreetName,
                HouseNumber = request.HouseNumber,
                PostalCode = request.PostalCode,
                City = request.City,
                Country = request.Country
            }
        };
        _db.ConsultantProfiles.Add(profile);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMyProfile), new { }, profile);
    }
}