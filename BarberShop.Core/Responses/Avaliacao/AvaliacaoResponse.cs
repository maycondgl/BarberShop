namespace BarberShop.Core.Responses.Avaliacao
{
   public record AvaliacaoResponse(
   long Id,
   long ClienteId,
   int Estrelas,
   string? Comentario,
   DateTime Data
);
}
