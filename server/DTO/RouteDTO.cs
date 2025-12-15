using server.Models;

namespace server.DTO
{
    public class RouteDTO
    {
            public string Address { get; set; } = string.Empty;  // required field
            public DeliveryAddress.PriorityLevel Priority { get; set; }
    }
}
