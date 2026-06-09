using Microsoft.AspNetCore.Mvc;
using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Application.Services;

namespace FiapCloudGames.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        try
        {
            var user = await _userService.RegisterAsync(dto);
            return CreatedAtAction(nameof(Register), new { id = user.Id }, new { message = "Usuário criado com sucesso" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var token = await _userService.AuthenticateAsync(dto);
        if (token == null)
            return Unauthorized("Credenciais inválidas");

        return Ok(new { token });
    }
}
