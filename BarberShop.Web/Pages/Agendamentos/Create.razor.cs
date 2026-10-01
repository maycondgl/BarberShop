using BarberShop.Core.Enums;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Requests.Cortes;
using BarberShop.Core.Requests.DiasFechados;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Agendamentos
{
    public partial class CreateAgendamentoPage : ComponentBase
    {
        public bool IsLoading { get; set; } = true;
        public bool IsBusy { get; set; } = false;

        public CreateAgendamentoRequest InputModel { get; set; } = new();

        public DateTime? DataSelecionada { get; set; } = DateTime.Today;

        [Parameter]
        [SupplyParameterFromQuery(Name = "corteId")]
        public long? CorteId { get; set; }

        public List<TimeSpan> HorariosDisponiveis { get; set; } = new();
        public TimeSpan? HorarioSelecionado { get; set; }
        public List<Corte> Cortes { get; set; } = new();
        public HashSet<DateTime> DatasFechadas { get; set; } = new();

        public Corte? CorteSelecionado => Cortes.FirstOrDefault(c => c.Id == InputModel.CorteId);

        [Inject] public IAgendamentoHandler Handler { get; set; } = null!;
        [Inject] public NavigationManager NavigationManager { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;
        [Inject] public ICorteHandler CorteHandler { get; set; } = null!;
        [Inject] public IDiaFechadoHandler DiaFechadoHandler { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                var request = new GetAllCorteRequest { PageNumber = 1, PageSize = 100 };
                var result = await CorteHandler.GetAllAsync(request);

                if (result.IsSuccess && result.Data != null)
                {
                    Cortes = result.Data.Where(c => c.Ativo).ToList();
                    if (CorteId is > 0 && Cortes.Any(corte => corte.Id == CorteId.Value))
                    {
                        InputModel.CorteId = CorteId.Value;
                    }
                    else if (Cortes.Any())
                    {
                        InputModel.CorteId = Cortes.First().Id;
                    }
                }

                try
                {
                    var diasResult = await DiaFechadoHandler.GetAllAsync(new GetAllDiasFechadosRequest { PageNumber = 1, PageSize = 100 });
                    if (diasResult.IsSuccess && diasResult.Data != null)
                    {
                        DatasFechadas = diasResult.Data.Select(d => d.Data.Date).ToHashSet();
                    }
                }
                catch
                {
                    DatasFechadas = new();
                }

                AjustarDataInicial();

                if (DataSelecionada.HasValue)
                {
                    await CarregarHorariosDisponiveisAsync(DataSelecionada.Value);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro ao carregar agendamento: " + ex.Message, Severity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AjustarDataInicial()
        {
            var dt = DateTime.Today;
            while (IsDateDisabled(dt) && dt <= DateTime.Today.AddDays(15))
            {
                dt = dt.AddDays(1);
            }

            if (!IsDateDisabled(dt))
                DataSelecionada = dt;
            else
                DataSelecionada = null;
        }

        public async Task OnDateChangedAsync(DateTime? novaData)
        {
            DataSelecionada = novaData;
            HorarioSelecionado = null;

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
            return dt.DayOfWeek == DayOfWeek.Sunday
                || dt.Date < DateTime.Today
                || dt.Date > DateTime.Today.AddDays(15)
                || DatasFechadas.Contains(dt.Date);
        }

        private async Task CarregarHorariosDisponiveisAsync(DateTime data)
        {
            IsBusy = true;
            try
            {
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
                    horariosOcupados = result.Data
                        .Where(x => x.Data.Date == data.Date
                            && x.Status != "Cancelado"
                            && x.Status != EStatusAgendamento.Cancelado.ToString())
                        .Select(x => new TimeSpan(x.Data.Hour, x.Data.Minute, 0))
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

            TimeSpan horarioAbertura;
            TimeSpan horarioFechamento;

            if (dataEscolhida.DayOfWeek == DayOfWeek.Saturday)
            {
                horarioAbertura = new TimeSpan(7, 0, 0);
                horarioFechamento = new TimeSpan(12, 0, 0);
            }
            else
            {
                horarioAbertura = new TimeSpan(8, 0, 0);
                horarioFechamento = new TimeSpan(19, 0, 0);
            }

            var intervalo = TimeSpan.FromMinutes(40);
            var horarioAtual = horarioAbertura;

            while (horarioAtual.Add(intervalo) <= horarioFechamento)
            {
                bool horarioJaPassou = dataEscolhida.Date == DateTime.Today.Date && horarioAtual <= DateTime.Now.TimeOfDay;

                if (!horariosJaOcupadosNoBanco.Contains(horarioAtual) && !horarioJaPassou)
                {
                    HorariosDisponiveis.Add(horarioAtual);
                }
                horarioAtual = horarioAtual.Add(intervalo);
            }
        }

        public void SelecionarHorario(TimeSpan hora)
        {
            HorarioSelecionado = hora;
        }

        public async Task OnSubmitAsync()
        {
            if (InputModel.CorteId == 0)
            {
                Snackbar.Add("Por favor, selecione um corte da lista.", Severity.Warning);
                return;
            }

            if (DataSelecionada == null)
            {
                Snackbar.Add("Por favor, selecione a data do agendamento.", Severity.Warning);
                return;
            }

            if (HorarioSelecionado == null)
            {
                Snackbar.Add("Por favor, escolha um dos horários disponíveis.", Severity.Warning);
                return;
            }

            var dataLocal = DataSelecionada.Value.Date + HorarioSelecionado.Value;
            InputModel.Data = DateTime.SpecifyKind(dataLocal, DateTimeKind.Unspecified);
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