using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Requests.Cortes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace BarberShop.Web.Pages.Agendamentos
{
    public partial class EditAgendamentoPage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; } = false;

        public UpdateAgendamentoRequest InputModel { get; set; } = new();

        [Parameter]
        public string Id { get; set; } = string.Empty;

        #endregion

        #region Services
        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        public TimeSpan? _horario { get; set; } = TimeSpan.FromHours(8);

        [Inject]
        public ICorteHandler CorteHandler { get; set; } = null!;

        public List<Corte> Cortes { get; set; } = [];


        [Inject]
        public IAgendamentoHandler Handler { get; set; } = null!;

        #endregion

        #region Overrides

        protected override async Task OnInitializedAsync()
        {
            IsBusy = true;
            try
            {
                var cortesResult = await CorteHandler.GetAllAsync(new GetAllCorteRequest
                {
                    PageNumber = 1,
                    PageSize = 100
                });
                if (cortesResult.IsSuccess)
                    Cortes = cortesResult.Data ?? new List<Corte>();

                var request = new GetAgendamentoByIdRequest
                {
                    Id = long.Parse(Id)
                };

                var response = await Handler.GetByIdAsync(request);
                if (response is { IsSuccess: true, Data: not null })
                {
                    InputModel = new UpdateAgendamentoRequest
                    {
                        Id = response.Data.Id,
                        CorteId = response.Data.CorteId,
                        Data = response.Data.Data
                    };
                    _horario = response.Data.Data.TimeOfDay;
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

        #region Methods

        public async Task OnValidSubmitAsync()
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
                var result = await Handler.UpdateAsync(InputModel);
                if (result.IsSuccess)
                {
                    Snackbar.Add("Agendamento atualizado", Severity.Success);
                    NavigationManager.NavigateTo("/agendamentos");
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

            #endregion

        }
    }
}
