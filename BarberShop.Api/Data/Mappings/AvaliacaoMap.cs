using BarberShop.Api.Models;
using BarberShop.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Api.Data.Mappings
{
    public class AvaliacaoMap : IEntityTypeConfiguration<Avaliacao>
    {
        public void Configure(
            EntityTypeBuilder<Avaliacao> builder)
        {
            builder.ToTable("Avaliacao");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Estrelas)
                .IsRequired();
                
            builder.Property(x => x.Comentario)
                .HasMaxLength(500);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Data)
                .IsRequired()
                .HasColumnType("DATETIME2");

        }
    }
 }
