using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Agendamentos
{
    public class UpdateAgendamentoRequest : Request
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "O cliente é obrigatório")]
        [Range(1, long.MaxValue, ErrorMessage = "ID do cliente inválido")]
        public long ClienteId { get; set; }

        [Required(ErrorMessage = "O corte é obrigatório")]
        [Range(1, long.MaxValue, ErrorMessage = "ID do corte inválido")]
        public long CorteId { get; set; }

        [Required(ErrorMessage = "A data e hora são obrigatórias")]
        [DataType(DataType.DateTime)]
        [Range(typeof(DateTime), "2024-01-01", "2100-12-31",
        ErrorMessage = "Data inválida")]
        public DateTime Data { get; set; }
    }
}
