using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Filiais
{
    public class CreateFilialRequest : Request
    {
        [Required(ErrorMessage = "O nome da filial é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A localização da filial é obrigatória")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "A localização deve ter entre 3 e 200 caracteres")]
        public string Localizacao { get; set; } = string.Empty;

        private string _telefone = string.Empty;

        [Required(ErrorMessage = "Informe o telefone")]
        [StringLength(11, MinimumLength = 10, ErrorMessage = "O telefone deve conter entre 10 e 11 dígitos")]
        [RegularExpression(
            @"^(?:1[1-9]|2[12478]|3[1-578]|4[1-9]|5[1345]|6[1-9]|7[134579]|8[1-9]|9[1-9])\d{8,9}$",
            ErrorMessage = "Informe um telefone válido com DDD do Brasil")]
        public string Telefone
        {
            get => _telefone;
            set => _telefone = System.Text.RegularExpressions.Regex.Replace(value ?? string.Empty, @"\D", "");
        }

        public bool Ativo { get; set; } = true;
    }
}
