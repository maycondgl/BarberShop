using BarberShop.Core.Enums; // Para corrigir o erro CS0019
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
        public bool IsLoading { get; set; } = true;
        public bool IsBusy { get; set; } = false;

        // Inicia logo com a data de hoje para evitar o 01/01/0001
        public CreateAgendamentoRequest InputModel { get; set; } = new()
        {
            Data = DateTime.Today
        };

        [Parameter]
        [SupplyParameterFromQuery(Name = "corteId")]
        public long? CorteId { get; set; }

        public List<TimeSpan> HorariosDisponiveis { get; set; } = new();
        public TimeSpan? HorarioSelecionado { get; set; }
        public List<Corte> Cortes { get; set; } = new();
        #endregion

        #region Services
        [Inject] public IAgendamentoHandler Handler { get; set; } = null!;
        [Inject] public NavigationManager NavigationManager { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;
        [Inject] public ICorteHandler CorteHandler { get; set; } = null!;
        #endregion

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                // Carrega a lista de cortes
                var request = new GetAllCorteRequest { PageNumber = 1, PageSize = 100 };
                var result = await CorteHandler.GetAllAsync(request);

                if (result.IsSuccess && result.Data != null)
                {
                    Cortes = result.Data;
                    if (CorteId is > 0 && Cortes.Any(corte => corte.Id == CorteId.Value))
                        InputModel.CorteId = CorteId.Value;
                }

                // Inicia os horários para o dia de hoje
                await CarregarHorariosDisponiveisAsync(DateTime.Today);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task OnDateChangedAsync(DateTime? novaData)
        {
            if (novaData == null) return;

            InputModel.Data = novaData.Value;
            HorarioSelecionado = null;
            await CarregarHorariosDisponiveisAsync(novaData.Value);
        }

        private async Task CarregarHorariosDisponiveisAsync(DateTime dataSelecionada)
        {
            IsBusy = true;
            try
            {
                var result = await Handler.GetAllAsync(new GetAllAgendamentoRequest());
                var horariosOcupados = new List<TimeSpan>();

                if (result.IsSuccess && result.Data != null)
                {
                    horariosOcupados = result.Data
                        .Where(x => x.Data.Date == dataSelecionada.Date && x.Status != EStatusAgendamento.Cancelado)
                        .Select(x => x.Data.TimeOfDay)
                        .ToList();
                }

                GerarHorarios(dataSelecionada, horariosOcupados);
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro ao carregar horários: " + ex.Message, Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void GerarHorarios(DateTime dataEscolhida, List<TimeSpan> horariosJaOcupadosNoBanco)
        {
            HorariosDisponiveis.Clear();
            var horarioAbertura = new TimeSpan(8, 0, 0);
            var horarioFechamento = new TimeSpan(19, 0, 0);
            var tolerancia = TimeSpan.FromMinutes(40);
            var horarioAtual = horarioAbertura;

            while (horarioAtual.Add(tolerancia) <= horarioFechamento)
            {
                bool horarioJaPassou = dataEscolhida.Date == DateTime.Today.Date && horarioAtual <= DateTime.Now.TimeOfDay;

                if (!horariosJaOcupadosNoBanco.Contains(horarioAtual) && !horarioJaPassou)
                {
                    HorariosDisponiveis.Add(horarioAtual);
                }
                horarioAtual = horarioAtual.Add(tolerancia);
            }
        }

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
            if (HorarioSelecionado == null)
            {
                Snackbar.Add("Selecione um horário disponível nas opções", Severity.Warning);
                return;
            }

            InputModel.Data = InputModel.Data.Date + HorarioSelecionado.Value;
            IsBusy = true;

            try
            {
                var result = await Handler.CreateAsync(InputModel);
                if (result.IsSuccess)
                {
                    Snackbar.Add("Agendamento criado com sucesso!", Severity.Success);
                    NavigationManager.NavigateTo("/agendamentos");
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Erro ao agendar.", Severity.Error);
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
    }
}