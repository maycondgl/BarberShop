using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Filiais;
using BarberShop.Core.Requests.HorariosFuncionamento;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarberShop.Web.Pages.Admin.Funcionamento
{
    public partial class FuncionamentoPage : ComponentBase
    {
        [Parameter]
        public long? FilialId { get; set; }

        [Inject]
        public IFilialHandler FilialHandler { get; set; } = null!;

        [Inject]
        public IHorarioFuncionamentoHandler HorarioHandler { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        public bool IsLoading { get; set; } = true;
        public bool IsSaving { get; set; } = false;

        public List<Filial> Filiais { get; set; } = [];
        public List<HorarioFuncionamentoDto> Horarios { get; set; } = [];

        public long? FilialSelecionadaId { get; set; }
        public Filial? FilialAtual => Filiais.FirstOrDefault(f => f.Id == FilialSelecionadaId);

        protected override async Task OnInitializedAsync()
        {
            await CarregarFiliaisAsync();

            if (FilialId.HasValue && Filiais.Any(f => f.Id == FilialId.Value))
            {
                FilialSelecionadaId = FilialId.Value;
            }
            else if (Filiais.Any())
            {
                FilialSelecionadaId = Filiais.First().Id;
            }

            if (FilialSelecionadaId.HasValue)
            {
                await CarregarHorariosAsync(FilialSelecionadaId.Value);
            }
            else
            {
                IsLoading = false;
            }
        }

        private async Task CarregarFiliaisAsync()
        {
            try
            {
                var request = new GetAllFilialRequest { PageNumber = 1, PageSize = 100 };
                var result = await FilialHandler.GetAllAsync(request);
                if (result.IsSuccess && result.Data != null)
                {
                    Filiais = result.Data.OrderBy(f => f.Nome).ToList();
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro ao carregar filiais: " + ex.Message, Severity.Error);
            }
        }

        public async Task OnFilialChangedAsync(long? novaFilialId)
        {
            if (novaFilialId == FilialSelecionadaId) return;

            FilialSelecionadaId = novaFilialId;
            if (FilialSelecionadaId.HasValue)
            {
                await CarregarHorariosAsync(FilialSelecionadaId.Value);
            }
        }

        private async Task CarregarHorariosAsync(long filialId)
        {
            IsLoading = true;
            try
            {
                var result = await HorarioHandler.GetByFilialAsync(filialId);
                if (result.IsSuccess && result.Data != null)
                {
                    Horarios = result.Data;
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Falha ao carregar horários da filial.", Severity.Warning);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro ao buscar horários: " + ex.Message, Severity.Error);
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        public async Task SalvarHorariosAsync()
        {
            if (!FilialSelecionadaId.HasValue)
            {
                Snackbar.Add("Selecione uma filial para salvar os horários.", Severity.Warning);
                return;
            }

            // Validações no cliente
            foreach (var h in Horarios.Where(x => x.Aberto))
            {
                if (h.HorarioFechamento <= h.HorarioAbertura)
                {
                    Snackbar.Add($"No dia {h.NomeDiaSemana}, o horário de fechamento deve ser maior que o de abertura.", Severity.Warning);
                    return;
                }

                if (h.TemAlmoco)
                {
                    if (!h.AlmocoInicio.HasValue || !h.AlmocoFim.HasValue)
                    {
                        Snackbar.Add($"No dia {h.NomeDiaSemana}, preencha o início e fim do almoço.", Severity.Warning);
                        return;
                    }

                    if (h.AlmocoFim <= h.AlmocoInicio)
                    {
                        Snackbar.Add($"No dia {h.NomeDiaSemana}, o término do almoço deve ser maior que o início.", Severity.Warning);
                        return;
                    }

                    if (h.AlmocoInicio < h.HorarioAbertura || h.AlmocoFim > h.HorarioFechamento)
                    {
                        Snackbar.Add($"No dia {h.NomeDiaSemana}, o horário de almoço deve estar dentro do horário de funcionamento.", Severity.Warning);
                        return;
                    }
                }
            }

            IsSaving = true;
            try
            {
                var request = new SalvarHorariosFilialRequest
                {
                    FilialId = FilialSelecionadaId.Value,
                    Horarios = Horarios
                };

                var result = await HorarioHandler.SaveHorariosAsync(request);
                if (result.IsSuccess)
                {
                    Snackbar.Add("Horários de funcionamento e almoço salvos com sucesso!", Severity.Success);
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Falha ao salvar horários.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro ao salvar: " + ex.Message, Severity.Error);
            }
            finally
            {
                IsSaving = false;
                StateHasChanged();
            }
        }
    }
}
