-- =====================================================================
-- Sistema de Gestión de Licitaciones
-- Script de base de datos para PostgreSQL 16+
-- Compatible con Entity Framework Core 9 (Npgsql)
-- =====================================================================
-- Convenciones:
--   * snake_case para tablas y columnas.
--   * Todos los montos usan numeric(18,2) (o numeric(18,6) para tipo
--     de cambio) — nunca float/double.
--   * Auditoría: created_at, updated_at, deleted_at (borrado lógico).
--   * Concurrencia optimista: columna row_version (integer) mantenida
--     por trigger.
--   * Fechas como timestamptz; las comparaciones se realizan en UTC.
-- =====================================================================

BEGIN;

-- ---------------------------------------------------------------------
-- Extensiones necesarias
-- ---------------------------------------------------------------------
CREATE EXTENSION IF NOT EXISTS unaccent;   -- normalización de nombres (acentos)
CREATE EXTENSION IF NOT EXISTS pg_trgm;    -- búsquedas por similitud/filtrado de texto

-- ---------------------------------------------------------------------
-- Tipos enumerados
-- ---------------------------------------------------------------------
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'estado_licitacion') THEN
        CREATE TYPE estado_licitacion AS ENUM ('Borrador', 'Publicada', 'Cerrada');
    END IF;
END$$;

-- ---------------------------------------------------------------------
-- Funciones de soporte (auditoría, concurrencia, normalización)
-- ---------------------------------------------------------------------

-- Mantiene updated_at y row_version en cada UPDATE.
CREATE OR REPLACE FUNCTION fn_set_audit_fields()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at := now();
    NEW.row_version := COALESCE(OLD.row_version, 0) + 1;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Normaliza el código de licitación: trim + mayúsculas.
CREATE OR REPLACE FUNCTION fn_normalizar_codigo_licitacion()
RETURNS TRIGGER AS $$
BEGIN
    NEW.codigo_normalizado := upper(trim(NEW.codigo));
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Normaliza el nombre de proveedor: trim, colapsa espacios, sin acentos, mayúsculas.
CREATE OR REPLACE FUNCTION fn_normalizar_nombre_proveedor()
RETURNS TRIGGER AS $$
BEGIN
    NEW.nombre_normalizado := upper(unaccent(regexp_replace(trim(NEW.nombre), '\s+', ' ', 'g')));
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Valida reglas de negocio de la oferta a nivel de base de datos
-- (capa de respaldo de la validación de interfaz y servidor).
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
        RAISE EXCEPTION 'La licitación % no existe o fue eliminada', NEW.licitacion_id
            USING ERRCODE = '23503';
    END IF;

    IF v_estado <> 'Publicada' THEN
        RAISE EXCEPTION 'No se pueden registrar ofertas en licitaciones que no estén Publicadas (estado actual: %)', v_estado
            USING ERRCODE = '22023';
    END IF;

    IF v_fecha_cierre <= now() THEN
        RAISE EXCEPTION 'La licitación % está vencida o cerrada, no admite ofertas', NEW.licitacion_id
            USING ERRCODE = '22023';
    END IF;

    IF NEW.monto_ofertado_crc > v_presupuesto THEN
        RAISE EXCEPTION 'El monto ofertado (%) supera el presupuesto estimado (%)', NEW.monto_ofertado_crc, v_presupuesto
            USING ERRCODE = '22023';
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Impide crear/editar/eliminar ofertas de licitaciones cerradas (evidencia inmutable).
CREATE OR REPLACE FUNCTION fn_bloquear_oferta_licitacion_cerrada()
RETURNS TRIGGER AS $$
DECLARE
    v_estado estado_licitacion;
    v_fecha_cierre timestamptz;
    v_licitacion_id uuid;
BEGIN
    v_licitacion_id := COALESCE(NEW.licitacion_id, OLD.licitacion_id);

    SELECT estado, fecha_cierre INTO v_estado, v_fecha_cierre
      FROM licitaciones WHERE id = v_licitacion_id;

    IF TG_OP = 'UPDATE' OR TG_OP = 'DELETE' THEN
        IF v_estado = 'Cerrada' OR v_fecha_cierre <= now() THEN
            RAISE EXCEPTION 'No se pueden modificar ni eliminar ofertas de licitaciones cerradas'
                USING ERRCODE = '22023';
        END IF;
    END IF;

    IF TG_OP = 'DELETE' THEN
        RETURN OLD;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Garantiza que al activar un tipo de cambio se desactive el anterior (un único activo).
CREATE OR REPLACE FUNCTION fn_desactivar_tipos_cambio_previos()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.activo THEN
        UPDATE tipos_cambio
           SET activo = false, updated_at = now(), row_version = row_version + 1
         WHERE activo = true
           AND id <> NEW.id;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ---------------------------------------------------------------------
-- Tabla: proveedores
-- ---------------------------------------------------------------------
CREATE TABLE proveedores (
    id                  uuid            NOT NULL DEFAULT gen_random_uuid(),
    nombre              varchar(200)    NOT NULL,
    nombre_normalizado  varchar(200)    NOT NULL,
    created_at          timestamptz     NOT NULL DEFAULT now(),
    updated_at          timestamptz     NOT NULL DEFAULT now(),
    deleted_at          timestamptz     NULL,
    row_version         integer         NOT NULL DEFAULT 0,

    CONSTRAINT pk_proveedores PRIMARY KEY (id),
    CONSTRAINT ck_proveedores_nombre_caracteres
        CHECK (nombre ~ '^[\p{L}\p{N}\s.,()]+$'),
    CONSTRAINT ck_proveedores_nombre_no_vacio
        CHECK (length(trim(nombre)) > 0)
);

COMMENT ON TABLE proveedores IS 'Proveedores que pueden ofertar en licitaciones.';

-- Unicidad de nombre normalizado, solo entre registros activos (no eliminados lógicamente).
CREATE UNIQUE INDEX ux_proveedores_nombre_normalizado
    ON proveedores (nombre_normalizado)
    WHERE deleted_at IS NULL;

CREATE INDEX ix_proveedores_deleted_at ON proveedores (deleted_at);

CREATE TRIGGER trg_proveedores_normalizar
    BEFORE INSERT OR UPDATE OF nombre ON proveedores
    FOR EACH ROW EXECUTE FUNCTION fn_normalizar_nombre_proveedor();

CREATE TRIGGER trg_proveedores_audit
    BEFORE UPDATE ON proveedores
    FOR EACH ROW EXECUTE FUNCTION fn_set_audit_fields();

-- ---------------------------------------------------------------------
-- Tabla: licitaciones
-- ---------------------------------------------------------------------
CREATE TABLE licitaciones (
    id                          uuid                NOT NULL DEFAULT gen_random_uuid(),
    codigo                      varchar(50)         NOT NULL,
    codigo_normalizado          varchar(50)         NOT NULL,
    titulo                      varchar(300)        NOT NULL,
    estado                      estado_licitacion   NOT NULL DEFAULT 'Borrador',
    fecha_cierre                timestamptz         NOT NULL,
    presupuesto_estimado_crc    numeric(18,2)       NOT NULL,
    created_at                  timestamptz         NOT NULL DEFAULT now(),
    updated_at                  timestamptz         NOT NULL DEFAULT now(),
    deleted_at                  timestamptz         NULL,
    row_version                 integer             NOT NULL DEFAULT 0,

    CONSTRAINT pk_licitaciones PRIMARY KEY (id),
    CONSTRAINT ck_licitaciones_presupuesto_positivo
        CHECK (presupuesto_estimado_crc > 0),
    CONSTRAINT ck_licitaciones_titulo_no_vacio
        CHECK (length(trim(titulo)) > 0)
);

COMMENT ON TABLE licitaciones IS 'Procesos de licitación gestionados por la organización.';
COMMENT ON COLUMN licitaciones.presupuesto_estimado_crc IS 'Presupuesto oficial en colones costarricenses (fuente de verdad monetaria).';

CREATE UNIQUE INDEX ux_licitaciones_codigo_normalizado
    ON licitaciones (codigo_normalizado)
    WHERE deleted_at IS NULL;

CREATE INDEX ix_licitaciones_estado ON licitaciones (estado);
CREATE INDEX ix_licitaciones_fecha_cierre ON licitaciones (fecha_cierre);
CREATE INDEX ix_licitaciones_deleted_at ON licitaciones (deleted_at);

CREATE TRIGGER trg_licitaciones_normalizar
    BEFORE INSERT OR UPDATE OF codigo ON licitaciones
    FOR EACH ROW EXECUTE FUNCTION fn_normalizar_codigo_licitacion();

CREATE TRIGGER trg_licitaciones_audit
    BEFORE UPDATE ON licitaciones
    FOR EACH ROW EXECUTE FUNCTION fn_set_audit_fields();

-- ---------------------------------------------------------------------
-- Tabla: ofertas
-- ---------------------------------------------------------------------
CREATE TABLE ofertas (
    id                  uuid            NOT NULL DEFAULT gen_random_uuid(),
    licitacion_id       uuid            NOT NULL,
    proveedor_id        uuid            NOT NULL,
    monto_ofertado_crc  numeric(18,2)   NOT NULL,
    fecha_registro      timestamptz     NOT NULL DEFAULT now(),
    updated_at          timestamptz     NOT NULL DEFAULT now(),
    row_version         integer         NOT NULL DEFAULT 0,

    CONSTRAINT pk_ofertas PRIMARY KEY (id),
    CONSTRAINT fk_ofertas_licitacion
        FOREIGN KEY (licitacion_id) REFERENCES licitaciones (id) ON DELETE RESTRICT,
    CONSTRAINT fk_ofertas_proveedor
        FOREIGN KEY (proveedor_id) REFERENCES proveedores (id) ON DELETE RESTRICT,
    CONSTRAINT ck_ofertas_monto_positivo
        CHECK (monto_ofertado_crc > 0),
    -- Un proveedor no puede ofertar más de una vez en la misma licitación.
    CONSTRAINT ux_ofertas_licitacion_proveedor
        UNIQUE (licitacion_id, proveedor_id)
);

COMMENT ON TABLE ofertas IS 'Ofertas económicas de proveedores para una licitación específica. Las ofertas de licitaciones cerradas son evidencia inmutable.';

CREATE INDEX ix_ofertas_licitacion_id ON ofertas (licitacion_id);
CREATE INDEX ix_ofertas_proveedor_id ON ofertas (proveedor_id);
-- Soporte para el cálculo de "mejor oferta" (menor monto, desempate por orden de registro).
CREATE INDEX ix_ofertas_licitacion_monto_fecha
    ON ofertas (licitacion_id, monto_ofertado_crc ASC, fecha_registro ASC);

CREATE TRIGGER trg_ofertas_validar_negocio
    BEFORE INSERT ON ofertas
    FOR EACH ROW EXECUTE FUNCTION fn_validar_oferta();

CREATE TRIGGER trg_ofertas_bloquear_si_cerrada
    BEFORE UPDATE OR DELETE ON ofertas
    FOR EACH ROW EXECUTE FUNCTION fn_bloquear_oferta_licitacion_cerrada();

CREATE TRIGGER trg_ofertas_audit
    BEFORE UPDATE ON ofertas
    FOR EACH ROW EXECUTE FUNCTION fn_set_audit_fields();

-- ---------------------------------------------------------------------
-- Tabla: niveles_aprobacion
-- ---------------------------------------------------------------------
CREATE TABLE niveles_aprobacion (
    id                  uuid            NOT NULL DEFAULT gen_random_uuid(),
    monto_minimo_crc    numeric(18,2)   NOT NULL,
    monto_maximo_crc    numeric(18,2)   NULL,
    aprobador           varchar(150)    NOT NULL,
    created_at          timestamptz     NOT NULL DEFAULT now(),
    updated_at          timestamptz     NOT NULL DEFAULT now(),
    row_version         integer         NOT NULL DEFAULT 0,

    -- Rango numérico derivado, usado para impedir traslapes con una restricción
    -- de exclusión. Un monto_maximo_crc NULL representa un rango abierto (sin límite superior).
    rango_monto numrange GENERATED ALWAYS AS (
        numrange(monto_minimo_crc, monto_maximo_crc, '[]')
    ) STORED,

    CONSTRAINT pk_niveles_aprobacion PRIMARY KEY (id),
    CONSTRAINT ck_niveles_monto_minimo_no_negativo
        CHECK (monto_minimo_crc >= 0),
    CONSTRAINT ck_niveles_monto_maximo_mayor_minimo
        CHECK (monto_maximo_crc IS NULL OR monto_maximo_crc > monto_minimo_crc),
    CONSTRAINT ck_niveles_aprobador_no_vacio
        CHECK (length(trim(aprobador)) > 0),
    -- Los rangos de distintos niveles no pueden traslaparse.
    CONSTRAINT ex_niveles_rango_sin_traslape
        EXCLUDE USING gist (rango_monto WITH &&)
);

COMMENT ON TABLE niveles_aprobacion IS 'Tabla parametrizable de aprobadores según rango de monto (reemplaza lógica if/else fija).';

-- Solo puede existir un rango abierto (monto_maximo_crc IS NULL).
CREATE UNIQUE INDEX ux_niveles_aprobacion_unico_abierto
    ON niveles_aprobacion ((monto_maximo_crc IS NULL))
    WHERE monto_maximo_crc IS NULL;

CREATE TRIGGER trg_niveles_aprobacion_audit
    BEFORE UPDATE ON niveles_aprobacion
    FOR EACH ROW EXECUTE FUNCTION fn_set_audit_fields();

-- ---------------------------------------------------------------------
-- Tabla: tipos_cambio
-- ---------------------------------------------------------------------
CREATE TABLE tipos_cambio (
    id              uuid            NOT NULL DEFAULT gen_random_uuid(),
    crc_por_usd     numeric(18,6)   NOT NULL,
    fecha_vigencia  timestamptz     NOT NULL,
    activo          boolean         NOT NULL DEFAULT false,
    created_at      timestamptz     NOT NULL DEFAULT now(),
    updated_at      timestamptz     NOT NULL DEFAULT now(),
    row_version     integer         NOT NULL DEFAULT 0,

    CONSTRAINT pk_tipos_cambio PRIMARY KEY (id),
    CONSTRAINT ck_tipos_cambio_valor_positivo
        CHECK (crc_por_usd > 0)
);

COMMENT ON TABLE tipos_cambio IS 'Tipo de cambio administrable localmente (CRC es la moneda fuente de verdad; USD es solo representación calculada).';

-- Solo puede existir un tipo de cambio activo a la vez.
CREATE UNIQUE INDEX ux_tipos_cambio_unico_activo
    ON tipos_cambio (activo)
    WHERE activo;

CREATE INDEX ix_tipos_cambio_fecha_vigencia ON tipos_cambio (fecha_vigencia DESC);

CREATE TRIGGER trg_tipos_cambio_desactivar_previos
    BEFORE INSERT OR UPDATE OF activo ON tipos_cambio
    FOR EACH ROW
    WHEN (NEW.activo = true)
    EXECUTE FUNCTION fn_desactivar_tipos_cambio_previos();

CREATE TRIGGER trg_tipos_cambio_audit
    BEFORE UPDATE ON tipos_cambio
    FOR EACH ROW EXECUTE FUNCTION fn_set_audit_fields();

-- ---------------------------------------------------------------------
-- Datos semilla mínimos
-- ---------------------------------------------------------------------

-- Niveles de aprobación iniciales (según especificación del proyecto).
INSERT INTO niveles_aprobacion (monto_minimo_crc, monto_maximo_crc, aprobador) VALUES
    (0.01,          999999.99,      'Encargado de área'),
    (1000000.00,    9999999.99,     'Gerencia'),
    (10000000.00,   NULL,           'Junta Directiva');

-- Tipo de cambio inicial activo (valor de referencia; debe administrarse desde la aplicación).
INSERT INTO tipos_cambio (crc_por_usd, fecha_vigencia, activo) VALUES
    (520.00, now(), true);

COMMIT;

-- =====================================================================
-- Notas de mapeo para Entity Framework Core (Npgsql):
--   * Mapear "row_version" con .IsRowVersion() o [Timestamp] no aplica
--     directamente (eso es para tipos "xmin"/byte[]); usar en su lugar
--     .Property(x => x.RowVersion).IsConcurrencyToken() con actualización
--     manual, o mapear la columna de sistema "xmin" con
--     .UseXminAsConcurrencyToken() como alternativa más idiomática en
--     PostgreSQL si se prefiere no mantener row_version manualmente.
--   * Mapear "estado" (estado_licitacion) con
--     .HasPostgresEnum<EstadoLicitacion>() y .HasConversion<string>()
--     según la versión del proveedor Npgsql utilizada.
--   * Mapear todas las propiedades decimal con
--     .HasColumnType("numeric(18,2)") o "numeric(18,6)" según la tabla.
--   * No mapear "rango_monto" (columna generada, uso interno de la BD).
-- =====================================================================
