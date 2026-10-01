namespace BarberShop.Core.Models
{
    public class DiaFechado
    {
        public long Id { get; set; }
        public DateTime Data { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }
}
