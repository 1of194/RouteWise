using server.Models;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using server.Interfaces;
using System.Security.Cryptography;

namespace server.Services
{
    public class TokenService : ITokenService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenService(IConfiguration configuration)
        {
            _secretKey = configuration["Jwt:Secret"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
        }

        /// Creates a signed JWT containing user Identity information.
        public string GenerateAccessToken( User user)
        {
            // Claims are pieces of info about the user encoded into the token.
            // These can be read by the frontend (decoded) and the backend (authorized).
            var claims = new[]
            {
                new Claim (ClaimTypes.Name,user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            // Convert the secret string into a byte array for the hashing algorithm
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            // HMAC SHA256 is the standard algorithm for signing JWTs
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),  // Short lifespan for security
                signingCredentials: creds);

            // Serialize the token object into its final string format (header.payload.signature)
            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        /// Generates a non-JWT, cryptographically secure random string.
        /// This is stored in the DB and sent as a cookie.
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];  // 256-bit entropy

            // RandomNumberGenerator is more secure than 'Random' because it's non-deterministic
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);

        }


        /// Re-issues an Access Token based on a valid Refresh Token record.
        /// Used when the frontend interceptor detects a 401 error.
        public string GenerateAccessTokenFromRefreshToken(RefreshToken refreshToken)
        {
           
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, refreshToken.User.Username),
            new Claim(ClaimTypes.NameIdentifier, refreshToken.UserId.ToString())
        };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30), // Ensure UtcNow is used for consistent server checks
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
