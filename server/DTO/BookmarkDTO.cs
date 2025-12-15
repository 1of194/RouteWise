using server.Models;

namespace server.DTO
{

        public class BookmarkDto
        {
                public int UserId { get; set; }  // optional, if you track users
                public List<DeliveryAddressDto> DeliveryAddresses { get; set; }
        }

        public class DeliveryAddressDto
        {
                public string Street { get; set; }
                public string City { get; set; }
                public string PostalCode { get; set; }
                public DeliveryAddress.PriorityLevel Priority { get; set; }
                public int GeolocationId { get; set; }
                public GeoLocationDto GeoLocation { get; set; }
                public DateTime CreatedAt { get; set; }
        }


        public class GeoLocationDto
        {
                public double Latitude { get; set; }
                public double Longitude { get; set; }
        }
}

