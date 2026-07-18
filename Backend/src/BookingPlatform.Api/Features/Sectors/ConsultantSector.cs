public class ConsultantSector
{
    public Guid ConsultantProfileId { get; set; }
    public ConsultantProfile ConsultantProfile { get; set; } = null!;
    public Guid SectorId { get; set; }
    public Sector Sector { get; set; } = null!;
}