# Pequeña liberación — Iteración 4

## Objetivo de la demo

Validar que el incremento final sea mantenible, verificable y desplegable:
auditoría y concurrencia consistentes, pruebas y cobertura con umbrales,
contenedores reproducibles, manifiestos Kubernetes, pipeline bloqueante y una
documentación que permita comprender arquitectura, datos, módulos y contratos.

## Alcance XP

| Bloque | Historias | Puntos | Resultado |
|---|---|---:|---|
| Persistencia avanzada | HU-40 a HU-43 | 11 | Completado |
| TDD y pruebas | HU-44 a HU-47 | 24 | Completado |
| Docker, Kubernetes y CI | HU-48 a HU-50 | 18 | Completado |
| Documentación y prácticas XP | HU-51 a HU-54 | 10 | Completado |
| **Total** | **HU-40 a HU-54** | **63** | **15/15 historias** |

## Preparación

Desde la raíz del repositorio:

```powershell
Copy-Item .env.example .env
# Ajuste las credenciales locales de .env.
docker compose up --build -d
docker compose ps
```

La aplicación debe responder en <http://localhost:8080>, Swagger en
<http://localhost:8080/swagger> y salud en <http://localhost:8080/health>.

## Guion de demostración

| Paso | Acción | Resultado esperado |
|---|---|---|
| 1 | Crear y modificar un recurso; inspeccionar sus campos de auditoría. | `created_at` permanece inmutable, `updated_at` cambia y el borrado lógico fija `deleted_at`. |
| 2 | Enviar una edición con `rowVersion` obsoleto. | La API devuelve HTTP 409 y `concurrency_conflict` sin información interna. |
| 3 | Reiniciar Compose sin `-v`. | La aplicación y PostgreSQL vuelven saludables y los datos persisten. |
| 4 | Revisar el usuario del contenedor de aplicación. | El proceso se ejecuta como `app`, no como root. |
| 5 | Revisar `k8s/` y ejecutar Kubeconform. | Los nueve recursos son válidos y separan Secret, migración, base y aplicación. |
| 6 | Revisar `.github/workflows/ci.yml`. | Compilación, pruebas/cobertura, formato, Docker, Kubernetes, dependencias y navegador alimentan el gate `CI obligatorio`. |
| 7 | Abrir `docs/README.md`. | Todos los documentos de arquitectura, módulos, operación, pruebas y XP son navegables. |
| 8 | Abrir `modelo-datos.md`. | El ER coincide con `database_schema.sql` y el mapa identifica los 16 triggers. |
| 9 | Abrir `integracion-modulos.md` y `api.md`. | Las capas, colaboraciones, flujos, endpoints, contratos y errores son comprensibles. |
| 10 | Abrir `uso-ia.md`. | Se declara herramienta, finalidad, asistencia, ejemplos, validaciones y revisión humana. |

## Comprobación automatizada

Use el SDK local incluido cuando `dotnet` no esté disponible globalmente:

```powershell
.\.dotnet\dotnet.exe build SistemaLicitaciones.sln --no-restore
.\.dotnet\dotnet.exe test tests\Licitaciones.UnitTests\Licitaciones.UnitTests.csproj --no-build --no-restore
.\.dotnet\dotnet.exe test tests\Licitaciones.FunctionalTests\Licitaciones.FunctionalTests.csproj --no-build --no-restore
```

Evidencia del cierre documental: compilación con 0 errores y 0 advertencias,
219 pruebas unitarias y 9 funcionales aprobadas. Las comprobaciones Docker,
PostgreSQL, cobertura y Kubernetes están detalladas en la
[bitácora XP](../bitacora-xp.md).

## Retroalimentación incorporada

La revisión final detectó que faltaban visualizaciones para triggers y API, y
que el primer diagrama de integración concentraba demasiadas relaciones. Se
añadieron los diagramas faltantes y se separó la integración en arquitectura
por capas, colaboraciones funcionales y secuencias.

## Lista de aceptación

- [x] HU-40 a HU-54 están completadas y trazadas en la bitácora.
- [x] La velocidad final es 63 de 63 puntos.
- [x] La compilación y las suites unitarias/funcionales pasan.
- [x] Docker, Kubernetes y CI/CD tienen instrucciones y evidencia verificable.
- [x] La arquitectura y el modelo de datos incluyen diagramas fieles.
- [x] Cada módulo, la integración y la API están documentados.
- [x] El uso responsable de IA está declarado.
- [x] La retroalimentación documental fue aplicada.
- [x] La Iteración 4 se acepta como completada.

## Publicación

La versión final es `v1.0.0`. El tag fue publicado después de integrar el commit
revisado en `main` y confirmar el check remoto `CI obligatorio`.
