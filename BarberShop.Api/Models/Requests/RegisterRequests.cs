namespace BarberShop.Api.Models.Requests
{
    public class RegisterRequests
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string NomeCompleto { get; set; } = string.Empty;
        public string NumeroTelefone { get; set; } = string.Empty;
    }
}
