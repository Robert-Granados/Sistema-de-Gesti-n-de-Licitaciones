using Licitaciones.Domain.Entities;
using Licitaciones.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

internal sealed class LicitacionConfiguration : IEntityTypeConfiguration<Licitacion>
{
    public void Configure(EntityTypeBuilder<Licitacion> builder)
    {
        builder.ToTable("licitaciones", table =>
        {
            table.HasCheckConstraint(
                "ck_licitaciones_presupuesto_positivo",
                "presupuesto_estimado_crc > 0");
            table.HasCheckConstraint(
                "ck_licitaciones_titulo_no_vacio",
                "length(trim(titulo)) > 0");
        });

        builder.HasKey(x => x.Id).HasName("pk_licitaciones");
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(50)
            .IsRequired();
        builder.Property<string>("CodigoNormalizado")
            .HasColumnName("codigo_normalizado")
            .HasMaxLength(50)
            .IsRequired()
            .ValueGeneratedOnAddOrUpdate();
        builder.Property(x => x.Titulo)
            .HasColumnName("titulo")
            .HasMaxLength(300)
            .IsRequired();
        builder.Property(x => x.Estado)
            .HasColumnName("estado")
            .HasColumnType("estado_licitacion")
            .HasDefaultValue(EstadoLicitacion.Borrador);
        builder.Property(x => x.FechaCierre)
            .HasColumnName("fecha_cierre");
        builder.Property(x => x.PresupuestoEstimadoCrc)
            .HasColumnName("presupuesto_estimado_crc")
            .HasColumnType("numeric(18,2)");
        builder.Property(x => x.PublicadaEn)
            .HasColumnName("publicada_en");
        builder.Property(x => x.CerradaEn)
            .HasColumnName("cerrada_en");
        builder.Property(x => x.MotivoCierre)
            .HasColumnName("motivo_cierre")
            .HasMaxLength(500);

        builder.ConfigureAuditProperties(includeDeletedAt: true);

        builder.HasIndex("CodigoNormalizado")
            .IsUnique()
            .HasDatabaseName("ux_licitaciones_codigo_normalizado")
            .HasFilter("deleted_at IS NULL");
        builder.HasIndex(x => x.Estado)
            .HasDatabaseName("ix_licitaciones_estado");
        builder.HasIndex(x => x.FechaCierre)
            .HasDatabaseName("ix_licitaciones_fecha_cierre");
        builder.HasIndex("DeletedAt")
            .HasDatabaseName("ix_licitaciones_deleted_at");
    }
}

