using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

internal sealed class NivelAprobacionConfiguration
    : IEntityTypeConfiguration<NivelAprobacion>
{
    private static readonly DateTimeOffset SeedDate =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<NivelAprobacion> builder)
    {
        builder.ToTable("niveles_aprobacion", table =>
        {
            table.HasCheckConstraint(
                "ck_niveles_monto_minimo_no_negativo",
                "monto_minimo_crc >= 0");
            table.HasCheckConstraint(
                "ck_niveles_monto_maximo_mayor_minimo",
                "monto_maximo_crc IS NULL OR monto_maximo_crc > monto_minimo_crc");
            table.HasCheckConstraint(
                "ck_niveles_aprobador_no_vacio",
                "length(trim(aprobador)) > 0");
        });

        builder.HasKey(x => x.Id).HasName("pk_niveles_aprobacion");
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.MontoMinimoCrc)
            .HasColumnName("monto_minimo_crc")
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.MontoMaximoCrc)
            .HasColumnName("monto_maximo_crc")
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.Aprobador)
            .HasColumnName("aprobador")
            .HasMaxLength(150)
            .IsRequired();

        builder.ConfigureAuditProperties();

        builder.HasData(
            new
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                MontoMinimoCrc = 0.01m,
                MontoMaximoCrc = (decimal?)999_999.99m,
                Aprobador = "Encargado de área",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate,
                RowVersion = 0
            },
            new
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                MontoMinimoCrc = 1_000_000m,
                MontoMaximoCrc = (decimal?)9_999_999.99m,
                Aprobador = "Gerencia",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate,
                RowVersion = 0
            },
            new
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                MontoMinimoCrc = 10_000_000m,
                MontoMaximoCrc = (decimal?)null,
                Aprobador = "Junta Directiva",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate,
                RowVersion = 0
            });
    }
}

