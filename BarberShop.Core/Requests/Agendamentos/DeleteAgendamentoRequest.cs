using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Agendamentos
{
    public class DeleteAgendamentoRequest
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "O cliente é obrigatório")]
        [Range(1, long.MaxValue, ErrorMessage = "ID do cliente inválido")]
        public long ClienteId { get; set; }

        [Required(ErrorMessage = "O corte é obrigatório")]
        [Range(1, long.MaxValue, ErrorMessage = "ID do corte inválido")]
        public long CorteId { get; set; }

    }
}
