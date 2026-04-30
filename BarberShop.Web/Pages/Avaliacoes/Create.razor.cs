using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Requests.Avaliacao;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace BarberShop.Web.Pages.Avaliacoes
{
    public partial class CreateAvaliacaoPage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; } = false;
        public CreateAvaliacaoRequest InputModel { get; set; } = new();
        public List<Agendamento> Agendamentos { get; set; } = [];
        public Agendamento? AgendamentoSelecionado { get; set; }
        [Parameter] public long AgendamentoId { get; set; }

        #endregion

        #region Services

        [Inject]
        public IAvaliacaoHandler AvaliacaoHandler { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public IAgendamentoHandler AgendamentoHandler { get; set; } = null!;

        #endregion

        #region Override

        protected override async Task OnInitializedAsync()
        {
            IsBusy = true;
            try
            {
                InputModel.AgendamentoId = AgendamentoId;

                var result = await AgendamentoHandler.GetByIdAsync(new GetAgendamentoByIdRequest { Id = AgendamentoId });

                if (result.IsSuccess && result.Data != null)
                {
                    InputModel.Data = result.Data.Data;
                    StateHasChanged();
                }
                else
                {
                    Snackbar.Add("Não foi possível carregar os dados do agendamento", Severity.Error);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region Methods

        public async Task OnValidSubmitAsync(EditContext context)
        { 
            IsBusy = true;
            try
            {
                var result = await AvaliacaoHandler.CreateAsync(InputModel);
                if (result.IsSuccess)
                {
                    Snackbar.Add("Avaliação enviada com sucesso!", Severity.Success);
                    NavigationManager.NavigateTo("/avaliacoes");
                }
                else
                    Snackbar.Add(result.Message, Severity.Error);
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

    }
}
