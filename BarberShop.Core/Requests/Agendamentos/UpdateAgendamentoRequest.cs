using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Agendamentos
{
    public class UpdateAgendamentoRequest
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        [Required(ErrorMessage = "O corte é obrigatório")]
        [Range(1, long.MaxValue, ErrorMessage = "ID do corte inválido")]
        public long CorteId { get; set; }

        [Required(ErrorMessage = "A data e hora são obrigatórias")]
        [DataType(DataType.DateTime)]
        public DateTime Data { get; set; }
    }
}
