namespace BarberShop.Core.Responses.Filial
{
    public record FilialResponse(
        long Id,
        string Nome,
        string Localizacao,
        string Telefone,
        bool Ativo
    );
}
