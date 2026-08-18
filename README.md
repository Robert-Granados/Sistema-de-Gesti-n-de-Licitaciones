# Sistema de Gestión de Licitaciones

Aplicación .NET 9 para administrar proveedores, licitaciones, ofertas, niveles
de aprobación y tipos de cambio. El proyecto se desarrolló con una adaptación
individual de Extreme Programming (XP), cuatro iteraciones y pequeñas
liberaciones verificables.

## Estado

- Versión estable: `v1.0.0`.
- CI/CD: compilación, pruebas, cobertura, formato, Docker, Kubernetes,
  dependencias y pruebas de navegador.
- Pruebas unitarias: 219 casos.
- Documentación: [índice completo](docs/README.md).

## Ejecutar con Docker

```powershell
Copy-Item .env.example .env
docker compose up --build -d
docker compose ps
```

La aplicación queda en <http://localhost:8080>, Swagger en
<http://localhost:8080/swagger> y el estado de salud en
<http://localhost:8080/health>.

## Desarrollo y pruebas

```powershell
dotnet restore SistemaLicitaciones.sln
dotnet build SistemaLicitaciones.sln --configuration Release --no-restore
dotnet test tests/Licitaciones.UnitTests --configuration Release --no-build
dotnet test tests/Licitaciones.FunctionalTests --configuration Release --no-build
```

Las pruebas de integración requieren Docker/PostgreSQL. Las pruebas de navegador
requieren levantar previamente la aplicación y PostgreSQL con Docker Compose.
El procedimiento completo está en [Pruebas](docs/pruebas.md).

## Evidencia XP

- [Matriz de trazabilidad completa](docs/matriz-trazabilidad.md)
- [Adaptación de XP para una persona](docs/xp-individual.md)
- [Historias de usuario](docs/historias-usuario.md)
- [Plan de liberación](docs/plan-xp.md)
- [Bitácora de iteraciones](docs/bitacora-xp.md)
- [Pequeñas liberaciones](docs/releases/iteracion-1.md)
- [Pipeline CI/CD](docs/ci-cd.md)

La trazabilidad se completa en GitHub mediante Issues, Milestones, Pull
Requests, checks automáticos y Releases. Las prácticas que requieren varias
personas se declaran honestamente como adaptadas o no aplicables; no se presenta
la asistencia de IA como programación en pareja humana.

## Arquitectura

La solución separa `Domain`, `Application`, `Infrastructure`, `Web` y `Api`.
Consulte [Arquitectura general](docs/arquitectura-general.md),
[Modelo de datos](docs/modelo-datos.md) y [API REST](docs/api.md).
