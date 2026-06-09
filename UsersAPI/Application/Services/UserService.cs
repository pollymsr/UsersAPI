using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Domain.Entities;
using FiapCloudGames.Domain.Services;
using FiapCloudGames.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MassTransit;
using FiapCloudGames.Events;

namespace FiapCloudGames.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserDomainService _userDomainService;
    private readonly IConfiguration _configuration;
    private readonly IPublishEndpoint _publishEndpoint;

    public UserService(
        IUserRepository userRepository,
        IUserDomainService userDomainService,
        IConfiguration configuration,
        IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _userDomainService = userDomainService;
        _configuration = configuration;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<User> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepository.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email já cadastrado");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = _userDomainService.HashPassword(dto.Password),
            Role = "User"
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        await _publishEndpoint.Publish(new UserCreatedEvent(user.Id, user.Name, user.Email));

        return user;
    }

    public async Task<string?> AuthenticateAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());
        if (user == null || !_userDomainService.VerifyPassword(dto.Password, user.PasswordHash))
            return null;

        return GenerateToken(user);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User?> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return null;

        user.Name = dto.Name?.Trim() ?? user.Name;
        user.Email = dto.Email?.Trim().ToLowerInvariant() ?? user.Email;

        if (!string.IsNullOrWhiteSpace(dto.Password))
            user.PasswordHash = _userDomainService.HashPassword(dto.Password);

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return false;

        await _userRepository.DeleteAsync(user);
        return await _userRepository.SaveChangesAsync();
    }

    public async Task<bool> ChangeRoleAsync(Guid id, ChangeUserRoleDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            return false;

        if (!_userDomainService.IsValidRole(dto.Role))
            throw new InvalidOperationException("Role inválida. Use User ou Admin.");

        user.Role = dto.Role;
        await _userRepository.UpdateAsync(user);
        return await _userRepository.SaveChangesAsync();
    }



    private string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key está faltando")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(2);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
