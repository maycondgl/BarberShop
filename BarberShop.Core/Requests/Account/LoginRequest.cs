using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Account
{
    public class LoginRequest : Request
    {
        [Required(ErrorMessage = "Email inválido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha inválida")]
        public string Senha { get; set; } = string.Empty;
    }
}
