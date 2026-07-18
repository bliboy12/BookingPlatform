
using System.ComponentModel.DataAnnotations;

public class CreateConsultantProfileRequest
{
    [Required]
    public string BankAccountNumber { get; set; } = string.Empty;
    [Required]
    public string StreetName { get; set; } = string.Empty;
    [Required]
    public string HouseNumber { get; set; } = string.Empty;
    [Required]
    public string PostalCode { get; set; } = string.Empty;
    [Required]
    public string City { get; set; } = string.Empty;
    [Required]
    public string Country { get; set; } = string.Empty;
}