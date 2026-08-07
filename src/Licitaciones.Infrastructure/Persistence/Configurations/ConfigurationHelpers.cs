using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Licitaciones.Infrastructure.Persistence.Configurations;

internal static class ConfigurationHelpers
{
    public static void ConfigureAuditProperties<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        bool includeCreatedAt = true,
        bool includeDeletedAt = false)
        where TEntity : class
    {
        if (includeCreatedAt)
        {
            builder.Property<DateTimeOffset>("CreatedAt")
                .HasColumnName("created_at")
                .HasDefaultValueSql("now()")
                .ValueGeneratedNever();
        }

        builder.Property<DateTimeOffset>("UpdatedAt")
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now()")
            .ValueGeneratedNever();

        if (includeDeletedAt)
        {
            builder.Property<DateTimeOffset?>("DeletedAt")
                .HasColumnName("deleted_at");
        }

        builder.Property<int>("RowVersion")
            .HasColumnName("row_version")
            .HasDefaultValue(0)
            .IsRowVersion();
    }
}
