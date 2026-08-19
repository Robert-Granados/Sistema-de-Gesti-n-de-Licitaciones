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

Las pruebas de navegador requieren los binarios de Playwright generados durante
la compilación y la disponibilidad de la aplicación y la base indicada en
`TestSettings`. Las pruebas de integración con PostgreSQL requieren Docker.

## Cobertura

La cobertura se recopila con Coverlet y se valida con `tools/check-coverage.py`. El pipeline de [CI/CD](ci-cd.md) es la referencia de comandos y umbrales vigentes; no se debe sustituir evidencia de ejecución por estimaciones manuales.

## Trazabilidad

La relación verificable por historia —Issue, prueba, commit, PR, documentación y
release— se encuentra en la [matriz de trazabilidad](matriz-trazabilidad.md).

- Reglas de dominio y aplicación: UnitTests.
- Restricciones del [modelo de datos](modelo-datos.md): IntegrationTests/Postgres.
- Contratos de [API](api.md): FunctionalTests.
- Flujos de [integración](integracion-modulos.md): BrowserTests e IntegrationTests.

Una historia se considera verificada cuando sus pruebas relevantes pasan y la demostración manual de aceptación no contradice el comportamiento automatizado.
