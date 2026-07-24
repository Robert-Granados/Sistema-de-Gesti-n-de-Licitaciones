using Licitaciones.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

internal sealed class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.ToTable("proveedores", table =>
        {
            table.HasCheckConstraint(
                "ck_proveedores_nombre_caracteres",
                "nombre ~ '^[[:alnum:][:space:].,()]+$'");
            table.HasCheckConstraint(
                "ck_proveedores_nombre_no_vacio",
                "length(trim(nombre)) > 0");
        });

        builder.HasKey(x => x.Id).HasName("pk_proveedores");
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(x => x.NombreNormalizado)
            .HasColumnName("nombre_normalizado")
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(x => x.EliminadoEn)
            .HasColumnName("deleted_at");
        builder.Ignore(x => x.EstaEliminado);
        builder.Navigation(x => x.Ofertas)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.ConfigureAuditProperties();

        builder.HasIndex(x => x.NombreNormalizado)
            .IsUnique()
            .HasDatabaseName("ux_proveedores_nombre_normalizado")
            .HasFilter("deleted_at IS NULL");
        builder.HasIndex(x => x.EliminadoEn)
            .HasDatabaseName("ix_proveedores_deleted_at");
    }
}
