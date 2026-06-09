using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Application.Services;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;

namespace FiapCloudGames.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
[Tags("Usuários")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("list")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Get All Users")]
    public async Task<IActionResult> ListAllUsers()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role
        }));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Get User By Id")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound("Usuário não encontrado");

        return Ok(new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        });
    }

    [HttpGet("me")]
    [SwaggerOperation(Summary = "Get My Profile")]
    public async Task<IActionResult> GetMyProfile()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized("Usuário inválido");

        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
            return NotFound("Usuário não encontrado");

        return Ok(new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        });
    }



    [HttpPatch("me")]
    [SwaggerOperation(Summary = "Update My Profile")]
    public async Task<IActionResult> UpdateMyProfile(UpdateUserDto dto)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized("Usuário inválido");

        var updatedUser = await _userService.UpdateAsync(userId, dto);
        if (updatedUser == null)
            return NotFound("Usuário não encontrado");

        return Ok(new UserResponseDto
        {
            Id = updatedUser.Id,
            Name = updatedUser.Name,
            Email = updatedUser.Email,
            Role = updatedUser.Role
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Update User By Id")]
    public async Task<IActionResult> UpdateUserById(Guid id, UpdateUserDto dto)
    {
        var updatedUser = await _userService.UpdateAsync(id, dto);
        if (updatedUser == null)
            return NotFound("Usuário não encontrado");

        return Ok(new UserResponseDto
        {
            Id = updatedUser.Id,
            Name = updatedUser.Name,
            Email = updatedUser.Email,
            Role = updatedUser.Role
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Delete User By Id")]
    public async Task<IActionResult> DeleteUserById(Guid id)
    {
        if (!await _userService.DeleteAsync(id))
            return NotFound("Usuário não encontrado");

        return NoContent();
    }

    [HttpPatch("{id}/role")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Change User Role")]
    public async Task<IActionResult> ChangeUserRole(Guid id, ChangeUserRoleDto dto)
    {
        if (!await _userService.ChangeRoleAsync(id, dto))
            return NotFound("Usuário não encontrado");

        return NoContent();
    }


}