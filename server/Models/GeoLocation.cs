using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.Models
{
    public class GeoLocation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public required double Latitude { get; set; }

        [Required]
        public required double Longitude { get; set; }

        [ForeignKey("Bookmark")]
        public int BookmarkId { get; set; }

        public Bookmark Bookmark { get; set; }

       
        

    }

    




}


