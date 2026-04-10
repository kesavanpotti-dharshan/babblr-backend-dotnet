using Babblr.Core.Entities;
using Babblr.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace Babblr.Tests.Unit.Services;

public class TokenServiceTests
{
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "Babblr@SuperSecret#2024!Key$Secure99",
                ["Jwt:Issuer"] = "babblr-api",
                ["Jwt:Audience"] = "babblr-client"
            })
            .Build();

        _sut = new TokenService(config);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwt_WhenUserIsValid()
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            UserName = "test@example.com",
            DisplayName = "Test User"
        };

        var token = _sut.GenerateToken(user);

        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Should().NotBeNull();
    }

    [Fact]
    public void GenerateToken_ShouldContainCorrectClaims_WhenUserIsValid()
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "dharshan@example.com",
            UserName = "dharshan@example.com",
            DisplayName = "Dharshan"
        };

        var token = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c =>
            c.Type == "email" || c.Value == user.Email);
        jwt.Issuer.Should().Be("babblr-api");
    }

    [Fact]
    public void GenerateToken_ShouldExpireInSevenDays()
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@example.com",
            UserName = "test@example.com",
            DisplayName = "Test"
        };

        var token = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.ValidTo.Should().BeCloseTo(
            DateTime.UtcNow.AddDays(7),
            precision: TimeSpan.FromMinutes(1));
    }
}