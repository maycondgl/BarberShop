namespace BarberShop.Core.Responses.Barbeiro
{
    public record BarbeiroResponse(
        long Id,
        string Nome,
        string FotoUrl,
        long? FilialId,
        string FilialNome,
        bool Ativo
    );
}
