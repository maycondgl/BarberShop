using System.ComponentModel.DataAnnotations;

namespace BarberShop.Core.Requests.Avaliacao
{
    public class UpdateAvaliacaoRequest : Request
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "A nota é obrigatória")]
        [Range(1, 5, ErrorMessage = "A nota deve ser entre 1 e 5 estrelas")]
        public int Estrelas { get; set; }

        [StringLength(255, ErrorMessage = "O comentário pode ter no máximo 255 caracteres")]
        public string Comentario { get; set; } = string.Empty;

        [Required(ErrorMessage = "O identificador do cliente é obrigatório")]
        [Range(1, long.MaxValue, ErrorMessage = "Cliente inválido")]
        public long ClienteId { get; set; }
    }
}
