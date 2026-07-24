using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

internal sealed class OfertaConfiguration : IEntityTypeConfiguration<Oferta>
{
    public void Configure(EntityTypeBuilder<Oferta> builder)
    {
        builder.ToTable("ofertas", table =>
            table.HasCheckConstraint(
                "ck_ofertas_monto_positivo",
                "monto_ofertado_crc > 0"));

        builder.HasKey(x => x.Id).HasName("pk_ofertas");
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.LicitacionId)
            .HasColumnName("licitacion_id");
        builder.Property(x => x.ProveedorId)
            .HasColumnName("proveedor_id");
        builder.Property(x => x.MontoOfertadoCrc)
            .HasColumnName("monto_ofertado_crc")
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.FechaRegistro)
            .HasColumnName("fecha_registro")
            .HasDefaultValueSql("now()");

        builder.ConfigureAuditProperties(includeCreatedAt: false);

        builder.HasOne(x => x.Licitacion)
            .WithMany()
            .HasForeignKey(x => x.LicitacionId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_ofertas_licitacion");
        builder.HasOne(x => x.Proveedor)
            .WithMany(x => x.Ofertas)
            .HasForeignKey(x => x.ProveedorId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_ofertas_proveedor");

        builder.HasIndex(x => new { x.LicitacionId, x.ProveedorId })
            .IsUnique()
            .HasDatabaseName("ux_ofertas_licitacion_proveedor");
        builder.HasIndex(x => x.LicitacionId)
            .HasDatabaseName("ix_ofertas_licitacion_id");
        builder.HasIndex(x => x.ProveedorId)
            .HasDatabaseName("ix_ofertas_proveedor_id");
        builder.HasIndex(x => new { x.LicitacionId, x.MontoOfertadoCrc, x.FechaRegistro })
            .HasDatabaseName("ix_ofertas_licitacion_monto_fecha");
    }
}
