using BarberShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Api.Data.Mappings
{
    public class BarbeiroMap : IEntityTypeConfiguration<Barbeiro>
    {
        public void Configure(EntityTypeBuilder<Barbeiro> builder)
        {
            builder.ToTable("Barbeiro");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.FotoUrl)
                .HasMaxLength(500);

            builder.HasOne(x => x.Filial)
                .WithMany()
                .HasForeignKey(x => x.FilialId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(x => x.Ativo)
                .IsRequired();
        }
    }
}
