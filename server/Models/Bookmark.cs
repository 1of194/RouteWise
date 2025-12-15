using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    public class Bookmark
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
         
        public User User { get; set; }

        [InverseProperty("Bookmark")]
        public ICollection<DeliveryAddress> DeliveryAddresses { get; set; } = new List<DeliveryAddress>();

        [InverseProperty("Bookmark")]
        public ICollection<GeoLocation> GeoLocations { get; set; } = new List<GeoLocation>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
