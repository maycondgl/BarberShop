using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Barbeiros
{
    public class UpdateBarbeiroRequest : Request
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "O nome do barbeiro é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
        public string Nome { get; set; } = string.Empty;

        public string FotoUrl { get; set; } = string.Empty;

        public long? FilialId { get; set; }

        public bool Ativo { get; set; } = true;
    }
}
