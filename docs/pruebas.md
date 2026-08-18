# Estrategia de pruebas

El proyecto combina pruebas unitarias, integración con PostgreSQL real, funcionales de contrato/middleware y navegador. La solución usa xUnit; las pruebas de navegador usan Playwright y las de PostgreSQL pueden requerir Docker.

## Suites

| Proyecto | Alcance | Ejemplos |
|---|---|---|
| `Licitaciones.UnitTests` | Entidades, reglas, handlers y servicios aislados | transiciones, normalización, mejor oferta, ahorro, moneda, reloj |
| `Licitaciones.IntegrationTests` | EF Core y PostgreSQL real | migraciones, FKs, checks, índices, transacciones, auditoría, concurrencia |
| `Licitaciones.FunctionalTests` | Contrato API y middleware en proceso | rutas v1, Swagger, DTO, ProblemDetails y seguridad de errores |
| `Licitaciones.BrowserTests` | Flujos de usuario en navegador | landing, CRUD, formularios, tema y conversión |

## Ejecución

```powershell
dotnet restore SistemaLicitaciones.sln
dotnet build SistemaLicitaciones.sln --no-restore
dotnet test SistemaLicitaciones.sln --no-build
```

Para navegador, instale primero los binarios requeridos por Playwright según el artefacto generado al compilar y asegure que la aplicación/base estén disponibles según `TestSettings`. Para la integración PostgreSQL, Docker debe estar operativo.

## Cobertura

La cobertura se recopila con Coverlet y se valida con `tools/check-coverage.py`. El pipeline de [CI/CD](ci-cd.md) es la referencia de comandos y umbrales vigentes; no se debe sustituir evidencia de ejecución por estimaciones manuales.

## Trazabilidad

- Reglas de dominio y aplicación: UnitTests.
- Restricciones del [modelo de datos](modelo-datos.md): IntegrationTests/Postgres.
- Contratos de [API](api.md): FunctionalTests.
- Flujos de [integración](integracion-modulos.md): BrowserTests e IntegrationTests.

Una historia se considera verificada cuando sus pruebas relevantes pasan y la demostración manual de aceptación no contradice el comportamiento automatizado.
