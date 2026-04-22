using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Requests.Cortes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace BarberShop.Web.Pages.Agendamentos
{
    public partial class CreateAgendamentoPage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; } = false;
        public CreateAgendamentoRequest InputModel { get; set; } = new();

        #endregion

        #region Services

        [Inject]
        public IAgendamentoHandler Handler { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        public TimeSpan? _horario { get; set; } = TimeSpan.FromHours(8);

        [Inject]
        public ICorteHandler CorteHandler { get; set; } = null!;

        public List<Corte> Cortes { get; set; } = [];

        protected override async Task OnInitializedAsync()
        {
            var request = new GetAllCorteRequest
            {
                PageNumber = 1,
                PageSize = 100
            };

            var result = await CorteHandler.GetAllAsync(request);
            if (result.IsSuccess)
                Cortes = result.Data ?? new List<Corte>();
        }

        #endregion

        #region Methods

        public async Task OnValidSubmitAsync(EditContext context)
        {
            if (InputModel.CorteId == 0)
            {
                Snackbar.Add("Selecione um tipo de corte", Severity.Warning);
                return;
            }

            if (InputModel.Data == default)
            {
                Snackbar.Add("Selecione uma data", Severity.Warning);
                return;
            }

            InputModel.Data = InputModel.Data.Date + (_horario ?? TimeSpan.Zero);

            IsBusy = true;
            try
            {
                var result = await Handler.CreateAsync(InputModel);
                if (result.IsSuccess)
                {
                    Snackbar.Add(result.Message, Severity.Success);
                    NavigationManager.NavigateTo("/agendamentos");
                }
                else
                    Snackbar.Add(result.Message, Severity.Error);
            }
            catch(Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);

            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion
    }
}
