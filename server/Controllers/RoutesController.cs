using Microsoft.AspNetCore.Mvc;
using server.Models;
using server.DTO;
using System.Text.Json;
using server.Services;
using Microsoft.Extensions.Caching.Memory;


namespace server.Controllers
{
    [Route("api/route")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly RouteWiseContext _routeContext;
        private readonly RouteOptimization _routeOptimization;
       
        private readonly IMemoryCache _cache;

        public RoutesController(IHttpClientFactory httpClientFactory, RouteWiseContext routeContext, RouteOptimization routeOptimization, IMemoryCache cache)
        {
            _httpClient = httpClientFactory.CreateClient();  // Method must have a return type
            _routeContext = routeContext;
            _routeOptimization = routeOptimization;
            _cache = cache;
        }

        
        
        private static string GetPropString(JsonElement props, params string[] keys)
{
	foreach (var k in keys)
	{
		if (props.TryGetProperty(k, out var v))
		{
			var s = v.GetString();
			if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
		}
	}
	return string.Empty;
}    


        // POST api/<route>
        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody]  RouteDTO route)
        {

            if (string.IsNullOrWhiteSpace(route.Address))
                return BadRequest("Address is required.");

            // Build the query string
            string url = $"https://photon.komoot.io/api?q={route.Address.Trim()}&limit=1";

            // Add required User-Agent header
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("RouteWise/1.0");

            string response;
            try
            {
                response = await _httpClient.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                return StatusCode(503, $"Geocoding service error: {ex.Message}");
            }

            JsonDocument jsonDoc;
            try
            {
                Console.WriteLine($"Geocoding response: {response}");
                jsonDoc = JsonDocument.Parse(response);
            }
            catch
            {
                return StatusCode(502, new { Message = "Invalid response from geocoding service." });
            }

            // Photon returns either a top-level array OR { features: [...] }
            JsonElement resultsArray;
            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
            {
                resultsArray = jsonDoc.RootElement;  
            }
            else if (jsonDoc.RootElement.TryGetProperty("features", out var features) && features.ValueKind == JsonValueKind.Array)
            {
                resultsArray = features; // Photon API format
            }
            else
            {
                return StatusCode(502, new { Message = "Invalid response format from geocoding service." });
            }

            if (resultsArray.GetArrayLength() == 0)
            {
                return NotFound(new { Message = "Address not found in geocoding service." });
            }

            var firstItem = resultsArray[0];

            // Get geometry
            if (!firstItem.TryGetProperty("geometry", out var geometry) ||
                !geometry.TryGetProperty("coordinates", out var coords) ||
                coords.GetArrayLength() != 2)
            {
                return StatusCode(502, new { Message = "Missing or invalid coordinates in geocoding response." });
            }

            double lon = coords[0].GetDouble();
            double lat = coords[1].GetDouble();

            // Get properties
            if (!firstItem.TryGetProperty("properties", out var properties))
            {
                return StatusCode(502, new { Message = "Missing 'properties' field in geocoding response." });
            }

            // Robust fallbacks for Photon properties
            var streetBase = GetPropString(properties, "road", "street", "residential", "pedestrian", "path", "highway", "name");
            var houseNumber = GetPropString(properties, "house_number", "housenumber");
            string street = (streetBase + " " + houseNumber).Trim();
            if (string.IsNullOrWhiteSpace(street))
            {
                street = GetPropString(properties, "name");
                if (string.IsNullOrWhiteSpace(street)) street = "Unknown";
            }

            string city = GetPropString(properties, "city", "town", "village", "municipality", "suburb", "county", "state", "region", "locality", "name");
            if (string.IsNullOrWhiteSpace(city)) city = "Unknown";

            string postalCode = GetPropString(properties, "postcode", "post_code", "postalcode", "zip", "zipcode");
            if (string.IsNullOrWhiteSpace(postalCode)) postalCode = "00000";

            // Retrieve existing lists or initialize new ones
            var deliveryList = _cache.Get<List<DeliveryAddress>>("Addresses") ?? new List<DeliveryAddress>();
            var geoList = _cache.Get<List<GeoLocation>>("Geos") ?? new List<GeoLocation>();

            // Assign a unique Id to the new GeoLocation
            int nextGeoId = geoList.Any() ? geoList.Max(g => g.Id) + 1 : 1;
            var geo = new GeoLocation { Id = nextGeoId, Latitude = lat, Longitude = lon };

            int nextAddressId = deliveryList.Any() ? deliveryList.Max(a => a.Id) + 1 : 1;

            var newAddress = new DeliveryAddress
            {
                Id = nextAddressId,
                Street = street,
                City = city,
                PostalCode = postalCode,
                Priority = route.Priority,
                GeolocationId = geo.Id
            };

            // Prevent duplicates by coordinates (exact)
            bool addressExists = deliveryList.Any(a =>
            {
                var existingGeo = geoList.FirstOrDefault(g => g.Id == a.GeolocationId);
                return existingGeo != null &&
                       existingGeo.Latitude == geo.Latitude &&
                       existingGeo.Longitude == geo.Longitude;
            });
            if (!addressExists)
            {
                deliveryList.Add(newAddress);
                geoList.Add(geo);

                _cache.Set("Addresses", deliveryList, TimeSpan.FromMinutes(10));
                _cache.Set("Geos", geoList, TimeSpan.FromMinutes(10));
            }
            else
            {
                return Conflict(new { Message = "This address with the same coordinates already exists." });
            }

            // Trigger route optimization once enough points collected
            if (deliveryList.Count >= 4 && geoList.Count >= 4)
            {
                // Call the route optimization service
                var addresses = _routeOptimization.RouteOptimazer(deliveryList, geoList);

                // Clear cache after processing
                _cache.Remove("Addresses");
                _cache.Remove("Geos");
                
                return Ok(addresses);
            }

            return Ok(new { Message = "Address added successfully" });
        }


       
    }


}
