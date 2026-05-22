using BarberShop.Core.Handlers;
using BarberShop.Core.Models;
using BarberShop.Core.Requests.Cortes;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Admin.Cortes
{
    public partial class ListPageCorte : ComponentBase
    {
        #region Properties

        public bool IsBusy { get; set; }
        public List<Corte> ListaCortes { get; set; } = [];
        public string SearchTerm { get; set; } = String.Empty;

        #endregion

        #region Services

        [Inject]
        public ICorteHandler CorteHandler { get; set; } = null!;

        [Inject]
        public ISnackbar Snackbar { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;


        #endregion

        #region Override

        protected override async Task OnInitializedAsync()
        {
            await LoadCortesAsync();
        }

        #endregion

        #region Methods

        public async Task LoadCortesAsync()
        {
            try
            {
                IsBusy = true;

                var result = await CorteHandler.GetAllAsync(new GetAllCorteRequest
                {
                    PageNumber = 1,
                    PageSize = 50
                });

                if (result.IsSuccess && result.Data is not null)
                {
                    ListaCortes = result.Data.ToList();
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Erro ao carregar cortes", Severity.Error);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task OnCreateClicked()
        {
            NavigationManager.NavigateTo("/admin/cortes/create");
        }

        public async Task OnEditClicked()
        {
            NavigationManager.NavigateTo("/admin/cortes/editar/{id}");
        }

        public async Task OnDeleteClickedAsync(long id)
        {
            var result = await CorteHandler.DeleteAsync(id);

            if (result.IsSuccess)
            {
                ListaCortes.RemoveAll(x => x.Id == id);

                Snackbar.Add("Corte removido com sucesso!", Severity.Success);
            }
            else
            {
                Snackbar.Add(result.Message ?? "Erro ao remover corte", Severity.Error);
            }
        }

        public Func<Corte, bool> Filter => corte =>
        {
            if (string.IsNullOrWhiteSpace(SearchTerm))
                return true;
            return corte.Titulo.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase);
        };

        #endregion
    }
}
