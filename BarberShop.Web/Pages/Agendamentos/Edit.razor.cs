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
    public partial class EditAgendamentoPage : ComponentBase
    {
        #region Properties

        public bool IsLoading { get; set; } = true;
        public bool IsBusy { get; set; } = false;
        public bool HasErrorCarregarHorarios { get; set; } = false;

        public UpdateAgendamentoRequest InputModel { get; set; } = new();

        [Parameter]
        public string Id { get; set; } = string.Empty;

        public DateTime? DataSelecionada { get; set; }
        public TimeSpan? HorarioSelecionado { get; set; }
        public List<TimeSpan> HorariosDisponiveis { get; set; } = new();
        public List<Corte> Cortes { get; set; } = new();
        public HashSet<DateTime> DatasFechadas { get; set; } = new();

        public Corte? CorteSelecionado => Cortes.FirstOrDefault(c => c.Id == InputModel.CorteId);

        #endregion

        #region Services

        [Inject] public IAgendamentoHandler Handler { get; set; } = null!;
        [Inject] public ICorteHandler CorteHandler { get; set; } = null!;
        [Inject] public IDiaFechadoHandler DiaFechadoHandler { get; set; } = null!;
        [Inject] public NavigationManager NavigationManager { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;

        #endregion

        #region Override

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                var cortesResult = await CorteHandler.GetAllAsync(new GetAllCorteRequest { PageNumber = 1, PageSize = 100 });
                if (cortesResult.IsSuccess && cortesResult.Data != null)
                {
                    Cortes = cortesResult.Data.ToList();
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

                if (long.TryParse(Id, out var agendamentoId))
                {
                    var response = await Handler.GetByIdAsync(new GetAgendamentoByIdRequest { Id = agendamentoId });
                    if (response is { IsSuccess: true, Data: not null })
                    {
                        InputModel = new UpdateAgendamentoRequest
                        {
                            Id = response.Data.Id,
                            UserId = response.Data.UserId,
                            CorteId = response.Data.CorteId,
                            Data = response.Data.Data,
                            Status = response.Data.Status
                        };

                        DataSelecionada = response.Data.Data.Date;
                        HorarioSelecionado = new TimeSpan(response.Data.Data.Hour, response.Data.Data.Minute, 0);

                        await CarregarHorariosDisponiveisAsync(DataSelecionada.Value);
                    }
                    else
                    {
                        Snackbar.Add("Agendamento não encontrado.", Severity.Error);
                        NavigationManager.NavigateTo("/agendamentos");
                    }
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

        #endregion

        #region Methods

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

        public async Task RecarregarHorariosAsync()
        {
            if (DataSelecionada.HasValue)
            {
                await CarregarHorariosDisponiveisAsync(DataSelecionada.Value);
            }
        }

        private async Task CarregarHorariosDisponiveisAsync(DateTime data)
        {
            IsBusy = true;
            HasErrorCarregarHorarios = false;
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

                if (!result.IsSuccess)
                {
                    HasErrorCarregarHorarios = true;
                    Snackbar.Add(result.Message ?? "Falha ao verificar os horários disponíveis.", Severity.Warning);
                    return;
                }

                var horariosOcupados = new List<TimeSpan>();

                long currentId = long.TryParse(Id, out var parsedId) ? parsedId : 0;

                if (result.Data != null)
                {
                    horariosOcupados = result.Data
                        .Where(x => x.Id != currentId
                            && x.Data.Date == data.Date
                            && x.Status != "Cancelado"
                            && x.Status != EStatusAgendamento.Cancelado.ToString())
                        .Select(x => new TimeSpan(x.Data.Hour, x.Data.Minute, 0))
                        .ToList();
                }

                GerarHorarios(data, horariosOcupados);

                if (HorarioSelecionado.HasValue && !HorariosDisponiveis.Contains(HorarioSelecionado.Value))
                {
                    if (InputModel.Data.Date == data.Date && InputModel.Data.TimeOfDay == HorarioSelecionado.Value)
                    {
                        HorariosDisponiveis.Add(HorarioSelecionado.Value);
                        HorariosDisponiveis.Sort();
                    }
                    else
                    {
                        HorarioSelecionado = null;
                    }
                }
            }
            catch (Exception ex)
            {
                HasErrorCarregarHorarios = true;
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

        public async Task OnValidSubmitAsync()
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
                var result = await Handler.UpdateAsync(InputModel);

                if (result.IsSuccess)
                {
                    Snackbar.Add("Agendamento atualizado com sucesso!", Severity.Success);
                    NavigationManager.NavigateTo("/agendamentos");
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Ocorreu um erro ao atualizar o agendamento.", Severity.Error);
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

        #endregion
    }
}
