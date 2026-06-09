using System.ComponentModel.DataAnnotations;

namespace FiapCloudGames.Application.DTOs;

public class RegisterDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).+$")]
    public string Password { get; set; } = string.Empty;
}
