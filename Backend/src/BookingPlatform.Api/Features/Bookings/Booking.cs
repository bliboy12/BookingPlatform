public class Booking
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public ApplicationUser Client { get; set; } = null!;

    public Guid ConsultantProfileId { get; set; }
    public ConsultantProfile ConsultantProfile { get; set; } = null!;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public decimal HourlyRate { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

}