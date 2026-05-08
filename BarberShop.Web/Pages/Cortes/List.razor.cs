using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Cortes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.Globalization;

namespace BarberShop.Web.Pages.Cortes
{
    public partial class ListCortesPage : ComponentBase
    {
        private static readonly string[] MediaStyles =
        [
            "background: linear-gradient(145deg, rgba(255,193,7,.72), rgba(14,14,15,.32)), url('Imgs/logo.png') center / 50% no-repeat, #202022;",
            "background: linear-gradient(145deg, rgba(181,111,29,.76), rgba(14,14,15,.42)), url('Imgs/logo.png') center / 50% no-repeat, #202022;",
            "background: linear-gradient(145deg, rgba(88,96,112,.76), rgba(14,14,15,.42)), url('Imgs/logo.png') center / 50% no-repeat, #202022;",
            "background: linear-gradient(145deg, rgba(255,179,0,.66), rgba(14,14,15,.48)), url('Imgs/logo.png') center / 50% no-repeat, #202022;",
            "background: linear-gradient(145deg, rgba(70,70,76,.88), rgba(14,14,15,.46)), url('Imgs/logo.png') center / 50% no-repeat, #202022;",
            "background: linear-gradient(145deg, rgba(36,36,42,.92), rgba(255,193,7,.24)), url('Imgs/logo.png') center / 50% no-repeat, #202022;"
        ];

        [Inject] public ICorteHandler Handler { get; set; } = null!;
        [Inject] public NavigationManager NavigationManager { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;

        public bool IsBusy { get; set; } = true;
        public List<Corte> Cortes { get; set; } = [];
        public List<CorteCatalogItem> Catalogo { get; set; } = [];
        public long? SelectedImageCorteId { get; set; }

        #region Override
        protected override async Task OnInitializedAsync()
        {
            try
            {
                var result = await Handler.GetAllAsync(new GetAllCorteRequest
                {
                    PageNumber = 1,
                    PageSize = 50
                });

                if (result.IsSuccess && result.Data is not null)
                {
                    Cortes = result.Data;
                    Catalogo = Cortes
                        .Where(c => c.Titulo != "Corte Personalizado" && c.Titulo != "Corte Personalizado + Barba")
                        .Select((corte, index) => CorteCatalogItem.FromCorte(corte, MediaStyles[index % MediaStyles.Length]))
                        .ToList();
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Erro ao carregar cortes", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        public void SelectImage(long corteId)
           => SelectedImageCorteId = SelectedImageCorteId == corteId ? null : corteId;

        public bool IsImageSelected(long corteId)
            => SelectedImageCorteId == corteId;

        public string GetImageFrameClass(long id)
        {
            return IsImageSelected(id)
                ? "cut-image-selector cut-image-selector--selected"
                : "cut-image-selector";
        }

        public string GetCardClass(long id)
        {
            return IsImageSelected(id)
                ? "cc-card cc-card--active"
                : "cc-card";
        }

        public void HandleImageKeyDown(KeyboardEventArgs args, long corteId)
        {
            if (args.Key is "Enter" or " ")
            {
                SelectImage(corteId);
            }
        }


        public void ScheduleCut(long corteId)
            => NavigationManager.NavigateTo($"/agendamentos/adicionar?corteId={corteId}");

        public static string FormatDuration(int minutes)
            => minutes <= 0 ? "A consultar" : $"{minutes} min";

        public static string FormatPrice(decimal price)
            => price.ToString("C2", CultureInfo.GetCultureInfo("pt-BR"));

        public static readonly Dictionary<string, string> ImagensPorCorte = new()
            {
                { "Corte Social",    "Imgs/corte-de-cabelo-social-2.png" },
                { "Americano",       "Imgs/americano.png" },
                { "LowFade",         "Imgs/lowfade.png" },
                { "Moicano",         "Imgs/moicano.png" },
                { "BuzzCut",         "Imgs/buzzcut.png" },

            };



        public sealed record CorteCatalogItem(
            long Id,
            string Titulo,
            decimal Preco,
            int DuracaoMinutos,
            string MediaStyle,
            string ImagemUrl)
        {
            public static CorteCatalogItem FromCorte(Corte corte, string mediaStyle)
                => new(
                    corte.Id,
                    corte.Titulo,
                    corte.Preco,
                    corte.DuracaoMinutos,
                    mediaStyle,
                    ImagensPorCorte.GetValueOrDefault(corte.Titulo, "Imgs/corte-de-cabelo-social-2.png"));
        }
    }
}
