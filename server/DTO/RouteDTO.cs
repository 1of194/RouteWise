using server.Models;

namespace server.DTO
{
    public class RouteDTO
    {
            public string Address { get; set; } = string.Empty;
            public DeliveryAddress.PriorityLevel Priority { get; set; }
    }
}
