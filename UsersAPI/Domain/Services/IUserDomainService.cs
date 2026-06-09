using FiapCloudGames.Domain.Entities;

namespace FiapCloudGames.Domain.Services;

public interface IUserDomainService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    bool IsValidRole(string role);
}
