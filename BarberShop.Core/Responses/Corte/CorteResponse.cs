namespace BarberShop.Core.Responses.Corte;
    public record CorteResponse(
      long Id,
      long Titulo,
      long Preco,
      long DuracaoMinutos
);
