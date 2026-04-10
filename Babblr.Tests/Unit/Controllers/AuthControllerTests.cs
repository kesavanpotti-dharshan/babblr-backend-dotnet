using Babblr.API.Controllers;
using Babblr.Core.DTOs.Auth;
using Babblr.Core.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Babblr.Tests.Unit.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _sut = new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Register_ShouldReturn200_WhenRegistrationSucceeds()
    {
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "Password1!",
            DisplayName = "Test User"
        };

        var response = new AuthResponseDto
        {
            Token = "jwt-token",
            Email = request.Email,
            DisplayName = request.DisplayName,
            UserId = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(response);

        var result = await _sut.Register(request);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenEmailAlreadyExists()
    {
        var request = new RegisterRequestDto
        {
            Email = "existing@example.com",
            Password = "Password1!",
            DisplayName = "Test"
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(request))
            .ThrowsAsync(new InvalidOperationException("Email is already registered."));

        var result = await _sut.Register(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_ShouldReturn200_WhenCredentialsAreValid()
    {
        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "Password1!"
        };

        var response = new AuthResponseDto
        {
            Token = "jwt-token",
            Email = request.Email,
            DisplayName = "Test User",
            UserId = Guid.NewGuid().ToString(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(response);

        var result = await _sut.Login(request);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().Be(response);
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenCredentialsAreInvalid()
    {
        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ThrowsAsync(new InvalidOperationException("Invalid email or password."));

        var result = await _sut.Login(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}