using Microsoft.AspNetCore.Identity;

namespace BarberShop.Api.Models
{
    public class User : IdentityUser<long>
    {
        public string? NomeCompleto { get; set; }
        public bool Ativo { get; set; } = true;
        public List<IdentityRole<long>>? Roles { get; set; }
    }
}
