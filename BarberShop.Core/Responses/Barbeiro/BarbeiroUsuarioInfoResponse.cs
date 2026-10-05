namespace BarberShop.Core.Responses.Barbeiro
{
    public record BarbeiroUsuarioInfoResponse(
        long Id,
        string Nome,
        string Email,
        string? Telefone,
        bool JaCadastradoComoBarbeiro,
        string? BarbeiroNome = null
    );
}
