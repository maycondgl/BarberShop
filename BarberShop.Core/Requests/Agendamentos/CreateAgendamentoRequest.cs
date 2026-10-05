using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Agendamentos
{
    public class CreateAgendamentoRequest
    {
        [Required(ErrorMessage = "O cliente é obrigatório")]
        [Range(1, long.MaxValue, ErrorMessage = "ID do cliente inválido")]
        public long UserId { get; set; }

        [Required(ErrorMessage = "O corte é obrigatório")]
        [Range(1, long.MaxValue, ErrorMessage = "ID do corte inválido")]
        public long CorteId { get; set; }

        public List<long> CorteIds { get; set; } = new();

        [Required(ErrorMessage = "A filial é obrigatória")]
        [Range(1, long.MaxValue, ErrorMessage = "ID da filial inválido")]
        public long? FilialId { get; set; }

        [Required(ErrorMessage = "O barbeiro é obrigatório")]
        [Range(1, long.MaxValue, ErrorMessage = "ID do barbeiro inválido")]
        public long? BarbeiroId { get; set; }

        [Required(ErrorMessage = "A data e hora são obrigatórias")]
        [DataType(DataType.DateTime)]
        public DateTime Data { get; set; }

    }
}
