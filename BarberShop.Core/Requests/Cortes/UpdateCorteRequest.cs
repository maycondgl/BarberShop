using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Clientes
{
    public class UpdateCorteRequest : Request
    {
        [Required(ErrorMessage = "O título do corte é obrigatório")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "O título deve ter entre 3 e 80 caracteres")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O preço é obrigatório")]
        [Range(0.01, 10000.00, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A duração em minutos é obrigatória")]
        [Range(1, 480, ErrorMessage = "A duração deve ser entre 1 minuto e 8 horas")]
        public int DuracaoMinutos { get; set; }

        [Required(ErrorMessage = "A Role é obrigatória")]
        public string Role { get; set; } = string.Empty;
    }
}
