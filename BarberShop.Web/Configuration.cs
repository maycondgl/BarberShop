using MudBlazor;

namespace BarberShop.Web
{
    public static class Configuration
    {
        public const string HttpClientName = "BarberShop";

        public static string BackendUrl { get; set; } = "http://localhost:5131";

        public static MudTheme Theme = new()
        {
            Palette = new PaletteLight()
            {
                Primary = "#FFC107",

                AppbarBackground = "#FFC107",
                AppbarText = "#000000",

                DrawerBackground = "#FFFFFF",
                DrawerText = "#111111",
                DrawerIcon = "#111111",

                Background = "#F5F5F5",
                Surface = "#FFFFFF",

                TextPrimary = "#111111",
                TextSecondary = "#444444",

                PrimaryContrastText = "#000000"

            },

            PaletteDark = new PaletteDark()
            {
                Primary = "#FFC107",
                Secondary = "#FFB300",
                Background = "#121212",
                Surface = "#1E1E1E",
                AppbarBackground = "#1E1E1E",
                DrawerBackground = "#1A1A1A"
            }
        };
    }
}
