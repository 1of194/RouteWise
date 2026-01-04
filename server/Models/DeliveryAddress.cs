using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models {


    public class DeliveryAddress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required string Street { get; set; }

        [Required]
        public required string City { get; set; }

        [Required]
        public required string PostalCode { get; set; }

        [Required]
        public required PriorityLevel Priority { get; set; }

        [ForeignKey("Geolocation")]
        public int GeolocationId { get; set; }

        public GeoLocation Geolocation { get; set; }

        [ForeignKey("Bookmark")]
        public int? BookmarkId { get; set; }

        public Bookmark Bookmark { get; set; }

        public enum PriorityLevel
        {
            Start = 0,     // starting location
            High = 1,      // IsPriority = true
            Normal = 2     // IsPriority = false
        }



    }


}
