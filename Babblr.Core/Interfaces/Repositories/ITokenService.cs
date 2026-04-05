using Babblr.Core.Entities;

namespace Babblr.Core.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(AppUser user);
}