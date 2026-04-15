using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Account
{
    public class RegisterRequest : Request
    {
        [Required(ErrorMessage ="Nome inválido")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone inválido")]
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "E-mail")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha inválida")]
        [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
        public string Senha { get; set; } = string.Empty;

    }
}
