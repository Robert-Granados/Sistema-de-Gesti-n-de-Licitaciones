using System;
using Licitaciones.Domain.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Licitaciones.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:estado_licitacion", "Borrador,Publicada,Cerrada")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:unaccent", ",,");

            migrationBuilder.CreateTable(
                name: "licitaciones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    titulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    estado = table.Column<EstadoLicitacion>(type: "estado_licitacion", nullable: false, defaultValue: EstadoLicitacion.Borrador),
                    fecha_cierre = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    presupuesto_estimado_crc = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    codigo_normalizado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    row_version = table.Column<int>(type: "integer", rowVersion: true, nullable: false, defaultValue: 0),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_licitaciones", x => x.id);
                    table.CheckConstraint("ck_licitaciones_presupuesto_positivo", "presupuesto_estimado_crc > 0");
                    table.CheckConstraint("ck_licitaciones_titulo_no_vacio", "length(trim(titulo)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "niveles_aprobacion",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    monto_minimo_crc = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    monto_maximo_crc = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    aprobador = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    row_version = table.Column<int>(type: "integer", rowVersion: true, nullable: false, defaultValue: 0),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_niveles_aprobacion", x => x.id);
                    table.CheckConstraint("ck_niveles_aprobador_no_vacio", "length(trim(aprobador)) > 0");
                    table.CheckConstraint("ck_niveles_monto_maximo_mayor_minimo", "monto_maximo_crc IS NULL OR monto_maximo_crc > monto_minimo_crc");
                    table.CheckConstraint("ck_niveles_monto_minimo_no_negativo", "monto_minimo_crc >= 0");
                });

            migrationBuilder.CreateTable(
                name: "proveedores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    nombre_normalizado = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    row_version = table.Column<int>(type: "integer", rowVersion: true, nullable: false, defaultValue: 0),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_proveedores", x => x.id);
                    table.CheckConstraint("ck_proveedores_nombre_caracteres", "nombre ~ '^[[:alnum:][:space:].,()]+$'");
                    table.CheckConstraint("ck_proveedores_nombre_no_vacio", "length(trim(nombre)) > 0");
                });

            migrationBuilder.CreateTable(
                name: "tipos_cambio",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    crc_por_usd = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    fecha_vigencia = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    row_version = table.Column<int>(type: "integer", rowVersion: true, nullable: false, defaultValue: 0),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tipos_cambio", x => x.id);
                    table.CheckConstraint("ck_tipos_cambio_valor_positivo", "crc_por_usd > 0");
                });

            migrationBuilder.CreateTable(
                name: "ofertas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    licitacion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proveedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    monto_ofertado_crc = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fecha_registro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    row_version = table.Column<int>(type: "integer", rowVersion: true, nullable: false, defaultValue: 0),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ofertas", x => x.id);
                    table.CheckConstraint("ck_ofertas_monto_positivo", "monto_ofertado_crc > 0");
                    table.ForeignKey(
                        name: "fk_ofertas_licitacion",
                        column: x => x.licitacion_id,
                        principalTable: "licitaciones",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ofertas_proveedor",
                        column: x => x.proveedor_id,
                        principalTable: "proveedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "niveles_aprobacion",
                columns: new[] { "id", "aprobador", "created_at", "monto_maximo_crc", "monto_minimo_crc" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "Encargado de área", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 999999.99m, 0.01m },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "Gerencia", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 9999999.99m, 1000000m },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "Junta Directiva", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, 10000000m }
                });

            migrationBuilder.InsertData(
                table: "tipos_cambio",
                columns: new[] { "id", "activo", "crc_por_usd", "created_at", "fecha_vigencia" },
                values: new object[] { new Guid("20000000-0000-0000-0000-000000000001"), true, 520.000000m, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "ix_licitaciones_deleted_at",
                table: "licitaciones",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_licitaciones_estado",
                table: "licitaciones",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "ix_licitaciones_fecha_cierre",
                table: "licitaciones",
                column: "fecha_cierre");

            migrationBuilder.CreateIndex(
                name: "ux_licitaciones_codigo_normalizado",
                table: "licitaciones",
                column: "codigo_normalizado",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_ofertas_licitacion_id",
                table: "ofertas",
                column: "licitacion_id");

            migrationBuilder.CreateIndex(
                name: "ix_ofertas_licitacion_monto_fecha",
                table: "ofertas",
                columns: new[] { "licitacion_id", "monto_ofertado_crc", "fecha_registro" });

            migrationBuilder.CreateIndex(
                name: "ix_ofertas_proveedor_id",
                table: "ofertas",
                column: "proveedor_id");

            migrationBuilder.CreateIndex(
                name: "ux_ofertas_licitacion_proveedor",
                table: "ofertas",
                columns: new[] { "licitacion_id", "proveedor_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_proveedores_deleted_at",
                table: "proveedores",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ux_proveedores_nombre_normalizado",
                table: "proveedores",
                column: "nombre_normalizado",
                unique: true,
                filter: "deleted_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_tipos_cambio_fecha_vigencia",
                table: "tipos_cambio",
                column: "fecha_vigencia",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ux_tipos_cambio_unico_activo",
                table: "tipos_cambio",
                column: "activo",
                unique: true,
                filter: "activo");

            migrationBuilder.Sql(
                """
                ALTER TABLE niveles_aprobacion
                    ADD COLUMN rango_monto numrange
                    GENERATED ALWAYS AS (
                        numrange(monto_minimo_crc, monto_maximo_crc, '[]')
                    ) STORED;

                ALTER TABLE niveles_aprobacion
                    ADD CONSTRAINT ex_niveles_rango_sin_traslape
                    EXCLUDE USING gist (rango_monto WITH &&);

                CREATE UNIQUE INDEX ux_niveles_aprobacion_unico_abierto
                    ON niveles_aprobacion ((monto_maximo_crc IS NULL))
                    WHERE monto_maximo_crc IS NULL;

                CREATE OR REPLACE FUNCTION fn_set_audit_fields()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.updated_at := now();
                    NEW.row_version := COALESCE(OLD.row_version, 0) + 1;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE OR REPLACE FUNCTION fn_normalizar_codigo_licitacion()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.codigo_normalizado := upper(trim(NEW.codigo));
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE OR REPLACE FUNCTION fn_normalizar_nombre_proveedor()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.nombre_normalizado :=
                        upper(unaccent(regexp_replace(trim(NEW.nombre), '\s+', ' ', 'g')));
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE OR REPLACE FUNCTION fn_validar_oferta()
                RETURNS TRIGGER AS $$
                DECLARE
                    v_estado estado_licitacion;
                    v_fecha_cierre timestamptz;
                    v_presupuesto numeric(18,2);
                    v_deleted_at timestamptz;
                BEGIN
                    SELECT estado, fecha_cierre, presupuesto_estimado_crc, deleted_at
                      INTO v_estado, v_fecha_cierre, v_presupuesto, v_deleted_at
                      FROM licitaciones
                     WHERE id = NEW.licitacion_id
                     FOR UPDATE;

                    IF NOT FOUND OR v_deleted_at IS NOT NULL THEN
                        RAISE EXCEPTION 'La licitación % no existe o fue eliminada',
                            NEW.licitacion_id USING ERRCODE = '23503';
                    END IF;

                    IF v_estado <> 'Publicada' THEN
                        RAISE EXCEPTION
                            'La licitación no está publicada (estado actual: %)',
                            v_estado USING ERRCODE = '22023';
                    END IF;

                    IF v_fecha_cierre <= now() THEN
                        RAISE EXCEPTION 'La licitación está vencida o cerrada'
                            USING ERRCODE = '22023';
                    END IF;

                    IF NEW.monto_ofertado_crc > v_presupuesto THEN
                        RAISE EXCEPTION
                            'El monto ofertado supera el presupuesto estimado'
                            USING ERRCODE = '22023';
                    END IF;

                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE OR REPLACE FUNCTION fn_bloquear_oferta_licitacion_cerrada()
                RETURNS TRIGGER AS $$
                DECLARE
                    v_estado estado_licitacion;
                    v_fecha_cierre timestamptz;
                    v_licitacion_id uuid;
                BEGIN
                    v_licitacion_id := COALESCE(NEW.licitacion_id, OLD.licitacion_id);

                    SELECT estado, fecha_cierre
                      INTO v_estado, v_fecha_cierre
                      FROM licitaciones
                     WHERE id = v_licitacion_id;

                    IF TG_OP = 'UPDATE' OR TG_OP = 'DELETE' THEN
                        IF v_estado = 'Cerrada' OR v_fecha_cierre <= now() THEN
                            RAISE EXCEPTION
                                'No se pueden modificar ofertas de licitaciones cerradas'
                                USING ERRCODE = '22023';
                        END IF;
                    END IF;

                    IF TG_OP = 'DELETE' THEN
                        RETURN OLD;
                    END IF;

                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE OR REPLACE FUNCTION fn_desactivar_tipos_cambio_previos()
                RETURNS TRIGGER AS $$
                BEGIN
                    IF NEW.activo THEN
                        UPDATE tipos_cambio
                           SET activo = false,
                               updated_at = now(),
                               row_version = row_version + 1
                         WHERE activo = true
                           AND id <> NEW.id;
                    END IF;

                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_proveedores_normalizar
                    BEFORE INSERT OR UPDATE OF nombre ON proveedores
                    FOR EACH ROW EXECUTE FUNCTION fn_normalizar_nombre_proveedor();

                CREATE TRIGGER trg_proveedores_audit
                    BEFORE UPDATE ON proveedores
                    FOR EACH ROW EXECUTE FUNCTION fn_set_audit_fields();

                CREATE TRIGGER trg_licitaciones_normalizar
                    BEFORE INSERT OR UPDATE OF codigo ON licitaciones
                    FOR EACH ROW EXECUTE FUNCTION fn_normalizar_codigo_licitacion();

                CREATE TRIGGER trg_licitaciones_audit
                    BEFORE UPDATE ON licitaciones
                    FOR EACH ROW EXECUTE FUNCTION fn_set_audit_fields();

                CREATE TRIGGER trg_ofertas_validar_negocio
                    BEFORE INSERT ON ofertas
                    FOR EACH ROW EXECUTE FUNCTION fn_validar_oferta();

                CREATE TRIGGER trg_ofertas_bloquear_si_cerrada
                    BEFORE UPDATE OR DELETE ON ofertas
                    FOR EACH ROW EXECUTE FUNCTION fn_bloquear_oferta_licitacion_cerrada();

                CREATE TRIGGER trg_ofertas_audit
                    BEFORE UPDATE ON ofertas
                    FOR EACH ROW EXECUTE FUNCTION fn_set_audit_fields();

                CREATE TRIGGER trg_niveles_aprobacion_audit
                    BEFORE UPDATE ON niveles_aprobacion
                    FOR EACH ROW EXECUTE FUNCTION fn_set_audit_fields();

                CREATE TRIGGER trg_tipos_cambio_desactivar_previos
                    BEFORE INSERT OR UPDATE OF activo ON tipos_cambio
                    FOR EACH ROW
                    WHEN (NEW.activo = true)
                    EXECUTE FUNCTION fn_desactivar_tipos_cambio_previos();

                CREATE TRIGGER trg_tipos_cambio_audit
                    BEFORE UPDATE ON tipos_cambio
                    FOR EACH ROW EXECUTE FUNCTION fn_set_audit_fields();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "niveles_aprobacion");

            migrationBuilder.DropTable(
                name: "ofertas");

            migrationBuilder.DropTable(
                name: "tipos_cambio");

            migrationBuilder.DropTable(
                name: "licitaciones");

            migrationBuilder.DropTable(
                name: "proveedores");

            migrationBuilder.Sql(
                """
                DROP FUNCTION IF EXISTS fn_desactivar_tipos_cambio_previos();
                DROP FUNCTION IF EXISTS fn_bloquear_oferta_licitacion_cerrada();
                DROP FUNCTION IF EXISTS fn_validar_oferta();
                DROP FUNCTION IF EXISTS fn_normalizar_nombre_proveedor();
                DROP FUNCTION IF EXISTS fn_normalizar_codigo_licitacion();
                DROP FUNCTION IF EXISTS fn_set_audit_fields();
                DROP TYPE IF EXISTS estado_licitacion;
                """);
        }
    }
}
