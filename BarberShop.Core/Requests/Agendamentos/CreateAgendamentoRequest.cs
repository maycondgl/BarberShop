using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Clientes
{
    public class CreateAgendamentoRequest : Request
    {
        [Required(ErrorMessage = "O cliente é obrigatório")]
        [Range(1, long.MaxValue, ErrorMessage = "ID do cliente inválido")]
        public long ClienteId { get; set; }

        [Required(ErrorMessage = "O corte é obrigatório")]
        [Range(1, long.MaxValue, ErrorMessage = "ID do corte inválido")]
        public long CorteId { get; set; }

        [Required(ErrorMessage = "A data e hora são obrigatórias")]
        [DataType(DataType.DateTime)]
        public DateTime Data { get; set; }

    }
}
