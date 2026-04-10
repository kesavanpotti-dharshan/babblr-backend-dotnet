using Babblr.Core.DTOs.Auth;
using Babblr.Core.Entities;
using Babblr.Core.Interfaces.Services;
using Babblr.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Babblr.Tests.Unit.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userManagerMock = new Mock<UserManager<AppUser>>(
            Mock.Of<IUserStore<AppUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _tokenServiceMock = new Mock<ITokenService>();

        _sut = new AuthService(_userManagerMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnToken_WhenUserIsNew()
    {
        var request = new RegisterRequestDto
        {
            Email = "new@example.com",
            Password = "Password1!",
            DisplayName = "New User"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((AppUser?)null);

        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<AppUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        _tokenServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<AppUser>()))
            .Returns("mock-jwt-token");

        var result = await _sut.RegisterAsync(request);

        result.Should().NotBeNull();
        result.Token.Should().Be("mock-jwt-token");
        result.Email.Should().Be(request.Email);
        result.DisplayName.Should().Be(request.DisplayName);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var request = new RegisterRequestDto
        {
            Email = "existing@example.com",
            Password = "Password1!",
            DisplayName = "Existing User"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(new AppUser { Email = request.Email });

        var act = async () => await _sut.RegisterAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email is already registered.");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var request = new LoginRequestDto
        {
            Email = "dharshan@example.com",
            Password = "Password1!"
        };

        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            DisplayName = "Dharshan"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(true);

        _tokenServiceMock
            .Setup(x => x.GenerateToken(user))
            .Returns("mock-jwt-token");

        var result = await _sut.LoginAsync(request);

        result.Should().NotBeNull();
        result.Token.Should().Be("mock-jwt-token");
        result.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsWrong()
    {
        var request = new LoginRequestDto
        {
            Email = "dharshan@example.com",
            Password = "WrongPassword!"
        };

        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            DisplayName = "Dharshan"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(false);

        var act = async () => await _sut.LoginAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserDoesNotExist()
    {
        var request = new LoginRequestDto
        {
            Email = "nobody@example.com",
            Password = "Password1!"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((AppUser?)null);

        var act = async () => await _sut.LoginAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid email or password.");
    }
}