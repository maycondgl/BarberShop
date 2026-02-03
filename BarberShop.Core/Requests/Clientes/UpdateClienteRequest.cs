using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Clientes
{
    public class UpdateClienteRequest : Request
    {
        [Required(ErrorMessage = "Nome Obrigatório!")]
        [StringLength(60, ErrorMessage = "O nome pode ter no máximo 60 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone é obrigatório")]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Telefone deve ter 10 ou 11 dígitos (apenas números)")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "UserId inválido")]
        public string UserId { get; set; } = string.Empty;
    }
}
