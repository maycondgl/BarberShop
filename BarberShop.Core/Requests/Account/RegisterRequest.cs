using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Account
{
    public class RegisterRequest : Request
    {
        [Required(ErrorMessage ="Nome inválido")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o telefone")]
        [StringLength(11, MinimumLength = 10, ErrorMessage = "O telefone deve conter entre 10 e 11 dígitos")]
        [RegularExpression(
            @"^(?:1[1-9]|2[12478]|3[1-578]|4[1-9]|5[1345]|6[1-9]|7[134579]|8[1-9]|9[1-9])\d{8,9}$",
            ErrorMessage = "Informe um telefone válido com DDD do Brasil")]
        public string Telefone { get; set; } = string.Empty;

        private string _email = string.Empty;

        [Required(ErrorMessage = "Informe o e-mail")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [RegularExpression(
            @"(?i)^[^@\s]+@(gmail\.com|hotmail\.com|outlook\.com(\.br)?|yahoo\.com(\.br)?|icloud\.com|live\.com|uol\.com\.br|bol\.com\.br|barbershop\.com)$",
            ErrorMessage = "Informe um e-mail com provedor válido (ex: @gmail.com, @hotmail.com ou @outlook.com)")]
        public string Email
        {
            get => _email;
            set => _email = value?.Trim().ToLowerInvariant() ?? string.Empty;
        }

        [Required(ErrorMessage = "Informe a senha")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{6,}$",
            ErrorMessage = "A senha deve ter no mínimo 6 caracteres, 1 letra maiúscula, 1 minúscula, 1 número e 1 caractere especial")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme sua senha")]
        [Compare(nameof(Senha), ErrorMessage = "As senhas não coincidem")]
        public string ConfirmarSenha { get; set; } = string.Empty;

        public string? ChaveAdmin { get; set; }
    }
}
