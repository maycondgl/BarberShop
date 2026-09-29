using BarberShop.Core.Enums;
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
        public bool IsLoading { get; set; } = true;
        public bool IsBusy { get; set; } = false;

        public CreateAgendamentoRequest InputModel { get; set; } = new();

        // NOVA PROPRIEDADE: Feita com '?' para o MudDatePicker não crashar
        public DateTime? DataSelecionada { get; set; } = DateTime.Today;

        [Parameter]
        [SupplyParameterFromQuery(Name = "corteId")]
        public long? CorteId { get; set; }

        public List<TimeSpan> HorariosDisponiveis { get; set; } = new();
        public TimeSpan? HorarioSelecionado { get; set; }
        public List<Corte> Cortes { get; set; } = new();

        [Inject] public IAgendamentoHandler Handler { get; set; } = null!;
        [Inject] public NavigationManager NavigationManager { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;
        [Inject] public ICorteHandler CorteHandler { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                var request = new GetAllCorteRequest { PageNumber = 1, PageSize = 100 };
                var result = await CorteHandler.GetAllAsync(request);

                if (result.IsSuccess && result.Data != null)
                {
                    Cortes = result.Data;
                    if (CorteId is > 0 && Cortes.Any(corte => corte.Id == CorteId.Value))
                        InputModel.CorteId = CorteId.Value;
                }

                await CarregarHorariosDisponiveisAsync(DateTime.Today);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task OnDateChangedAsync(DateTime? novaData)
        {
            DataSelecionada = novaData;
            HorarioSelecionado = null; // Limpa a seleção anterior

            if (novaData.HasValue)
            {
                await CarregarHorariosDisponiveisAsync(novaData.Value);
            }
            else
            {
                HorariosDisponiveis.Clear();
            }
        }

        public bool IsDateDisabled(DateTime dt)
        {
            return dt.DayOfWeek == DayOfWeek.Sunday || dt.Date < DateTime.Today;
        }

        private async Task CarregarHorariosDisponiveisAsync(DateTime data)
        {
            IsBusy = true;
            try
            {
                // Buscamos um período alargado para cobrir o dia inteiro sem falhas de fuso
                var request = new GetAgendamentoByPeriodRequest
                {
                    StartDate = data.Date,
                    EndDate = data.Date.AddDays(1).AddTicks(-1),
                    PageNumber = 1,
                    PageSize = 100
                };

                var result = await Handler.GetByPeriodAsync(request);
                var horariosOcupados = new List<TimeSpan>();

                if (result.IsSuccess && result.Data != null)
                {
                    // Comparamos apenas o .Date e ignoramos o Kind para apanhar os agendamentos de TODOS os clientes
                    horariosOcupados = result.Data
                        .Where(x => x.Data.Date == data.Date && x.Status != "Cancelado" && x.Status != EStatusAgendamento.Cancelado.ToString())
                        .Select(x => new TimeSpan(x.Data.Hour, x.Data.Minute, 0)) // Normaliza para horas e minutos limpos
                        .ToList();
                }

                GerarHorarios(data, horariosOcupados);
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

        // NOVO MÉTODO: Usado pelos botões de horário
        public void SelecionarHorario(TimeSpan hora)
        {
            HorarioSelecionado = hora;
        }

        public async Task OnSubmitAsync()
        {
            // 1. Validações manuais para dar feedback ao utilizador
            if (InputModel.CorteId == 0)
            {
                Snackbar.Add("Por favor, selecione o tipo de corte.", Severity.Warning);
                return;
            }

            if (DataSelecionada == null)
            {
                Snackbar.Add("Por favor, selecione a data do agendamento.", Severity.Warning);
                return;
            }

            if (HorarioSelecionado == null)
            {
                Snackbar.Add("Por favor, clique num dos botões de horário disponíveis.", Severity.Warning);
                return;
            }

            // 2. Prepara os dados: Junta o dia (DataSelecionada) com a hora (HorarioSelecionado)
            var dataLocal = DataSelecionada.Value.Date + HorarioSelecionado.Value;
            InputModel.Data = DateTime.SpecifyKind(dataLocal, DateTimeKind.Local);
            IsBusy = true;

            try
            {
                // 3. Envia para a API local!
                var result = await Handler.CreateAsync(InputModel);

                if (result.IsSuccess)
                {
                    Snackbar.Add("Agendamento criado com sucesso!", Severity.Success);
                    NavigationManager.NavigateTo("/agendamentos"); // Volta para a lista
                }
                else
                {
                    // Mostra o erro exato que a API devolveu
                    Snackbar.Add(result.Message ?? "Ocorreu um erro ao agendar.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro de comunicação: " + ex.Message, Severity.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}