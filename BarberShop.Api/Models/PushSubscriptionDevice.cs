namespace BarberShop.Api.Models;

public class PushSubscriptionDevice
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string P256Dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
