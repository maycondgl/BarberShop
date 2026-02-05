using BarberShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BarberShop.Api.Data
{
    public class BarberShopContext(DbContextOptions<BarberShopContext> options) 
        : DbContext(options)
    {
        public DbSet<Agendamento> Agendamentos { get; set; } = null!;
        public DbSet<Avaliacao> Avaliacoes { get; set; } = null!;
        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Corte> Cortes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }

    }
}
