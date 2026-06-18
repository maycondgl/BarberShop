using BarberShop.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarberShop.Api.Data.Mappings;

public class PushSubscriptionDeviceMap : IEntityTypeConfiguration<PushSubscriptionDevice>
{
    public void Configure(EntityTypeBuilder<PushSubscriptionDevice> builder)
    {
        builder.ToTable("PushSubscriptionDevice");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Endpoint)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.P256Dh)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Auth)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasColumnType("DATETIME2");

        builder.Property(x => x.UpdatedAt)
            .IsRequired()
            .HasColumnType("DATETIME2");

        builder.HasIndex(x => x.Endpoint)
            .IsUnique();
    }
}
