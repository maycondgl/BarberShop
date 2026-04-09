namespace BarberShop.Core
{
    public static class Configuration
    {
        public const int DefaultCode = 200;
        public const int DefaultPageSize = 25;

        public static string Connection { get; set; } = string.Empty;
        public static string BackendUrl { get; set; } = string.Empty;
        public static string FrontendUrl { get; set; } = string.Empty;

    }
}
