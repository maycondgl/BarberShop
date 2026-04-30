namespace BarberShop.Core.Responses.Avaliacao
{
   public record AvaliacaoResponse(
   long Id,
   long UserId,
   long AgendamentoId,
   int Estrelas,
   string? Comentario,
   DateTime Data,
   string NomeCliente
);
}
