using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Account
{
    public class UpdateProfileRequest : Request
    {
        [Required(ErrorMessage = "Nome inválido")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o telefone")]
        [StringLength(11, MinimumLength = 10, ErrorMessage = "O telefone deve conter entre 10 e 11 dígitos")]
        [RegularExpression(
            @"^(?:1[1-9]|2[12478]|3[1-578]|4[1-9]|5[1345]|6[1-9]|7[134579]|8[1-9]|9[1-9])\d{8,9}$",
            ErrorMessage = "Informe um telefone válido com DDD do Brasil")]
        public string Telefone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email inválido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;
    }
}