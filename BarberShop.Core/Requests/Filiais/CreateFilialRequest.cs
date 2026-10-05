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

        public string Telefone { get; set; } = string.Empty;

        public bool Ativo { get; set; } = true;
    }
}
