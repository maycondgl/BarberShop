namespace BarberShop.Core.Responses.Corte;
    public record CorteResponse(
      long Id,
      string Titulo,
      Decimal Preco,
      int DuracaoMinutos,
      string Descricao,
      string ImagemUrl,
      bool Ativo

);
