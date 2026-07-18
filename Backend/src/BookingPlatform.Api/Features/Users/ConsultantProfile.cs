public class ConsultantProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public Address Address { get; set; } = null!;

    public ICollection<ConsultantSector> ConsultantSectors { get; set; } = new List<ConsultantSector>();
    public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
}