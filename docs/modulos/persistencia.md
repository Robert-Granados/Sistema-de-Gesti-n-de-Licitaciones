# Módulo de persistencia

## Propósito y responsabilidades

Implementa los puertos de aplicación con EF Core/Npgsql, configura el modelo, ejecuta migraciones y preserva integridad, auditoría y concurrencia en PostgreSQL.

## Dependencias, entradas y salidas

- Entrada: interfaces de repositorio y entidades/criterios de aplicación.
- Componentes: `AppDbContext`, configuraciones, repositorios, migraciones y `SystemClock`.
- Salida: DTO/proyecciones, entidades de dominio controladas y confirmación de escrituras.
- Dependencias externas: PostgreSQL 16, Npgsql, extensiones `unaccent` y `pg_trgm`.

## Reglas y errores

- Aplica filtros de borrado lógico, tipos `numeric`, enum PostgreSQL, FKs y token `row_version`.
- `SaveChanges` fija auditoría mediante `IClock` y valida proveedores activos en nuevas ofertas.
- Restricciones/triggers de `database_schema.sql` son la última defensa. Violaciones se traducen en aplicación/API; `DbUpdateConcurrencyException` termina en 409.
- Las migraciones se aplican al arranque o con `Database:MigrationsOnly`.

## Pruebas

Las pruebas de integración PostgreSQL cubren migraciones, checks, unicidad, FKs, transacciones y concurrencia; `AuditoriaAppDbContextTests` y `RowVersionModelTests` cubren el mapeo transversal.
