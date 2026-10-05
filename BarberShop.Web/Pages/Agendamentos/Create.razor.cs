using BarberShop.Core.Enums;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Agendamentos;
using BarberShop.Core.Requests.Barbeiros;
using BarberShop.Core.Requests.Cortes;
using BarberShop.Core.Requests.DiasFechados;
using BarberShop.Core.Requests.Filiais;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Agendamentos
{
    public partial class CreateAgendamentoPage : ComponentBase
    {
        #region Properties

        public bool IsLoading { get; set; } = true;
        public bool IsBusy { get; set; } = false;
        public bool HasErrorCarregarHorarios { get; set; } = false;

        public CreateAgendamentoRequest InputModel { get; set; } = new();

        public DateTime? DataSelecionada { get; set; } = DateTime.Today;

        [Parameter]
        [SupplyParameterFromQuery(Name = "corteId")]
        public long? CorteId { get; set; }

        public List<Filial> Filiais { get; set; } = new();
        public List<Barbeiro> Barbeiros { get; set; } = new();

        public Filial? FilialSelecionada => Filiais.FirstOrDefault(f => f.Id == InputModel.FilialId);
        public Barbeiro? BarbeiroSelecionado => Barbeiros.FirstOrDefault(b => b.Id == InputModel.BarbeiroId);

        public bool IsFilialSheetOpen { get; set; } = false;
        public bool IsBarbeiroSheetOpen { get; set; } = false;
        public bool IsServicoSheetOpen { get; set; } = false;

        public List<TimeSpan> HorariosDisponiveis { get; set; } = new();
        public TimeSpan? HorarioSelecionado { get; set; }
        public List<Corte> Cortes { get; set; } = new();
        public HashSet<DateTime> DatasFechadas { get; set; } = new();

        public List<long> CortesSelecionadosIds { get; set; } = new();

        public List<Corte> CortesSelecionados =>
            Cortes.Where(c => CortesSelecionadosIds.Contains(c.Id)).ToList();

        public decimal PrecoTotalServicos =>
            CortesSelecionados.Sum(c => c.Preco);

        public int DuracaoTotalServicos =>
            CortesSelecionados.Sum(c => c.DuracaoMinutos > 0 ? c.DuracaoMinutos : 40);

        public string TituloServicosSelecionados =>
            CortesSelecionados.Any()
                ? string.Join(" + ", CortesSelecionados.Select(c => c.Titulo))
                : "Nenhum serviço selecionado";

        public Corte? CorteSelecionado => CortesSelecionados.FirstOrDefault() ?? Cortes.FirstOrDefault(c => c.Id == InputModel.CorteId);

        #endregion

        #region Services

        [Inject] public IAgendamentoHandler Handler { get; set; } = null!;
        [Inject] public NavigationManager NavigationManager { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;
        [Inject] public ICorteHandler CorteHandler { get; set; } = null!;
        [Inject] public IDiaFechadoHandler DiaFechadoHandler { get; set; } = null!;
        [Inject] public IFilialHandler FilialHandler { get; set; } = null!;
        [Inject] public IBarbeiroHandler BarbeiroHandler { get; set; } = null!;

        #endregion

        #region Override

        protected override async Task OnInitializedAsync()
        {
            IsLoading = true;
            try
            {
                await CarregarFiliaisAsync();
                await CarregarBarbeirosAsync(InputModel.FilialId);

                var request = new GetAllCorteRequest { PageNumber = 1, PageSize = 100 };
                var result = await CorteHandler.GetAllAsync(request);

                if (result.IsSuccess && result.Data != null)
                {
                    Cortes = result.Data.ToList();
                    if (CorteId is > 0 && Cortes.Any(corte => corte.Id == CorteId.Value))
                    {
                        InputModel.CorteId = CorteId.Value;
                        CortesSelecionadosIds = new List<long> { CorteId.Value };
                        InputModel.CorteIds = CortesSelecionadosIds.ToList();
                    }
                    else if (Cortes.Any())
                    {
                        var primeiro = Cortes.First();
                        InputModel.CorteId = primeiro.Id;
                        CortesSelecionadosIds = new List<long> { primeiro.Id };
                        InputModel.CorteIds = CortesSelecionadosIds.ToList();
                    }
                }

                await CarregarDiasFechadosAsync();
                AjustarDataInicial();

                if (DataSelecionada.HasValue)
                {
                    await CarregarHorariosDisponiveisAsync(DataSelecionada.Value);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add(ex.Message, Severity.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        #endregion

        #region Methods

        public void AbrirFilialSheet() => IsFilialSheetOpen = true;
        public void FecharFilialSheet() => IsFilialSheetOpen = false;

        public async Task SelecionarFilialAsync(Filial filial)
        {
            InputModel.FilialId = filial.Id;
            FecharFilialSheet();

            await CarregarBarbeirosAsync(filial.Id);
            if (!Barbeiros.Any(b => b.Id == InputModel.BarbeiroId))
            {
                InputModel.BarbeiroId = Barbeiros.FirstOrDefault()?.Id;
            }

            if (DataSelecionada.HasValue)
            {
                await CarregarHorariosDisponiveisAsync(DataSelecionada.Value);
            }
        }

        public void AbrirBarbeiroSheet() => IsBarbeiroSheetOpen = true;
        public void FecharBarbeiroSheet() => IsBarbeiroSheetOpen = false;

        public async Task SelecionarBarbeiroAsync(Barbeiro barbeiro)
        {
            InputModel.BarbeiroId = barbeiro.Id;
            FecharBarbeiroSheet();

            if (DataSelecionada.HasValue)
            {
                await CarregarHorariosDisponiveisAsync(DataSelecionada.Value);
            }
        }

        public void AbrirServicoSheet() => IsServicoSheetOpen = true;
        public void FecharServicoSheet() => IsServicoSheetOpen = false;

        public void AlternarSelecaoServico(Corte corte)
        {
            if (CortesSelecionadosIds.Contains(corte.Id))
            {
                if (CortesSelecionadosIds.Count > 1)
                {
                    CortesSelecionadosIds.Remove(corte.Id);
                }
                else
                {
                    Snackbar.Add("Selecione pelo menos um serviço.", Severity.Info);
                }
            }
            else
            {
                CortesSelecionadosIds.Add(corte.Id);
            }
        }

        public async Task ConfirmarSelecaoServicosAsync()
        {
            if (!CortesSelecionadosIds.Any())
            {
                Snackbar.Add("Selecione pelo menos um serviço.", Severity.Warning);
                return;
            }

            InputModel.CorteIds = CortesSelecionadosIds.ToList();
            InputModel.CorteId = CortesSelecionadosIds.First();
            FecharServicoSheet();

            if (DataSelecionada.HasValue)
            {
                await CarregarHorariosDisponiveisAsync(DataSelecionada.Value);
            }
        }

        private async Task CarregarFiliaisAsync()
        {
            try
            {
                var result = await FilialHandler.GetAllAsync(new GetAllFilialRequest
                {
                    PageNumber = 1,
                    PageSize = 100,
                    ApenasAtivos = true
                });
                if (result.IsSuccess && result.Data != null)
                {
                    Filiais = result.Data.ToList();
                    if (!InputModel.FilialId.HasValue || InputModel.FilialId.Value == 0)
                    {
                        var primeira = Filiais.FirstOrDefault();
                        if (primeira != null)
                        {
                            InputModel.FilialId = primeira.Id;
                        }
                    }
                }
            }
            catch
            {
                Filiais = new();
            }
        }

        private async Task CarregarBarbeirosAsync(long? filialId = null)
        {
            try
            {
                var result = await BarbeiroHandler.GetAllAsync(new GetAllBarbeiroRequest
                {
                    FilialId = filialId,
                    PageNumber = 1,
                    PageSize = 100,
                    ApenasAtivos = true
                });

                if (result.IsSuccess && result.Data != null)
                {
                    Barbeiros = result.Data.ToList();
                    if (!InputModel.BarbeiroId.HasValue || InputModel.BarbeiroId.Value == 0)
                    {
                        var primeiro = Barbeiros.FirstOrDefault();
                        if (primeiro != null)
                        {
                            InputModel.BarbeiroId = primeiro.Id;
                        }
                    }
                }
            }
            catch
            {
                Barbeiros = new();
            }
        }

        private async Task CarregarDiasFechadosAsync()
        {
            try
            {
                var result = await DiaFechadoHandler.GetAllAsync(new GetAllDiasFechadosRequest
                {
                    PageNumber = 1,
                    PageSize = 100
                });

                if (result.IsSuccess && result.Data != null)
                {
                    DatasFechadas = result.Data.Select(d => d.Data.Date).ToHashSet();
                }
            }
            catch
            {
                DatasFechadas = new();
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

                if (result.Data != null)
                {
                    horariosOcupados = result.Data
                        .Where(x => x.Data.Date == data.Date
                            && x.Status != "Cancelado"
                            && x.Status != EStatusAgendamento.Cancelado.ToString()
                            && (!InputModel.BarbeiroId.HasValue || x.BarbeiroId == null || x.BarbeiroId == InputModel.BarbeiroId.Value))
                        .Select(x => new TimeSpan(x.Data.Hour, x.Data.Minute, 0))
                        .ToList();
                }

                GerarHorarios(data, horariosOcupados);
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

            var duracaoTotal = DuracaoTotalServicos > 0 ? DuracaoTotalServicos : 40;
            var intervalo = TimeSpan.FromMinutes(40);
            var horarioAtual = horarioAbertura;

            while (horarioAtual.Add(TimeSpan.FromMinutes(duracaoTotal)) <= horarioFechamento)
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
            if (!InputModel.FilialId.HasValue || InputModel.FilialId.Value == 0)
            {
                Snackbar.Add("Por favor, selecione uma filial.", Severity.Warning);
                return;
            }

            if (!InputModel.BarbeiroId.HasValue || InputModel.BarbeiroId.Value == 0)
            {
                Snackbar.Add("Por favor, selecione um barbeiro.", Severity.Warning);
                return;
            }

            if (!CortesSelecionadosIds.Any())
            {
                Snackbar.Add("Por favor, selecione pelo menos um serviço do catálogo.", Severity.Warning);
                return;
            }

            InputModel.CorteIds = CortesSelecionadosIds.ToList();
            InputModel.CorteId = CortesSelecionadosIds.First();

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

        #endregion
    }
}