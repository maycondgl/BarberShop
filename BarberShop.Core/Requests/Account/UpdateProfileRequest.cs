using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Account
{
    public class UpdateProfileRequest : Request
    {
        [Required(ErrorMessage = "Nome inválido")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Telefone inválido")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email inválido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;
    }
}