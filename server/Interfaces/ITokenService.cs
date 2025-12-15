using server.Models;

namespace server.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);

        string GenerateRefreshToken();

        string GenerateAccessTokenFromRefreshToken(RefreshToken refreshToken);
    }
}
