using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Licitaciones.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260807120000_HU40AuditCreatedAtImmutable")]
public sealed class HU40AuditCreatedAtImmutable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION fn_preserve_created_at()
            RETURNS TRIGGER AS $$
            BEGIN
                NEW.created_at := OLD.created_at;
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            CREATE OR REPLACE FUNCTION fn_set_deleted_at()
            RETURNS TRIGGER AS $$
            BEGIN
                IF OLD.deleted_at IS NULL AND NEW.deleted_at IS NOT NULL THEN
                    NEW.deleted_at := now();
                END IF;
                RETURN NEW;
            END;
            $$ LANGUAGE plpgsql;

            CREATE TRIGGER trg_proveedores_created_at_inmutable
                BEFORE UPDATE ON proveedores
                FOR EACH ROW EXECUTE FUNCTION fn_preserve_created_at();
            CREATE TRIGGER trg_proveedores_deleted_at
                BEFORE UPDATE OF deleted_at ON proveedores
                FOR EACH ROW EXECUTE FUNCTION fn_set_deleted_at();

            CREATE TRIGGER trg_licitaciones_created_at_inmutable
                BEFORE UPDATE ON licitaciones
                FOR EACH ROW EXECUTE FUNCTION fn_preserve_created_at();
            CREATE TRIGGER trg_licitaciones_deleted_at
                BEFORE UPDATE OF deleted_at ON licitaciones
                FOR EACH ROW EXECUTE FUNCTION fn_set_deleted_at();

            CREATE TRIGGER trg_niveles_aprobacion_created_at_inmutable
                BEFORE UPDATE ON niveles_aprobacion
                FOR EACH ROW EXECUTE FUNCTION fn_preserve_created_at();
            CREATE TRIGGER trg_tipos_cambio_created_at_inmutable
                BEFORE UPDATE ON tipos_cambio
                FOR EACH ROW EXECUTE FUNCTION fn_preserve_created_at();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP TRIGGER IF EXISTS trg_tipos_cambio_created_at_inmutable ON tipos_cambio;
            DROP TRIGGER IF EXISTS trg_niveles_aprobacion_created_at_inmutable ON niveles_aprobacion;
            DROP TRIGGER IF EXISTS trg_licitaciones_deleted_at ON licitaciones;
            DROP TRIGGER IF EXISTS trg_licitaciones_created_at_inmutable ON licitaciones;
            DROP TRIGGER IF EXISTS trg_proveedores_deleted_at ON proveedores;
            DROP TRIGGER IF EXISTS trg_proveedores_created_at_inmutable ON proveedores;
            DROP FUNCTION IF EXISTS fn_set_deleted_at();
            DROP FUNCTION IF EXISTS fn_preserve_created_at();
            """);
    }
}
