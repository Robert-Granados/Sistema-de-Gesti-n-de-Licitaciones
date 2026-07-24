using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

internal sealed class TipoCambioConfiguration : IEntityTypeConfiguration<TipoCambio>
{
    private static readonly DateTimeOffset SeedDate =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<TipoCambio> builder)
    {
        builder.ToTable("tipos_cambio", table =>
            table.HasCheckConstraint(
                "ck_tipos_cambio_valor_positivo",
                "crc_por_usd > 0"));

        builder.HasKey(x => x.Id).HasName("pk_tipos_cambio");
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.CrcPorUsd)
            .HasColumnName("crc_por_usd")
            .HasColumnType("numeric(18,6)");
        builder.Property(x => x.FechaVigencia)
            .HasColumnName("fecha_vigencia");
        builder.Property(x => x.Activo)
            .HasColumnName("activo")
            .HasDefaultValue(false);

        builder.ConfigureAuditProperties();

        builder.HasIndex(x => x.Activo)
            .IsUnique()
            .HasDatabaseName("ux_tipos_cambio_unico_activo")
            .HasFilter("activo");
        builder.HasIndex(x => x.FechaVigencia)
            .IsDescending()
            .HasDatabaseName("ix_tipos_cambio_fecha_vigencia");

        builder.HasData(new
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
            CrcPorUsd = 520.000000m,
            FechaVigencia = SeedDate,
            Activo = true,
            CreatedAt = SeedDate,
            UpdatedAt = SeedDate,
            RowVersion = 0
        });
    }
}
