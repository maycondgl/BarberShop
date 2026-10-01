namespace BarberShop.Core.Responses.DiasFechados
{
    public record DiaFechadoResponse(
        long Id,
        DateTime Data,
        string Motivo
    );
}
