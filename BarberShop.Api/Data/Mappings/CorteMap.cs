using BarberShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Api.Data.Mappings
{
    public class CorteMap : IEntityTypeConfiguration<Corte>
    {
        public void Configure(
            EntityTypeBuilder<Corte> builder)
        {
            builder.ToTable("Corte");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Titulo)
                .IsRequired()
                .HasMaxLength(80);

            builder.Property(x => x.Preco)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.DuracaoMinutos)
                .IsRequired();

        }
    }
}
