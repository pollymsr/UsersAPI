using FiapCloudGames.Domain.Entities;

namespace FiapCloudGames.Domain.Services;

public class UserDomainService : IUserDomainService
{
    private static readonly string[] ValidRoles = { "User", "Admin" };

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public bool IsValidRole(string role)
    {
        return !string.IsNullOrWhiteSpace(role) && ValidRoles.Contains(role);
    }
}
