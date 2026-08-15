using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Account
{
    public class RegisterRequest : Request
    {
        [Required(ErrorMessage ="Nome inválido")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o telefone")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "O telefone deve conter 11 dígitos")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "O telefone deve conter somente números")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha inválida")]
        [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
        public string Senha { get; set; } = string.Empty;

        public string? ChaveAdmin { get; set; }

    }
}
