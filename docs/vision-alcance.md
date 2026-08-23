# Visión y alcance — Sistema de Gestión de Licitaciones

Este documento explica qué quiero construir, para quién es y hasta dónde llega
el proyecto. En XP el alcance no se define una sola vez al inicio: se decide en
cada Planning Game mediante historias de usuario y se ajusta con el feedback de
cada pequeña liberación. Por eso este documento describe la visión general y
delega el detalle a las [historias de usuario](historias-usuario.md) y al
[plan de liberación](plan-xp.md).

---

## 1. Visión del producto

Un sistema web sencillo para administrar un proceso de licitaciones completo:
registrar proveedores, publicar licitaciones con fecha de vencimiento, recibir
ofertas dentro del plazo, compararlas automáticamente y dejar registro de quién
aprueba cada monto según niveles configurables.

El problema que resuelve es real y cotidiano: muchas organizaciones pequeñas
llevan sus licitaciones en hojas de cálculo sueltas, sin control de estados ni
de plazos, y decidir ganador se vuelve una tarea manual propensa a errores. La
visión es reemplazar eso por una herramienta única donde el proceso quede
registrado, validado y auditable.

Como proyecto individual de XP, la visión también tiene un lado personal:
aplicar las prácticas de Extreme Programming (Planning Game, TDD, diseño
simple, refactorización, pequeñas liberaciones) sobre un producto de tamaño
real, dejando evidencia verificable de cada práctica en esta carpeta `/docs`.

## 2. Principios que guían el alcance

Coherentes con los valores de XP:

- **Simplicidad**: construir solo lo que una historia pide. Si algo no está en
  ninguna historia, no entra al alcance (ver sección 5).
- **Feedback rápido**: cada iteración termina en una liberación demostrable que
  el cliente (en este caso, yo mismo asumiendo ese rol) acepta o corrige.
- **Calidad como hábito**: nada se considera "terminado" sin pruebas que lo
  respalden y sin estar integrado al repositorio principal.
- **Comunicación**: todo lo relevante queda escrito en `/docs`, porque en un
  proyecto de una persona los documentos son la memoria del equipo.

## 3. Alcance incluido

Lo siguiente sí forma parte del producto y está cubierto por las 54 historias:

- **Proveedores**: CRUD completo con unicidad de nombre normalizado y borrado
  lógico.
- **Licitaciones**: CRUD completo, código único normalizado, presupuesto
  positivo, fecha límite futura y ciclo de vida con estados (Borrador →
  Publicada → Cerrada).
- **Ofertas**: registro dentro del plazo, monto positivo, unicidad por par
  licitación–proveedor, tope por presupuesto y bloqueo de cambios tras cierre o
  vencimiento.
- **Mejor oferta y ahorro**: selección automática de la mejor oferta (menor
  monto, desempate determinista) y clasificación del ahorro respecto al
  presupuesto.
- **Niveles de aprobación**: rangos de monto sin traslapes ni duplicados y
  resolución automática del aprobador según el monto.
- **Tipo de cambio CRC/USD**: tasa vigente única activa y conversión visual de
  montos en la interfaz.
- **Interfaz web MVC** con landing, formularios validados, modo claro/oscuro,
  accesibilidad básica y diseño adaptable.
- **API REST versionada** (`/api/v1`) con OpenAPI, paginación, filtrado y
  errores ProblemDetails.
- **PostgreSQL** con migraciones EF Core, datos semilla, auditoría y concurrencia
  optimista.
- **Infraestructura reproducible**: Docker Compose, manifiestos de Kubernetes,
  pipeline de CI/CD en GitHub Actions y suite de pruebas (unitarias,
  integración con PostgreSQL real, funcionales y de navegador).

## 4. Priorización del alcance

El alcance se ordenó con MoSCoW y estimación Fibonacci durante el Planning
Game, y se entregó por incrementos:

| Iteración | Enfoque | Liberación |
|---|---|---|
| 1 | Proveedores y cimientos del dominio | Tag `v0.1.0-iteracion1` |
| 2 | Licitaciones: ciclo de vida y reglas base | Tag `v0.2.0-iteracion2` |
| 3 | Ofertas, mejor oferta, aprobación y tipo de cambio | Tag `v0.3.0-iteracion3` |
| 4 | Interfaz web, endurecimiento, CI/CD y documentación | Tag final `v1.0.0` |

El detalle de qué historia entró en cada liberación está en el
[plan de liberación](plan-xp.md), la [bitácora XP](bitacora-xp.md) y los
guiones de demo en [`releases/`](releases/).

## 5. Fuera del alcance

Declarado explícitamente para evitar expectativas equivocadas. Estas decisiones
se tomaron en los Planning Games priorizando valor con el esfuerzo disponible:

- **Autenticación y roles de usuario**: el sistema opera como consola interna
  confiable; no hay inicio de sesión ni permisos por rol.
- **Adjudicación y pagos**: el proceso termina en cerrar la licitación con su
  mejor oferta identificada; no gestiona contratos ni transacciones económicas.
- **Integraciones externas**: no consume servicios de tipo de cambio en línea
  ni sistemas institucionales; la tasa se administra manualmente.
- **Notificaciones por correo o alertas automáticas** de vencimientos.
- **Multi-idioma**: la interfaz y los mensajes están únicamente en español.
- **Escalabilidad masiva**: el objetivo es una organización pequeña con volumen
  moderado de registros, no cargas de nivel empresarial.

Si el proyecto continuara, estas exclusiones serían candidatas naturales para
nuevas historias en un próximo Planning Game.

## 6. Criterios de cierre

El producto se considera dentro de visión y alcance cuando:

1. Las cuatro iteraciones están cerradas con sus pequeñas liberaciones
   etiquetadas (última: `v1.0.0`).
2. Las historias aceptadas coinciden con lo implementado y trazado en la
   [matriz de trazabilidad](matriz-trazabilidad.md).
3. El pipeline de CI/CD pasa completo (compilación, pruebas, cobertura, lint,
   Docker, Kubernetes y pruebas de navegador).
4. La documentación de `/docs` corresponde con la implementación real.
