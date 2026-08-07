using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Licitaciones.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260807150000_HU43LicitacionLifecycleColumns")]
public sealed class HU43LicitacionLifecycleColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE licitaciones
                ADD COLUMN IF NOT EXISTS publicada_en timestamp with time zone NULL;
            ALTER TABLE licitaciones
                ADD COLUMN IF NOT EXISTS cerrada_en timestamp with time zone NULL;
            ALTER TABLE licitaciones
                ADD COLUMN IF NOT EXISTS motivo_cierre character varying(500) NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE licitaciones DROP COLUMN IF EXISTS motivo_cierre;
            ALTER TABLE licitaciones DROP COLUMN IF EXISTS cerrada_en;
            ALTER TABLE licitaciones DROP COLUMN IF EXISTS publicada_en;
            """);
    }
}
