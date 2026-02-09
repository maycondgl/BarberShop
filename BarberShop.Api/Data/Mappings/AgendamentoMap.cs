using BarberShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Api.Data.Mappings
{
    public class AgendamentoMap : IEntityTypeConfiguration<Agendamento>
    {
        public void Configure(
            EntityTypeBuilder<Agendamento> builder)
        {
            builder.ToTable("Agendamento");

            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Cliente)
                .WithMany()
                .HasForeignKey(x => x.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Corte)
                .WithMany()
                .HasForeignKey(x => x.CorteId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Property(x => x.Tempo)
                .IsRequired();

            builder.Property(x => x.Valor)
                .HasColumnName("Valor")
                .IsRequired()
                .HasColumnType("DECIMAL(10,2)");

            builder.Property(x => x.Data)
                .IsRequired()
                .HasColumnType("DATETIME2");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasColumnType("NVARCHAR")
                .HasMaxLength(20);

        }
    }
}
