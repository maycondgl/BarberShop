using BarberShop.Core.Enums;
using BarberShop.Core.Handlers;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Responses.Agendamento;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Admin.Agendamentos
{
    public class ListAdminAgendamentoPage : ComponentBase
    {
        [Inject] public IAgendamentoHandler Handler { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;

        public List<AgendamentoResponse> Agendamentos { get; set; } = [];
        public string SearchTerm { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            var request = new GetAllAgendamentoRequest { UserId = 0, PageNumber = 1, PageSize = 100 };
            var result = await Handler.GetAllAdminAsync(request);
            if (result.IsSuccess && result.Data is not null)
                Agendamentos = result.Data;
            else
                Snackbar.Add(result.Message ?? "Erro ao carregar", Severity.Error);
        }

        public Func<AgendamentoResponse, bool> Filter => x =>
            string.IsNullOrWhiteSpace(SearchTerm) ||
            x.NomeCliente.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
            x.Status.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase);

        public async Task OnAceitarClickedAsync(long id)
        {
            var request = new UpdateStatusAgendamentoRequest 
            { 
                Id = id, 
                Status = EStatusAgendamento.Aceito 
            };

            var result = await Handler.UpdateStatusAsync(request);
            if (result.IsSuccess)
            {
                Snackbar.Add("Agendamento aceito!", Severity.Success);
                await OnInitializedAsync();
            }
            else
                Snackbar.Add(result.Message ?? "Erro", Severity.Error);
        }

        public async Task OnConcluirClickedAsync(long id)
        {
            var request = new UpdateStatusAgendamentoRequest 
            { 
                Id = id, 
                Status = EStatusAgendamento.Concluido 
            };

            var result = await Handler.UpdateStatusAsync(request);
            if (result.IsSuccess)
            {
                Snackbar.Add("Agendamento concluído!", Severity.Success);
                await OnInitializedAsync();
            }
            else
                Snackbar.Add(result.Message ?? "Erro", Severity.Error);
        }


        public async Task OnDeleteClickedAsync(long id)
        {
            var result = await Handler.DeleteAsync(id);
            if (result.IsSuccess)
            {
                Snackbar.Add("Agendamento excluído!", Severity.Success);
                Agendamentos.RemoveAll(x => x.Id == id);
            }
            else
                Snackbar.Add(result.Message ?? "Erro", Severity.Error);
        }
    }
}