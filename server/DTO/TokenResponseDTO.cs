using server.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace server.DTO
{
    public class TokenResponseDTO
    {
        public string AccessToken { get; set; }

    }
}
