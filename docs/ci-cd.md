# Pipeline de CI/CD

La rama `main` está protegida para exigir Pull Request, conversaciones resueltas
y el check actualizado `8. CI obligatorio`. No permite *force push* ni eliminar
la rama. La configuración reproducible se conserva en
`.github/branch-protection.json`; no se exige aprobación de otra persona porque
el proyecto tiene un único responsable.

El workflow `.github/workflows/ci.yml` se ejecuta en cada pull request y en los
push a `main`. Sus jobs forman una cadena deliberadamente bloqueante:

1. Restaurar paquetes y compilar toda la solución en Release.
2. Ejecutar pruebas unitarias, funcionales y de integración, generar cobertura
   y verificar los umbrales del proyecto.
3. Verificar `dotnet format`, analizadores y advertencias como errores.
4. Construir la imagen Docker y confirmar que utiliza el usuario `app`.
5. Validar los nueve manifiestos Kubernetes con Kubeconform estricto.
6. Auditar dependencias NuGet directas/transitivas y, en pull requests, ejecutar
   `actions/dependency-review-action` con severidad mínima `moderate`.
7. Levantar Compose y ejecutar las pruebas Playwright contra la aplicación real.
8. Publicar el check estable `CI obligatorio`, que falla si cualquier job falló
   o fue omitido.

Los reportes de cobertura y resultados Playwright se publican como artefactos,
incluso cuando una prueba falla. Los contenedores de navegador se detienen con
`if: always()`.

## Protección de la rama

La regla de protección de `main` exige `CI obligatorio` como *status check* y
resolución de conversaciones. La configuración no permite *force push* ni la
eliminación de la rama.

## Comprobaciones locales equivalentes

```powershell
dotnet build SistemaLicitaciones.sln -c Release
dotnet test SistemaLicitaciones.sln -c Release --no-build
dotnet format SistemaLicitaciones.sln --verify-no-changes --no-restore
docker build -t licitaciones-app:ci .
docker run --rm -v "${PWD}:/work" ghcr.io/yannh/kubeconform:v0.6.7 -strict -summary /work/k8s
dotnet list SistemaLicitaciones.sln package --vulnerable --include-transitive
```
