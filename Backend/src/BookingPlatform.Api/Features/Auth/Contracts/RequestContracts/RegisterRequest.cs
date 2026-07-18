using System.ComponentModel.DataAnnotations;

public class RegisterRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public DateOnly BirthDate { get; set; }
    public string Role { get; set; } = "Client";
}