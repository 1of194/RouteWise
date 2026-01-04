using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.DTO;
using server.Models;
using server.Interfaces;
using Microsoft.AspNetCore.Authorization;


namespace server.Controllers
{

    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RouteWiseContext _routecontext;
        private readonly ITokenService _tokenService;
        private readonly IWebHostEnvironment _env;

        // Dependencies: DB Context, Token Logic, and Environment (to check if we are in Production/Development)
        public AuthController(RouteWiseContext routeContext, IWebHostEnvironment env ,ITokenService tokenService)
        {
            _routecontext = routeContext;
            _tokenService = tokenService;
            _env = env;
        }

        /// Combines Registration and Login into one endpoint based on PageType.
        /// [AllowAnonymous] is required because the user isn't logged in yet!
        [AllowAnonymous]
        [HttpPost("api/login")]
        public async Task<IActionResult> Login([FromBody] UserDTO auth)
        {
            // Ensure fields aren't empty
            if (string.IsNullOrWhiteSpace(auth.Username) || string.IsNullOrWhiteSpace(auth.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var existingUser = await _routecontext.Users
                .FirstOrDefaultAsync(u => u.Username == auth.Username);

            // BRANCH 1: REGISTRATION
            if (auth.PageType == PageType.Register)
            {
                if (existingUser != null)
                {
                    return Conflict(new { message = "User already exists." });
                }

                //  BCrypt handles the "Salting" automatically.
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(auth.Password);
                var newUser = new User
                {
                    Username = auth.Username,
                    PasswordHash = hashedPassword
                };

                _routecontext.Users.Add(newUser);
                await _routecontext.SaveChangesAsync();

                // Generate initial tokens for the new user
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

                // Set the Refresh Token in a secure HttpOnly cookie
                Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = _env.IsProduction(),
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.Now.AddDays(5)
                });

                return Ok(new { message = "User registered successfully.", user = new {userId = newUser.Id, username = newUser.Username},AccessToken = accessToken });
            }
            // BRANCH 2: LOGIN
            else
            {
                // Verification: Compare incoming password with the stored hash
                if (existingUser == null || !BCrypt.Net.BCrypt.Verify(auth.Password, existingUser.PasswordHash))
                {
                    return Unauthorized(new { message = "Invalid username or password." });
                }

                var accessToken = _tokenService.GenerateAccessToken(existingUser);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Set the Cookie
                Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = _env.IsProduction(),
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.Now.AddDays(5)
                });

                // Persist the new Refresh Token
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
                    user =  new { userId = existingUser.Id, username = existingUser.Username},  // send user to frontend
                    AccessToken = accessToken,
                });
            }
        }

        /// REFRESH TOKEN ROTATION
        /// Used when the short-lived Access Token expires. 
        /// It reads the refreshToken from the cookie and issues a new pair.
        [AllowAnonymous]
        [HttpPost("api/refresh-token")]
        public async Task<IActionResult> Refresh()
        {
            // Extract the cookie sent automatically by the browser
            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "No refresh token provided." });

            var tokenRecord = await _routecontext.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            // Validation: Must exist, not expired, and not manually revoked (logout)
            if (tokenRecord == null || tokenRecord.ExpiryDate < DateTime.UtcNow || tokenRecord.IsRevoked)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            // ROTATION LOGIC:
            // 1. Generate a brand new Refresh Token
            // 2. Revoke the old one (prevent reuse attacks)
            // 3. Issue a new Access Token
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            tokenRecord.IsRevoked = true;

            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = _env.IsProduction(),
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.Now.AddDays(5)
            });

            var replacement = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = tokenRecord.UserId,
                IsRevoked = false,
                ExpiryDate = DateTime.UtcNow.AddDays(5)
            };
            _routecontext.RefreshTokens.Add(replacement);
            await _routecontext.SaveChangesAsync();



            var newAccessToken = _tokenService.GenerateAccessTokenFromRefreshToken(replacement);

            return Ok(new
            {
                message = "Token refreshed successfully.",
                AccessToken = newAccessToken,
                
            });
        }

        /// LOGOUT
        /// Revokes the token in the database and deletes the cookie.
        [HttpPost("api/logout")]
        public async Task<IActionResult> Logout()
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
                // Clear the cookie from the browser
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
