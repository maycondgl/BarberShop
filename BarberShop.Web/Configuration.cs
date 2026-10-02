using MudBlazor;

namespace BarberShop.Web
{
    public static class Configuration
    {
        public const string HttpClientName = "BarberShop";

        public static string BackendUrl { get; set; } = "https://barbershop-api-hmbsh7b0cugdbabd.centralus-01.azurewebsites.net";

        public static MudTheme Theme = new()
        {
            Palette = new PaletteDark()
            {
                Primary = "#FFC107", // Dourado luminoso clássico
                Secondary = "#FFB300",
                Tertiary = "#FFD54F",

                Background = "#121214", // Grafite escuro sofisticado
                Surface = "#1C1C22",    // Cartões com superfície distinta do fundo

                AppbarBackground = "#16161A",
                AppbarText = "#FFFFFF",

                DrawerBackground = "#16161A",
                DrawerText = "#E2E2E8",
                DrawerIcon = "#B0B0BA",

                TextPrimary = "#FFFFFF",
                TextSecondary = "#A0A0B0",

                ActionDefault = "#B0B0BA",
                ActionDisabled = "#616161",
                ActionDisabledBackground = "#2A2A2A",

                LinesDefault = "rgba(255, 193, 7, 0.22)",
                TableLines = "rgba(255, 255, 255, 0.08)",
                Divider = "rgba(255, 255, 255, 0.12)",

                PrimaryContrastText = "#000000"
            },

            PaletteDark = new PaletteDark()
            {
                Primary = "#FFC107", // Dourado luminoso clássico
                Secondary = "#FFB300",
                Tertiary = "#FFD54F",

                Background = "#121214", // Grafite escuro sofisticado (não preto morto)
                Surface = "#1C1C22",    // Cartões com superfície distinta do fundo

                AppbarBackground = "#16161A",
                AppbarText = "#FFFFFF",

                DrawerBackground = "#16161A",
                DrawerText = "#E2E2E8",
                DrawerIcon = "#B0B0BA",

                TextPrimary = "#FFFFFF",
                TextSecondary = "#A0A0B0",

                ActionDefault = "#B0B0BA",
                ActionDisabled = "#616161",
                ActionDisabledBackground = "#2A2A2A",

                LinesDefault = "rgba(255, 193, 7, 0.22)",
                TableLines = "rgba(255, 255, 255, 0.08)",
                Divider = "rgba(255, 255, 255, 0.12)",

                PrimaryContrastText = "#000000"
            }
        };
    }
}
