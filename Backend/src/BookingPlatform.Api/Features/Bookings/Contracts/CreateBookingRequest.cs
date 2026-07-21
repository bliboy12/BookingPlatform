
public class CreateBookingRequest
{
    public Guid ConsultantProfileId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}