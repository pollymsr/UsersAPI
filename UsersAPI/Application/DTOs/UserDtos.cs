using System.ComponentModel.DataAnnotations;

namespace FiapCloudGames.Application.DTOs;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    public string? Name { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [MinLength(8)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).+$")]
    public string? Password { get; set; }
}

public class ChangeUserRoleDto
{
    [Required]
    [RegularExpression("^(User|Admin)$", ErrorMessage = "Role deve ser User ou Admin")]
    public string Role { get; set; } = string.Empty;
}
