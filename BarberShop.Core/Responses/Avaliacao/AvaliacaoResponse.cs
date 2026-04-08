namespace BarberShop.Core.Responses.Avaliacao
{
   public record AvaliacaoResponse(
   long Id,
   long UserId,
   int Estrelas,
   string? Comentario,
   DateTime Data
);
}
