# Uso responsable de inteligencia artificial

## Herramienta y finalidad

Se utilizó Codex, asistente de programación de OpenAI, como apoyo para analizar el repositorio, contrastar documentación con la implementación y redactar documentación técnica de la Iteración 4. La herramienta actuó como asistencia; las decisiones del proyecto, la aceptación y la autoría responsable permanecen en el equipo.

## Módulos y artefactos asistidos

- Arquitectura y modelo de datos: lectura de proyectos, dependencias, `AppDbContext`, configuraciones y `database_schema.sql`; producción de diagramas Mermaid.
- Documentación funcional: proveedores, licitaciones, ofertas, niveles de aprobación, tipo de cambio, persistencia, interfaz web y API REST.
- Integración y API: contraste con controladores, contratos, middleware, ejemplos HTTP y pruebas funcionales.
- Índice, estrategia de pruebas y esta declaración de uso.

No se atribuye a IA la implementación previa de módulos cuando no existe evidencia de ello. Esta declaración se debe actualizar si se usa otra herramienta o si la asistencia se extiende a nuevos artefactos.

## Ejemplos relevantes de asistencia

1. Derivar el `erDiagram` directamente de las cinco tablas, dos relaciones, tipos y claves de `database_schema.sql`.
2. Inventariar las rutas declaradas con `[HttpGet]`, `[HttpPost]`, `[HttpPut]` y `[HttpDelete]` para documentar los contratos v1.
3. Relacionar handlers, puertos y repositorios para explicar la dirección de dependencias y los flujos de extremo a extremo.
4. Identificar archivos Markdown vacíos y convertirlos en documentación navegable sin insertar marcas, comentarios artificiales ni contenido ajeno a la funcionalidad.

## Validaciones realizadas

- Comparación manual del diagrama con `CREATE TABLE`, PK, FK, campos y restricciones del script SQL.
- Comparación de endpoints y cuerpos con controladores y `ApiContracts.cs`.
- Comparación de errores con `ApiExceptionMiddleware` y sus pruebas funcionales.
- Revisión de que cada módulo incluya propósito, responsabilidades, dependencias, entradas, salidas, reglas, errores y pruebas.
- Verificación automatizada de compilación/pruebas y de enlaces Markdown relativos al completar esta iteración; cualquier limitación del entorno debe registrarse en la entrega o bitácora.

## Protocolo de revisión del equipo

Antes de aceptar la historia, una persona del equipo debe revisar los cambios, ejecutar las pruebas pertinentes y confirmar que diagramas, ejemplos y reglas coinciden con el sistema observable. No se deben incorporar secretos, datos personales, código no comprendido ni afirmaciones no verificadas. Los cambios sugeridos por IA se mantienen bajo el mismo proceso de revisión, versionamiento y pruebas que cualquier contribución humana.
