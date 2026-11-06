using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FiapCloudGames.API.Controllers;
using FiapCloudGames.Application.DTOs;
using FiapCloudGames.Application.Services;
using FiapCloudGames.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace UsersAPI.Tests;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _controller = new UserController(_userServiceMock.Object);
    }

    [Fact]
    public async Task ListAllUsers_ShouldReturnOkWithUsers()
    {
        // Arrange
        var mockUsers = new List<User>
        {
            new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@fiap.com.br", Role = "Admin" },
            new User { Id = Guid.NewGuid(), Name = "Bob", Email = "bob@fiap.com.br", Role = "User" }
        };

        _userServiceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync(mockUsers);

        // Act
        var result = await _controller.ListAllUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var users = Assert.IsAssignableFrom<IEnumerable<UserResponseDto>>(okResult.Value);
        Assert.Equal(2, users.Count());
    }

    [Fact]
    public async Task GetUserById_WhenUserExists_ShouldReturnOkWithUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockUser = new User { Id = userId, Name = "Alice", Email = "alice@fiap.com.br", Role = "Admin" };

        _userServiceMock.Setup(s => s.GetByIdAsync(userId))
            .ReturnsAsync(mockUser);

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDto = Assert.IsType<UserResponseDto>(okResult.Value);
        Assert.Equal(userId, responseDto.Id);
        Assert.Equal("Alice", responseDto.Name);
    }

    [Fact]
    public async Task GetUserById_WhenUserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userServiceMock.Setup(s => s.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetUserById(userId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
