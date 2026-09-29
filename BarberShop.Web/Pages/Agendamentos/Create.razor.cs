using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Requests.Cortes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using BarberShop.Core.Enums;

namespace BarberShop.Web.Pages.Agendamentos
{
    public partial class CreateAgendamentoPage : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; } = false;
        public CreateAgendamentoRequest InputModel { get; set; } = new();

        [Parameter]
        [SupplyParameterFromQuery(Name = "corteId")]
        public long? CorteId { get; set; }

        // Propriedades para os Chips de 40 em 40 minutos
        public List<TimeSpan> HorariosDisponiveis { get; set; } = new();
        public TimeSpan? HorarioSelecionado { get; set; }

        public List<Corte> Cortes { get; set; } = [];

        #endregion

        #region Services

        [Inject] public IAgendamentoHandler Handler { get; set; } = null!;
        [Inject] public NavigationManager NavigationManager { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;
        [Inject] public ICorteHandler CorteHandler { get; set; } = null!;

        #endregion

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

            if (CorteId is > 0 && Cortes.Any(corte => corte.Id == CorteId.Value))
                InputModel.CorteId = CorteId.Value;

            // Inicia a tela carregando os horários do dia atual
            await OnDateChangedAsync(DateTime.Today);
        }

        #region Methods

        // Novo: Disparado quando o usuário clica num dia no calendário
        public async Task OnDateChangedAsync(DateTime? novaData)
        {
            if (novaData == null) return;

            InputModel.Data = novaData.Value;
            HorarioSelecionado = null; // Zera a seleção ao mudar de dia

            await CarregarHorariosDisponiveisAsync(novaData.Value);
        }

        private async Task CarregarHorariosDisponiveisAsync(DateTime dataSelecionada)
        {
            IsBusy = true;
            try
            {
                // Busca todos os agendamentos
                var result = await Handler.GetAllAsync(new GetAllAgendamentoRequest());
                var horariosOcupados = new List<TimeSpan>();

                if (result.IsSuccess && result.Data != null)
                {
                    // Filtra apenas os agendamentos DESTE dia que não estejam cancelados
                    // (Ajuste a string "Cancelado" caso você use Enum, ex: EStatusAgendamento.Cancelado)
                    horariosOcupados = result.Data
                    .Where(x => x.Data.Date == dataSelecionada.Date && x.Status != EStatusAgendamento.Cancelado)
                    .Select(x => x.Data.TimeOfDay)
                    .ToList();
                }

                // Manda gerar a grade de horários, passando o que já está pego
                GerarHorarios(dataSelecionada, horariosOcupados);
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro ao carregar horários ocupados: " + ex.Message, Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void GerarHorarios(DateTime dataEscolhida, List<TimeSpan> horariosJaOcupadosNoBanco)
        {
            HorariosDisponiveis.Clear();

            var horarioAbertura = new TimeSpan(8, 0, 0); // 08:00
            var horarioFechamento = new TimeSpan(19, 0, 0); // 19:00
            var tolerancia = TimeSpan.FromMinutes(40); // 40 minutos

            var horarioAtual = horarioAbertura;

            while (horarioAtual.Add(tolerancia) <= horarioFechamento)
            {
                // Verifica se a data escolhida é hoje e se o horário já passou no relógio
                bool horarioJaPassou = dataEscolhida.Date == DateTime.Today.Date && horarioAtual <= DateTime.Now.TimeOfDay;

                // Só adiciona se o horário NÃO estiver no banco e NÃO for passado
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

            // Junta a data escolhida com a hora do Chip selecionado
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