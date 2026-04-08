using BarberShop.Api.Models;
using BarberShop.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BarberShop.Api.Data
{
    public class BarberShopContext(DbContextOptions<BarberShopContext> options) 
        : IdentityDbContext
        <
            User, 
            IdentityRole<long>, 
            long,
            IdentityUserClaim<long>,
            IdentityUserRole<long>,
            IdentityUserLogin<long>,
            IdentityRoleClaim<long>,
            IdentityUserToken<long>
            >(options)
    {
        public DbSet<Agendamento> Agendamentos { get; set; } = null!;
        public DbSet<Avaliacao> Avaliacoes { get; set; } = null!;
        public DbSet<Corte> Cortes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }

    }
}
