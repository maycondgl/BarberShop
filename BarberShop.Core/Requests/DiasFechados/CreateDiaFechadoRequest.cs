using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.DiasFechados
{
    public class CreateDiaFechadoRequest
    {
        [Required(ErrorMessage = "A data é obrigatória")]
        public DateTime Data { get; set; }

        [MaxLength(150, ErrorMessage = "O motivo deve conter no máximo 150 caracteres")]
        public string Motivo { get; set; } = string.Empty;
    }
}
