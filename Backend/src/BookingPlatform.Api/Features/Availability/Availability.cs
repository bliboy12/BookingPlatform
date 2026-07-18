public class Availability
{
    public Guid Id { get; set; }
    public Guid ConsultantProfileId { get; set; }
    public ConsultantProfile ConsultantProfile { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}