using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.DTO;
using server.Models;
using server.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;


namespace server.Controllers
{

    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RouteWiseContext _routecontext;
        private readonly ITokenService _tokenService;

        public AuthController(RouteWiseContext routeContext, ITokenService tokenService)
        {
            _routecontext = routeContext;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("api/login")]
        public async Task<IActionResult> Login([FromBody] UserDTO auth)
        {
            if (string.IsNullOrWhiteSpace(auth.Username) || string.IsNullOrWhiteSpace(auth.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var existingUser = await _routecontext.Users
                .FirstOrDefaultAsync(u => u.Username == auth.Username);

            if (auth.PageType == PageType.Register)
            {
                if (existingUser != null)
                {
                    return Conflict(new { message = "User already exists." });
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(auth.Password);
                var newUser = new User
                {
                    Username = auth.Username,
                    PasswordHash = hashedPassword
                };

                _routecontext.Users.Add(newUser);
                await _routecontext.SaveChangesAsync();

                var accessToken = _tokenService.GenerateAccessToken(newUser);
                var refreshToken = _tokenService.GenerateRefreshToken();



                var newRefreshToken = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = newUser.Id,
                    IsRevoked = false,
                    ExpiryDate = DateTime.UtcNow.AddDays(5)
                };

                _routecontext.RefreshTokens.Add(newRefreshToken);
                await _routecontext.SaveChangesAsync();

                Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(5)
                });

                return Ok(new { message = "User registered successfully.", AccessToken = accessToken });
            }
            else
            {
                if (existingUser == null || !BCrypt.Net.BCrypt.Verify(auth.Password, existingUser.PasswordHash))
                {
                    return Unauthorized(new { message = "Invalid username or password." });
                }

                var accessToken = _tokenService.GenerateAccessToken(existingUser);
                var refreshToken = _tokenService.GenerateRefreshToken();

                Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(5)
                });

                var newRefreshToken = new RefreshToken
                {
                    Token = refreshToken,
                    UserId = existingUser.Id,
                    IsRevoked = false,
                    ExpiryDate = DateTime.UtcNow.AddDays(5)
                };

                _routecontext.RefreshTokens.Add(newRefreshToken);
                await _routecontext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Login successful.",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("api/refresh-token")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "No refresh token provided." });

            var tokenRecord = await _routecontext.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (tokenRecord == null || tokenRecord.ExpiryDate < DateTime.UtcNow || tokenRecord.IsRevoked)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            var newRefreshToken = _tokenService.GenerateRefreshToken();

            tokenRecord.IsRevoked = true;

            var replacement = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = tokenRecord.UserId,
                IsRevoked = false,
                ExpiryDate = DateTime.UtcNow.AddDays(5)
            };
            _routecontext.RefreshTokens.Add(replacement);
            await _routecontext.SaveChangesAsync();

            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(5)

            });

            var newAccessToken = _tokenService.GenerateAccessTokenFromRefreshToken(replacement);

            return Ok(new
            {
                message = "Token refreshed successfully.",
                AccessToken = newAccessToken,
                
            });
        }

        [HttpPost("api/logout")]
        public async Task<IActionResult> Logout([FromBody] TokenResponseDTO tokenResponse)
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return BadRequest(new { message = "Refresh token is required." });
                }

                var tokenRecord = await _routecontext.RefreshTokens
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Token == refreshToken);

                if (tokenRecord != null)
                {
                    tokenRecord.IsRevoked = true;
                    await _routecontext.SaveChangesAsync();
                }
                Response.Cookies.Delete("refreshToken");

                return Ok(new { message = "Logged out successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred during logout." });
            }
        }
    }


}
