using System.ComponentModel.DataAnnotations;

namespace server.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }                  // Unique user ID

        [Required]
        public required string Username { get; set; }         // Login name

        [Required]
        public required string PasswordHash { get; set; }     // Hashed password for auth

        
       public ICollection<Bookmark> Bookmarks { get; set; }

        
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    }
}
