namespace BarberShop.Core.Requests.Notifications;

public class PushSubscriptionRequest
{
    public string Endpoint { get; set; } = string.Empty;
    public string P256Dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}
