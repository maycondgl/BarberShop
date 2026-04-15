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
                Secondary = "#FF8F00",
                Background = "#FFFFFF",
                AppbarBackground = "#FFC107",
                Surface = "#FFFFFF",
                AppbarText = "#000000",
                TextPrimary = "#000000",
                PrimaryContrastText = "000000",
                DrawerText = Colors.Shades.Black,
                DrawerBackground = Colors.Yellow.Darken3
            },

            PaletteDark = new PaletteDark()
            {
                Primary = "#FFC107",
                AppbarBackground = "#121212",
                AppbarText = "#FFC107"
            }
        };
    }
}
