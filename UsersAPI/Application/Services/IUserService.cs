using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Domain.Entities;

namespace FiapCloudGames.Application.Services;

public interface IUserService
{
    Task<User> RegisterAsync(RegisterDto dto);
    Task<string?> AuthenticateAsync(LoginDto dto);
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> UpdateAsync(Guid id, UpdateUserDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ChangeRoleAsync(Guid id, ChangeUserRoleDto dto);
}
