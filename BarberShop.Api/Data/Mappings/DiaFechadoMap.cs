using BarberShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Api.Data.Mappings
{
    public class DiaFechadoMap : IEntityTypeConfiguration<DiaFechado>
    {
        public void Configure(EntityTypeBuilder<DiaFechado> builder)
        {
            builder.ToTable("DiaFechado");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Data)
                .IsRequired()
                .HasColumnType("DATETIME2");

            builder.Property(x => x.Motivo)
                .IsRequired(false)
                .HasColumnType("NVARCHAR")
                .HasMaxLength(150);
        }
    }
}
