using BarberShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Api.Data.Mappings
{
    public class HorarioFuncionamentoMap : IEntityTypeConfiguration<HorarioFuncionamento>
    {
        public void Configure(EntityTypeBuilder<HorarioFuncionamento> builder)
        {
            builder.ToTable("HorarioFuncionamento");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FilialId)
                .IsRequired();

            builder.Property(x => x.DiaSemana)
                .IsRequired();

            builder.Property(x => x.Aberto)
                .IsRequired();

            builder.Property(x => x.HorarioAbertura)
                .IsRequired();

            builder.Property(x => x.HorarioFechamento)
                .IsRequired();

            builder.Property(x => x.TemAlmoco)
                .IsRequired();

            builder.Property(x => x.AlmocoInicio)
                .IsRequired(false);

            builder.Property(x => x.AlmocoFim)
                .IsRequired(false);

            builder.HasOne(x => x.Filial)
                .WithMany()
                .HasForeignKey(x => x.FilialId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
