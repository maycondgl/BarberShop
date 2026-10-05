using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Barbeiros
{
    public class CreateBarbeiroRequest : Request
    {
        private string _email = string.Empty;

        [Required(ErrorMessage = "O e-mail do usuário cadastrado é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [RegularExpression(
            @"(?i)^[^@\s]+@(gmail\.com|hotmail\.com|outlook\.com(\.br)?|yahoo\.com(\.br)?|icloud\.com|live\.com|uol\.com\.br|bol\.com\.br|barbershop\.com)$",
            ErrorMessage = "Informe um e-mail com provedor válido (ex: @gmail.com, @hotmail.com ou @outlook.com)")]
        public string Email
        {
            get => _email;
            set => _email = value?.Trim().ToLowerInvariant() ?? string.Empty;
        }

        [Required(ErrorMessage = "O nome do barbeiro é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        public string FotoUrl { get; set; } = string.Empty;

        public long? FilialId { get; set; }

        public bool Ativo { get; set; } = true;
        public long? UsuarioId { get; set; }
    }
}
