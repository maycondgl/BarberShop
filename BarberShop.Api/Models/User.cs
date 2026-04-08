using Microsoft.AspNetCore.Identity;

namespace BarberShop.Api.Models
{
    public class User : IdentityUser<long>

    {
        public bool Ativo { get; set; } = true;
        public string NomeCompleto { get; set; } = string.Empty;

    }
}
