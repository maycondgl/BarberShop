namespace BarberShop.Api;

public static class DatabaseConnectionSettings
{
    public static string Resolve(IConfiguration configuration)
    {
        var connectionString = new[]
        {
            configuration.GetConnectionString("Connection"),
            configuration.GetConnectionString("DefaultConnection"),
            configuration["SQLAZURECONNSTR_Connection"],
            configuration["SQLAZURECONNSTR_DefaultConnection"],
            configuration["CUSTOMCONNSTR_Connection"]
        }
        .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
        ?.Trim();

        return !string.IsNullOrWhiteSpace(connectionString)
            ? connectionString
            : throw new InvalidOperationException(
                "Conexão com o banco não configurada. Defina 'ConnectionStrings__Connection' ou 'ConnectionStrings__DefaultConnection'.");
    }
}
