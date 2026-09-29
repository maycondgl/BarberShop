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
                var result = await Handler.GetAllAsync(new GetAllAgendamentoRequest());
                var horariosOcupados = new List<TimeSpan>();

                if (result.IsSuccess && result.Data != null)
                {
                    horariosOcupados = result.Data
                        .Where(x => x.Data.Date == data.Date && x.Status != EStatusAgendamento.Cancelado)
                        .Select(x => x.Data.TimeOfDay)
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

        public async Task OnValidSubmitAsync(EditContext context)
        {
            if (InputModel.CorteId == 0)
            {
                Snackbar.Add("Selecione um tipo de corte", Severity.Warning);
                return;
            }
            if (DataSelecionada == null)
            {
                Snackbar.Add("Selecione uma data", Severity.Warning);
                return;
            }
            if (HorarioSelecionado == null)
            {
                Snackbar.Add("Selecione um horário disponível nas opções", Severity.Warning);
                return;
            }

            // Agora juntamos a data e a hora corretas antes de enviar para a API
            InputModel.Data = DataSelecionada.Value.Date + HorarioSelecionado.Value;
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