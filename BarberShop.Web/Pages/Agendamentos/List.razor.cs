using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Agendamentos
{
    public partial class ListAgendamentoPage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; } = false;
        public List<Agendamento> Agendamentos { get; set; } = [];
        public string SearchTerm { get; set; } = String.Empty;

        #endregion

        #region Services

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public IDialogService DialogService { get; set; } = null!;

        [Inject]
        public IAgendamentoHandler Handler { get; set; } = null!;

        #endregion

        #region Overrides

        protected override async Task OnInitializedAsync()
        {
            IsBusy = true;
            try
            {
                var request = new GetAllAgendamentoRequest
                {
                    PageNumber = 1,
                    PageSize = 25
                };
                var result = await Handler.GetAllAsync(request);
                if (result.IsSuccess)
                    Agendamentos = result.Data ?? new List<Agendamento>();
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

        public async void OnDeleteButtonClickedAsync(long id, string corte)
        {
            var result = await DialogService.ShowMessageBox(
                "ATENÇÃO",
                $"Ao prosseguir o agendamento {id} será excluído. Esta é uma ação irreversível! Deseja continuar?",
                yesText: "EXCLUIR",
                cancelText: "Cancelar");

            if (result is true)
                await OnDeleteAsync(id);

            StateHasChanged();
        }

        public async Task OnDeleteAsync(long id) 
        {
            try
            {
                await Handler.DeleteAsync(id);
                Agendamentos.RemoveAll(x => x.Id == id);
                Snackbar.Add("Agendamento excluído", Severity.Success);
            }
            catch(Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
        }

        public Func<Agendamento, bool> Filter => agendamento =>
        {
            if (string.IsNullOrEmpty(SearchTerm))
                return true;

            if (agendamento.Id.ToString().Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                return true;

            if (agendamento.Corte.Titulo.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                return true;

            if (agendamento.Data.ToString().Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                return true; 
            
            if (agendamento.Tempo.ToString().Contains(SearchTerm, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        #endregion
    }
}
