using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Models;
using server.DTO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace server.Controllers
{
    [Authorize]
    [Route("api/bookmark")]
    [ApiController]
    public class BookmarkController : ControllerBase
    {
        private readonly RouteWiseContext _routecontext;



        public BookmarkController(RouteWiseContext routeContext)
        {
            _routecontext = routeContext;

        }


        // GET api/bookmark

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Bookmark>>> GetBookmarks()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var uid))
            return Unauthorized(new { message = "Invalid or missing user id." });

        var bookmarks = await _routecontext.Bookmarks
            .Include(b => b.DeliveryAddresses)
            .Include(b => b.GeoLocations)
            .Include(b => b.User)
            .Where(b => b.UserId == uid)
            .ToListAsync();

        if (!bookmarks.Any()) return NotFound("No bookmarks found.");
        return Ok(bookmarks);
    }


        [HttpGet("{id}")]
        public async Task<ActionResult<Bookmark>> GetBookmark(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var bookmark = await _routecontext.Bookmarks
                .Include(b => b.DeliveryAddresses)
                .Include(b => b.GeoLocations)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId.ToString() == userId);

            if (bookmark == null)
            {
                return NotFound($"Bookmark with id {id} not found.");
            }

            return Ok(bookmark);
        }



        // POST api/<BookmarkController>
        [HttpPost]
        public async Task<ActionResult> AddBookmark([FromBody] List<BookmarkDto> bookmarkDtos)
        {
            try
            {
                Console.WriteLine("📥 Incoming DTOs: " +
                    System.Text.Json.JsonSerializer.Serialize(bookmarkDtos));

                if (bookmarkDtos == null || !bookmarkDtos.Any())
                {
                    Console.WriteLine("❌ BookmarkDtos is null or empty");
                    return BadRequest(new { message = "Bookmark cannot be null.", bookmark = new List<BookmarkDto>() });
                }

               

                // Get the authenticated user's ID from the token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "User not authenticated or invalid." });
                }

                var allAddresses = bookmarkDtos
                    .SelectMany(b => b.DeliveryAddresses) // flatten
                    .GroupBy(a => new { a.Street, a.City, a.PostalCode }) // distinct by these fields
                    .Select(g => g.First())
                    .ToList();

                

                Console.WriteLine("✅ All Addresses DTOs: " +
                    System.Text.Json.JsonSerializer.Serialize(allAddresses));

                DateTime createdAt = allAddresses
                    .Where(b => b.CreatedAt != default)
                    .OrderByDescending(b => b.CreatedAt)
                    .FirstOrDefault()?.CreatedAt ?? DateTime.UtcNow;

                Console.WriteLine("🕒 Created At: " + createdAt);

                var addressList = new List<DeliveryAddress>();

                foreach (var dto in allAddresses)
                {
                    Console.WriteLine($"➡️ Processing DTO: {dto.Street}, {dto.City}, {dto.PostalCode}");

                    // Null-check the incoming GeoLocation DTO
                    var lat = dto.GeoLocation?.Latitude ?? 0;
                    var lng = dto.GeoLocation?.Longitude ?? 0;


                    var geo = new GeoLocation { Latitude = lat , Longitude = lng};
                    Console.WriteLine($"   🌍 GeoLocation created: {geo.Latitude}, {geo.Longitude}");

                    var address = new DeliveryAddress { 
                        Street = dto.Street,
                        City = dto.City,
                        PostalCode = dto.PostalCode,
                        Priority = dto.Priority,
                        Geolocation = geo   
       
                    };
              
                    addressList.Add(address);
                    
                }

                var newBookmark = new Bookmark
                {
                    UserId =userId,
                    DeliveryAddresses = addressList,
                    GeoLocations = addressList.Select(a => a.Geolocation).ToList(),
                    CreatedAt = createdAt
                };

                Console.WriteLine(" New Bookmark prepared, saving to DB...");
                await _routecontext.Bookmarks.AddAsync(newBookmark);
                await _routecontext.SaveChangesAsync();
                Console.WriteLine(" Bookmark saved successfully");

                return CreatedAtAction(nameof(GetBookmarks), new { },
                    new { msessage = "Route successfully saved!", bookmark = bookmarkDtos });
            }
            catch (Exception ex)
            {
                Console.WriteLine(" ERROR in Post /bookmark: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
        // DELETE api/<BookmarkController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteRoute(int id)
        {
            var foundItem = await _routecontext.Bookmarks.FindAsync(id);

            if (foundItem == null)
            {
                return NotFound($"Bookmark with id {id} not found.");
            }

            _routecontext.Bookmarks.Remove(foundItem);
            await _routecontext.SaveChangesAsync();

            // 204 No Content is the usual response for successful DELETE
            return NoContent();
        }

      
    }


}
