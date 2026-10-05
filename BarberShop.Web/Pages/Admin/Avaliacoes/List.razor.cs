using BarberShop.Core.Handlers;
using BarberShop.Core.Responses.Avaliacao;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BarberShop.Web.Pages.Admin.Avaliacoes
{
    public class AdminAvaliacoesPage : ComponentBase
    {
        [Inject] public IAvaliacaoHandler Handler { get; set; } = null!;
        [Inject] public ISnackbar Snackbar { get; set; } = null!;

        public List<AvaliacaoResponse> Avaliacoes { get; set; } = [];
        public bool IsBusy { get; set; } = true;
        public string BuscaTexto { get; set; } = string.Empty;
        public int? FiltroEstrelas { get; set; } = null;

        public double MediaGeral => Avaliacoes.Any() ? Avaliacoes.Average(a => a.Estrelas) : 5.0;
        public int TotalComComentario => Avaliacoes.Count(a => !string.IsNullOrWhiteSpace(a.Comentario));

        protected override async Task OnInitializedAsync()
        {
            await CarregarAvaliacoesAsync();
        }

        public async Task CarregarAvaliacoesAsync()
        {
            IsBusy = true;
            try
            {
                var result = await Handler.GetAllAdminAsync(1, 100);
                if (result.IsSuccess && result.Data is not null)
                {
                    Avaliacoes = result.Data;
                }
                else
                {
                    Snackbar.Add(result.Message ?? "Falha ao carregar avaliações.", Severity.Error);
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

        public List<AvaliacaoResponse> ObterAvaliacoesFiltradas()
        {
            var query = Avaliacoes.AsEnumerable();

            if (FiltroEstrelas.HasValue && FiltroEstrelas.Value > 0)
            {
                query = query.Where(a => a.Estrelas == FiltroEstrelas.Value);
            }

            if (!string.IsNullOrWhiteSpace(BuscaTexto))
            {
                var termo = BuscaTexto.Trim().ToLowerInvariant();
                query = query.Where(a =>
                    (a.NomeCliente?.ToLowerInvariant().Contains(termo) ?? false) ||
                    (a.BarbeiroNome?.ToLowerInvariant().Contains(termo) ?? false) ||
                    (a.ServicoTitulo?.ToLowerInvariant().Contains(termo) ?? false) ||
                    (a.Comentario?.ToLowerInvariant().Contains(termo) ?? false)
                );
            }

            return query.ToList();
        }
    }
}
