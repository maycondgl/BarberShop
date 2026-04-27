using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Agendamentos;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace BarberShop.Web.Pages.Pagamento
{
    public partial class PixPage : ComponentBase
    {
        #region Properties
        [Parameter] public long Id { get; set; }
        [Parameter] public decimal Valor { get; set; }

        #endregion

        #region Services

        [Inject]
        public IAgendamentoHandler Handler { get; set; } = null!;

        [Inject] 
        public NavigationManager NavigationManager { get; set; } = null!;

        [Inject] 
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject] 
        public IJSRuntime JS { get; set; } = null!;

        #endregion

        #region Methods

        public const string ChavePix = "068.152.388-30";

        protected string QrCodeUrl =>
        $"https://api.qrserver.com/v1/create-qr-code/?size=250x250&data=" +
            $"{Uri.EscapeDataString(ChavePix)}&valor={Valor.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&saida=qr&chave={ChavePix}";

        protected async Task CopiarChave()
        {
            await JS.InvokeVoidAsync("navigator.clipboard.writeText", ChavePix);
            Snackbar.Add("Chave Pix copiada!", Severity.Success);
        }
        protected override async Task OnInitializedAsync()
        {
            var result = await Handler.GetByIdAsync(new GetAgendamentoByIdRequest { Id = Id });
            if (result.IsSuccess && result.Data is not null)
                Valor = result.Data.Valor;
        }

        #endregion

    }
}