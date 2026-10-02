using System.Globalization;
using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.DiasFechados;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Admin
{
    public partial class DiasFechadosPage : ComponentBase
    {
        #region Properties

        protected List<DiaFechado> _diasFechados = new();
        protected DateTime? _novaData = DateTime.Today;
        protected string _novoMotivo = string.Empty;
        protected bool _isLoading = true;
        protected bool _isBusy = false;

        #endregion

        #region Services

        [Inject]
        public IDiaFechadoHandler Handler { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public IDialogService DialogService { get; set; } = null!;

        #endregion

        #region Override

        protected override async Task OnInitializedAsync()
        {
            await CarregarDiasFechadosAsync();
        }

        #endregion

        #region Methods

        protected async Task CarregarDiasFechadosAsync()
        {
            _isLoading = true;
            try
            {
                var result = await Handler.GetAllAsync(new GetAllDiasFechadosRequest { PageNumber = 1, PageSize = 100 });
                if (result.IsSuccess && result.Data != null)
                {
                    _diasFechados = result.Data.OrderBy(d => d.Data).ToList();
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro ao carregar dias fechados: " + ex.Message, Severity.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        protected async Task CadastrarDiaFechadoAsync()
        {
            if (_novaData == null)
            {
                Snackbar.Add("Por favor, selecione uma data.", Severity.Warning);
                return;
            }

            if (_novaData.Value.Date < DateTime.Today)
            {
                Snackbar.Add("Não é possível bloquear uma data no passado.", Severity.Warning);
                return;
            }

            _isBusy = true;
            try
            {
                var request = new CreateDiaFechadoRequest
                {
                    Data = _novaData.Value.Date,
                    Motivo = _novoMotivo?.Trim() ?? string.Empty
                };

                var result = await Handler.CreateAsync(request);
                if (result.IsSuccess)
                {
                    Snackbar.Add("Data bloqueada com sucesso!", Severity.Success);
                    _novoMotivo = string.Empty;
                    await CarregarDiasFechadosAsync();
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Falha ao bloquear data.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro: " + ex.Message, Severity.Error);
            }
            finally
            {
                _isBusy = false;
            }
        }

        protected async Task RemoverDiaFechadoAsync(long id, DateTime data)
        {
            bool? confirm = await DialogService.ShowMessageBox(
                "Desbloquear Data",
                $"Tem certeza que deseja reabrir a data {data:dd/MM/yyyy} para agendamentos?",
                yesText: "Sim, Desbloquear", cancelText: "Cancelar");

            if (confirm != true)
                return;

            _isBusy = true;
            try
            {
                var result = await Handler.DeleteAsync(id);
                if (result.IsSuccess)
                {
                    Snackbar.Add("Data desbloqueada com sucesso!", Severity.Success);
                    _diasFechados.RemoveAll(d => d.Id == id);
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Falha ao desbloquear data.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add("Erro: " + ex.Message, Severity.Error);
            }
            finally
            {
                _isBusy = false;
            }
        }

        #endregion
    }
}
