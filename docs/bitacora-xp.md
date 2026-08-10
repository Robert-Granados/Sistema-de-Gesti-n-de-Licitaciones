# Bitácora XP

Este documento registra el resultado observable de cada iteración. Una iteración
solo se considera cerrada cuando el cliente revisa la pequeña liberación y su
retroalimentación queda registrada.

## Iteración 1 — Cimientos del dominio y proveedores

**Periodo:** cierre técnico realizado el 23 de julio de 2026

**Estado:** aceptada con ajustes; pendiente de crear el tag de liberación

**Objetivo:** administrar proveedores de extremo a extremo, conservando las
reglas de negocio, la unicidad normalizada y el historial de ofertas.

### Planning Game

El cliente priorizó como imprescindibles el modelo de dominio, la persistencia
en PostgreSQL y el flujo completo de proveedores. El equipo seleccionó las
historias HU-04 a HU-10 por su dependencia funcional y mantuvo el alcance
planificado durante la iteración.

| Historia | Resultado | Puntos |
|---|---|---:|
| HU-04 — Modelar entidades de dominio | Completada | 5 |
| HU-05 — EF Core, migraciones y datos semilla | Completada | 5 |
| HU-06 — Registrar proveedor | Completada | 3 |
| HU-07 — Listar proveedores | Completada | 3 |
| HU-08 — Consultar detalle | Completada | 2 |
| HU-09 — Editar proveedor | Completada | 2 |
| HU-10 — Eliminar proveedor lógicamente | Completada | 3 |
| **Total** | **7 de 7 historias** | **23** |

**Velocidad planificada:** 23 puntos

**Velocidad observada:** 23 puntos
**Desviación:** 0 puntos

### Desarrollo: TDD, diseño simple y trabajo colaborativo

- Se escribieron pruebas de dominio y de casos de uso para invariantes,
  normalización, duplicidad, consulta, edición, concurrencia y borrado lógico.
- En HU-09 y HU-10 se ejecutó explícitamente el ciclo rojo-verde-refactor:
  primero se definieron los escenarios esperados, después la implementación
  mínima y finalmente la integración con persistencia y MVC.
- La solución conserva la separación Domain → Application → Infrastructure/Web.
  Domain no depende de EF Core ni de PostgreSQL.
- El trabajo se realizó en sesiones colaborativas entre la persona responsable
  del proyecto y el agente de desarrollo. Para cumplir la evidencia académica
  de *pair programming* entre integrantes del equipo, deben agregarse aquí los
  nombres, roles de conductor/navegante y duración de la sesión real:

| Fecha | Conductor | Navegante | Historias | Duración |
|---|---|---|---|---|
| 23/07/2026 | Robert Granados | Robert Granados | HU-04 a HU-10 | 24 horas |

### Refactorizaciones relevantes

- Se extrajo la normalización de nombres para reutilizarla al crear, listar y
  editar proveedores.
- Los accesos de persistencia se definieron mediante puertos específicos por
  caso de uso.
- Las transiciones de edición y eliminación permanecen encapsuladas en la
  entidad `Proveedor`.
- Se eligió borrado lógico consistente para todos los proveedores, evitando
  bifurcar el comportamiento según tengan o no ofertas.

### Resultado técnico

- Compilación Release: 0 errores y 0 advertencias.
- Pruebas unitarias: 32 aprobadas.
- Pruebas de integración: 3 aprobadas.
- Pruebas funcionales base: 1 aprobada.
- Total: 36 de 36 pruebas aprobadas.
- `docker compose up --build` levanta PostgreSQL 16 y la aplicación.
- Health checks de `app` y `db`: saludables.
- `GET /health`: HTTP 200.

### Pequeña liberación

**Candidata:** `v0.1.0-iteracion1`
**Estado:** aceptada con ajustes y disponible localmente; tag pendiente.

La liberación permite:

1. Registrar proveedores con validación y unicidad normalizada.
2. Listarlos con búsqueda, orden y paginación.
3. Consultar su detalle e historial de ofertas.
4. Editar el nombre con control de concurrencia.
5. Eliminarlos lógicamente después de una confirmación explícita.
6. Conservar las ofertas y rechazar nuevas ofertas para proveedores eliminados.

El procedimiento de demostración y aceptación está en
[`releases/iteracion-1.md`](releases/iteracion-1.md).

### Retroalimentación del cliente

**Estado:** aceptada con ajustes.

- **Fecha de revisión: 23/07/2026**
- **Nombre o rol del cliente: Robert Granados**
- **Funcionalidad aceptada: Editar proveedor, Eliminar proveedor, Consultar detalle, Registrar proveedor, Listar proveedores, Buscar proveedores**
- **Observaciones:** La funcionalidad es adecuada y cumple las expectativas de
  la Iteración 1; se requiere mejorar la experiencia del usuario y facilitar
  pruebas con más datos en los diferentes apartados.
- **Cambios solicitados: Mejorar la experiencia del usuario (UX/UI)**
- **Prioridad de los cambios: Media**
- **Decisión:** Aceptada con ajustes

### Retrospectiva del equipo

- **Qué funcionó bien:** Funcionalidad implementada correctamente, pruebas unitarias y de integración exitosas. 
- **Qué debe mejorar:** Mejorar la experiencia del usuario (UX/UI) y permitir pruebas con más datos en los diferentes apartados.
- **Acción concreta para la Iteración 2:** 
  - Mejorar la experiencia del usuario (UX/UI) en la aplicación.
  - Implementar la capacidad de realizar pruebas con más datos en los diferentes apartados.
- **Responsable y fecha de seguimiento:** 
  - Responsable: Robert Granados
  - Fecha de seguimiento: 30/07/2026

### Condición de cierre

La Iteración 1 podrá marcarse como **cerrada** cuando:

- se ejecute la demo con el cliente;
- se complete la sección de retroalimentación;
- se complete la retrospectiva;
- se incorporen o planifiquen los ajustes aceptados;
- el CI permanezca verde; y
- se cree el tag `v0.1.0-iteracion1` sobre el commit aceptado.

---

## Iteración 2 — El núcleo del negocio: Licitaciones y Ofertas

**Periodo:** cierre técnico realizado el 31 de julio de 2026

**Estado:** Aceptada con ajustes; pendiente de crear el tag de liberación.

**Objetivo:** ejecutar el flujo funcional mínimo del sistema: crear, publicar y
cerrar licitaciones; registrar ofertas (válidas y rechazadas); administrarlas; y
calcular automáticamente la mejor oferta con su clasificación de ahorro.

### Planning Game

El cliente priorizó el núcleo del negocio (licitaciones y ofertas) sobre el
resto de las épicas. Se adelantó HU-42 (`IClock`, reloj inyectable) al inicio de
la iteración para que HU-16 y HU-21 fueran probables de forma determinista,
según el ajuste previsto en `plan-xp.md`. El equipo seleccionó HU-11 a HU-25 y
mantuvo el alcance planificado durante la iteración.

| Historia | Resultado | Puntos |
|---|---:|---:|
| HU-11 — Crear licitación | Completada | 5 |
| HU-12 — Listar licitaciones con paginación, filtro y orden | Completada | 3 |
| HU-13 — Consultar detalle de licitación con mejor oferta y aprobador | Completada | 5 |
| HU-14 — Editar licitación | Completada | 3 |
| HU-15 — Publicar licitación | Completada | 3 |
| HU-16 — Cerrar licitación (manual y por vencimiento) | Completada | 5 |
| HU-17 — Eliminar (borrado lógico) licitación | Completada | 3 |
| HU-18 — Registrar oferta válida | Completada | 5 |
| HU-19 — Rechazar oferta duplicada | Completada | 2 |
| HU-20 — Rechazar oferta que excede el presupuesto | Completada | 2 |
| HU-21 — Rechazar oferta vencida o de licitación cerrada | Completada | 3 |
| HU-22 — Listar y filtrar ofertas por licitación y proveedor | Completada | 2 |
| HU-23 — Editar oferta | Completada | 3 |
| HU-24 — Eliminar oferta | Completada | 2 |
| HU-25 — Calcular mejor oferta y clasificación de ahorro | Completada | 3 |
| **Total** | **15 de 15 historias** | **49** |

**Velocidad planificada:** 49 puntos

**Velocidad observada:** 49 puntos
**Desviación:** 0 puntos

### Desarrollo: TDD, diseño simple y trabajo colaborativo

- Se escribieron pruebas de dominio y de casos de uso para los invariantes de
  licitaciones (transiciones de estado, fechas, presupuesto) y de ofertas
  (monto positivo, unicidad compuesta, límite de presupuesto, vencimiento).
- La matriz de transición de estados (Borrador → Publicada → Cerrada) quedó
  cubierta por pruebas unitarias de la entidad `Licitacion` y por pruebas de
  integración con PostgreSQL.
- Los rechazos de negocio (HU-19, HU-20, HU-21) se definieron primero como
  pruebas y después se implementaron, dejando evidencia del ciclo TDD.
- En HU-23 y HU-24 se reutilizó el validador de HU-18 (`OfertaValidador`) para
  editar y eliminar ofertas, evitando duplicación de reglas (diseño simple XP).
- HU-25 quedó cubierta por pruebas unitarias dedicadas
  (`CalculadorMejorOfertaTests` y `ClasificadorAhorroTests`) para los cinco
  casos de aceptación: sin ofertas, ahorro ≥10%, ahorro entre 0% y 10%, oferta
  igual al presupuesto y desempate por orden de registro.
- La solución conserva la separación Domain → Application → Infrastructure/Web.
  Domain no depende de EF Core ni de PostgreSQL.
- El trabajo se realizó en sesiones colaborativas entre la persona responsable
  del proyecto y el agente de desarrollo. Para cumplir la evidencia académica
  de *pair programming* entre integrantes del equipo, deben agregarse aquí los
  nombres, roles de conductor/navegante y duración de la sesión real:

| Fecha | Conductor | Navegante | Historias | Duración |
|---|---|---|---|---|
| 31/07/2026 | Robert Granados | Robert Granados | HU-11 a HU-25 | 24 horas |

### Refactorizaciones relevantes

- Se extrajo `OfertaValidador` como servicio compartido para registrar, editar y
  eliminar ofertas (HU-18, HU-23 y HU-24).
- La edición del monto quedó encapsulada en la entidad `Oferta` mediante
  `ActualizarMonto`.
- La base de datos respalda la regla de licitaciones cerradas con el trigger
  `fn_bloquear_oferta_licitacion_cerrada`, traducido a
  `LicitacionNoDisponibleException` en la capa de infraestructura.
- Las pruebas de `CalculadorMejorOferta` y `ClasificadorAhorro` se extrajeron
  del test del detalle a clases dedicadas para evidenciar la aceptación de
  HU-25.

### Resultado técnico

- Compilación Release: 0 errores y 0 advertencias.
- Pruebas unitarias: 141 aprobadas.
- Pruebas de integración: 20 aprobadas.
- Pruebas funcionales base: 1 aprobada.
- Total: 162 de 162 pruebas aprobadas.
- `docker compose up --build` levanta PostgreSQL 16 y la aplicación.
- Health checks de `app` y `db`: saludables.
- `GET /health`: HTTP 200.

### Pequeña liberación

**Candidata:** `v0.2.0-iteracion2`
**Estado:** construida y disponible localmente; tag pendiente de aceptación.

La liberación permite:

1. Crear, listar, publicar, editar, cerrar y eliminar (lógicamente) licitaciones.
2. Consultar el detalle de una licitación con su mejor oferta, clasificación de
   ahorro y aprobador.
3. Registrar ofertas válidas y rechazar ofertas duplicadas, que excedan el
   presupuesto o correspondan a licitaciones vencidas o cerradas.
4. Listar y filtrar ofertas por licitación y proveedor.
5. Editar y eliminar ofertas solo mientras la licitación esté abierta, con
   confirmación antes de eliminar.
6. Ver automáticamente la mejor oferta y su clasificación de ahorro (HU-25).

El procedimiento de demostración y aceptación está en
[`releases/iteracion-2.md`](releases/iteracion-2.md).

### Retroalimentación del cliente

**Estado:** Aceptada con ajustes.

- **Fecha de revisión:** 31/07/2026
- **Nombre o rol del cliente:** Robert Granados
- **Funcionalidad aceptada:** Crear licitación, Publicar licitación, Cerrar licitación, Registrar oferta, Listar y filtrar ofertas, Editar oferta, Eliminar oferta, Consultar detalle con mejor oferta
- **Observaciones:** El flujo funcional mínimo del negocio (crear, publicar y cerrar; registrar y administrar ofertas; mejor oferta con clasificación de ahorro) funciona correctamente y los rechazos de negocio son claros. La interfaz sigue siendo básica: se requiere completar la experiencia de usuario y ver el aprobador y la conversión CRC/USD en el detalle.
- **Cambios solicitados:** Completar la experiencia de usuario (landing, menú, modo claro/oscuro, validaciones y mensajes) y mostrar el aprobador y la conversión de moneda en el detalle de licitación.
- **Prioridad de los cambios:** Media
- **Decisión:** Aceptada con ajustes

### Retrospectiva del equipo

- **Qué funcionó bien:** La matriz de transición de estados y el flujo licitación → publicación → ofertas → mejor oferta quedaron cubiertos por pruebas unitarias y de integración; se reutilizó `OfertaValidador` para registrar, editar y eliminar ofertas, evitando duplicación de reglas.
- **Qué debe mejorar:** La interfaz de usuario sigue siendo básica y falta la API REST documentada y sus pruebas para cerrar el alcance de la Iteración 3.
- **Acción concreta para la Iteración 3:**
  - Completar la experiencia de usuario (landing, menú, temas, validaciones, mensajes y confirmaciones).
  - Exponer la API REST versionada con DTOs, Swagger y errores estandarizados.
- **Responsable y fecha de seguimiento:**
  - Responsable: Robert Granados
  - Fecha de seguimiento: 07/08/2026

### Condición de cierre

La Iteración 2 podrá marcarse como **cerrada** cuando:

- se ejecute la demo con el cliente;
- se complete la sección de retroalimentación;
- se complete la retrospectiva;
- se incorporen o planifiquen los ajustes aceptados;
- el CI permanezca verde; y
- se cree el tag `v0.2.0-iteracion2` sobre el commit aceptado.

---

## Iteración 3 — Reglas paramétricas, moneda y experiencia de usuario

**Periodo:** cierre técnico realizado el 05 de agosto de 2026

**Estado:** Aceptada con ajustes; pendiente de crear el tag de liberación.

**Objetivo:** hacer la aplicación usable de principio a fin (landing, menú,
temas, validaciones, mensajes y confirmaciones), parametrizar la aprobación
mediante niveles y tipos de cambio, y exponer una API REST versionada y
documentada en Swagger que refleje el aprobador y la conversión CRC/USD.

### Planning Game

El cliente priorizó las reglas paramétricas (niveles de aprobación y tipo de
cambio), la experiencia de usuario completa y la exposición de la API REST,
según el alcance previsto para la Iteración 3 en `plan-xp.md`. El equipo
seleccionó HU-26 a HU-39 y mantuvo el alcance planificado durante la iteración.

| Historia | Resultado | Puntos |
|---|---|---:|
| HU-26 — CRUD de niveles de aprobación sin traslapes | Completada | 5 |
| HU-27 — Resolver aprobador según monto | Completada | 2 |
| HU-28 — CRUD de tipo de cambio con un único activo | Completada | 3 |
| HU-29 — Alternar visualización CRC/USD sin modificar datos | Completada | 3 |
| HU-30 — Landing page explicativa | Completada | 2 |
| HU-31 — Menú de navegación principal | Completada | 1 |
| HU-32 — Modo claro y modo oscuro persistente | Completada | 2 |
| HU-33 — Formularios con validación junto al campo | Completada | 3 |
| HU-34 — Tablas con paginación, filtrado y ordenamiento | Completada | 3 |
| HU-35 — Mensajes de éxito, advertencia y error | Completada | 2 |
| HU-36 — Confirmación antes de eliminar | Completada | 1 |
| HU-37 — API REST versionada con DTOs y Swagger | Completada | 8 |
| HU-38 — Manejo estandarizado de errores en la API | Completada | 3 |
| HU-39 — Colección reproducible de solicitudes de API | Completada | 2 |
| **Total** | **14 de 14 historias** | **40** |

**Velocidad planificada:** 40 puntos

**Velocidad observada:** 40 puntos
**Desviación:** 0 puntos

### Desarrollo: TDD, diseño simple y trabajo colaborativo

- Se escribieron pruebas unitarias de dominio y de casos de uso para los
  invariantes de los niveles de aprobación (rango traslapado, segundo rango
  abierto, resolución correcta y ausencia de configuración) y del tipo de
  cambio (valor CRC/USD mayor que cero y único activo).
- La activación de un tipo de cambio se definió primero como prueba y después
  se implementó en una transacción que desactiva el registro previamente activo
  y activa el seleccionado, dejando evidencia del ciclo rojo-verde-refactor.
- HU-37 y HU-38 se cubrieron con pruebas de contrato y del middleware antes de
  la implementación final: `ApiContractTests` verifica rutas versionadas,
  ausencia de entidades de dominio en firmas, acciones `publicar`/`cerrar` y el
  documento OpenAPI; `ApiExceptionMiddlewareTests` verifica `ProblemDetails`
  con `errorCode` y `correlationId`, y que los errores 500 no exponen datos
  sensibles.
- El resolutor de aprobador (HU-27) consulta los niveles ordenados por monto
  mínimo sin una cadena fija de `if/else` (diseño simple XP, según exige la
  historia) y retorna explícitamente `Sin aprobador configurado` cuando ningún
  rango contiene el monto.
- La solución conserva la separación Domain → Application → Infrastructure/Web,
  con la API compartiendo los mismos casos de uso de la UI y Domain sin
  depender de EF Core ni de PostgreSQL.
- El trabajo se realizó en sesiones colaborativas entre la persona responsable
  del proyecto y el agente de desarrollo. Para cumplir la evidencia académica
  de *pair programming* entre integrantes del equipo, deben agregarse aquí los
  nombres, roles de conductor/navegante y duración de la sesión real:

| Fecha | Conductor | Navegante | Historias | Duración |
|---|---|---|---|---|
| 05/08/2026 | Robert Granados | Robert Granados | HU-26 a HU-39 | 24 horas |

### Refactorizaciones relevantes

- El servicio de aplicación permite listar, obtener, crear, editar y eliminar
  niveles de aprobación; la creación y edición validan los traslapes antes de
  persistir y rechazan un segundo rango abierto. PostgreSQL conserva una
  segunda defensa mediante `ex_niveles_rango_sin_traslape` y
  `ux_niveles_aprobacion_unico_abierto`.
- Los tres rangos semilla requeridos permanecen configurados tanto en EF Core
  como en `database_schema.sql`.
- El índice parcial `ux_tipos_cambio_unico_activo` y el trigger
  `trg_tipos_cambio_desactivar_previos` respaldan la regla de un único tipo de
  cambio activo en PostgreSQL.
- El detalle de licitación ofrece un selector CRC/USD que convierte presupuesto
  y ofertas en el navegador usando únicamente la tasa activa cargada desde la
  base de datos local. Cada conversión muestra la tasa y su fecha de vigencia.
  Los formularios y DTO de escritura continúan recibiendo CRC, por lo que
  alternar la vista no modifica ni persiste montos en USD y no requiere
  conexión a Internet.
- Todas las eliminaciones visibles —proveedor, licitación, oferta, nivel de
  aprobación y tipo de cambio— pasan por un único modal Bootstrap definido en
  el layout. El componente reutiliza el formulario POST original con su token
  antifalsificación y solo lo envía después de una confirmación explícita.
- La página de inicio explica el recorrido desde la preparación de una
  licitación hasta su aprobación, incluyendo selección de mejor oferta y
  visualización CRC/USD. Usa la cuadrícula responsive de Bootstrap y ofrece
  accesos directos a los módulos operativos.
- La barra principal incluye Inicio, Licitaciones, Proveedores, Ofertas,
  Niveles de aprobación, Tipo de cambio y Swagger. En pantallas pequeñas se
  presenta como un menú Bootstrap colapsable con etiquetas accesibles.
- El menú incorpora un control visible de tema claro/oscuro. La preferencia se
  conserva en `localStorage` y un script en el encabezado aplica `data-theme`
  y `data-bs-theme` antes de cargar las hojas de estilo para evitar parpadeos.
- Los formularios muestran errores de validación junto al campo (cliente y
  servidor), las tablas de licitaciones, proveedores y ofertas paginan, filtran
  y ordenan de forma consistente, y cada operación CRUD notifica éxito,
  advertencia o error con texto comprensible.
- La API expone CRUD versionado bajo `/api/v1` para licitaciones, proveedores,
  ofertas, niveles de aprobación y tipos de cambio, además de las acciones
  `publicar`, `cerrar` y `activar`.
- Corrección posterior: los controladores API usan nombres internos con prefijo
  `Api` y rutas explícitas. Esto evita que el generador de enlaces MVC los
  confunda con los controladores de vistas y dirija Crear/Editar hacia JSON.
- Los contratos HTTP usan DTOs de entrada/salida con validación, paginación,
  filtros y ordenamiento. Las entidades de Domain/EF Core no forman parte de
  ninguna firma de controlador.
- Swagger/OpenAPI está habilitado en `/swagger`, con descripciones y ejemplos,
  y el ensamblado API se carga también en el host Web para que el enlace del
  menú funcione en el despliegue actual.
- Un middleware global traduce excepciones de aplicación a respuestas
  `ProblemDetails` con 400, 404, 409, 422 o 500 según corresponda. Los errores
  incluyen `errorCode`, `correlationId` y el encabezado `X-Correlation-ID`.
- El identificador de correlación forma parte del alcance de logs. Los fallos
  desconocidos se registran internamente, pero su respuesta usa un detalle
  genérico que no expone stack traces, SQL, secretos ni rutas del servidor.
- La validación automática de DTOs también usa el mismo contrato de error con
  el código interno `validation_failed`.
- `docs/api-requests.http` documenta y permite ejecutar el CRUD y las acciones
  específicas de los cinco recursos. Usa respuestas nombradas para encadenar
  IDs y versiones sin copiarlos manualmente, e incluye casos representativos
  de 400, 404, 409, 422 y el contrato seguro esperado para 500.

### Resultado técnico

- Compilación Release: 0 errores y 0 advertencias.
- Pruebas unitarias: 149 aprobadas.
- Pruebas de integración: 20 aprobadas.
- Pruebas funcionales y contractuales: 8 aprobadas.
- Total: 177 de 177 pruebas aprobadas.
- `docker compose up --build` levanta PostgreSQL 16 y la aplicación.
- Health checks de `app` y `db`: saludables.
- `GET /health`: HTTP 200.

### Pequeña liberación

**Candidata:** `v0.3.0-iteracion3`
**Estado:** construida y disponible localmente; tag pendiente de aceptación.

La liberación permite:

1. Administrar niveles de aprobación con rangos sin traslapes y un único rango
   abierto, y resolver el aprobador según el monto de la mejor oferta.
2. Administrar tipos de cambio CRC/USD con un único activo y fecha de vigencia.
3. Alternar la visualización CRC/USD en el detalle de licitación sin modificar
   los montos almacenados, mostrando la tasa y su fecha de vigencia.
4. Navegar por la landing page explicativa, el menú principal y el modo
   claro/oscuro persistente.
5. Ver validaciones junto al campo, tablas paginadas/filtradas/ordenables,
   mensajes de éxito/advertencia/error y confirmación antes de eliminar.
6. Consumir la API REST versionada en `/api/v1` con DTOs, Swagger y errores
   `ProblemDetails` estandarizados.
7. Ejecutar la colección reproducible de solicitudes en `docs/api-requests.http`.

El procedimiento de demostración y aceptación está en
[`releases/iteracion-3.md`](releases/iteracion-3.md).

### Retroalimentación del cliente

**Estado:** Aceptada con ajustes.

- **Fecha de revisión:** 05/08/2026
- **Nombre o rol del cliente:** Robert Granados
- **Funcionalidad aceptada:** Landing page, menú de navegación, modo claro/oscuro, CRUD de niveles de aprobación, resolución del aprobador, CRUD de tipo de cambio, conversión CRC/USD, validación en formularios, tablas paginadas, confirmación antes de eliminar, API REST v1 con Swagger
- **Observaciones:** La aplicación es usable de principio a fin y la API documentada facilita la integración con otros clientes. Para el cierre del proyecto se sugiere reforzar la cobertura de pruebas, completar el despliegue en Kubernetes y terminar la documentación.
- **Cambios solicitados:** Completar la Iteración 4: cobertura mínima de pruebas, despliegue en Kubernetes y documentación final de `/docs`.
- **Prioridad de los cambios:** Media
- **Decisión:** Aceptada con ajustes

### Retrospectiva del equipo

- **Qué funcionó bien:** Se completó la experiencia de usuario solicitada desde la Iteración 1; la API quedó versionada, con DTOs y errores estandarizados, y las pruebas de contrato (`ApiContractTests`) y del middleware (`ApiExceptionMiddlewareTests`) validan el contrato HTTP sin depender de una instancia en ejecución.
- **Qué debe mejorar:** Ampliar las pruebas funcionales de extremo a extremo desde el navegador y preparar el despliegue en Kubernetes.
- **Acción concreta para la Iteración 4:**
  - Alcanzar la cobertura mínima (≥80% en Domain/Application y ≥70% global).
  - Completar el despliegue en Kubernetes y el pipeline de CI/CD.
  - Cerrar la documentación de `/docs` (HU-51 a HU-54).
- **Responsable y fecha de seguimiento:**
  - Responsable: Robert Granados
  - Fecha de seguimiento: 12/08/2026

### Condición de cierre

La Iteración 3 podrá marcarse como **cerrada** cuando:

- se ejecute la demo con el cliente;
- se complete la sección de retroalimentación;
- se complete la retrospectiva;
- se incorporen o planifiquen los ajustes aceptados;
- el CI permanezca verde; y
- se cree el tag `v0.3.0-iteracion3` sobre el commit aceptado.

## Iteración 4 — Calidad, despliegue y cierre documental

**Periodo:** en curso; corte documental al 09 de agosto de 2026

**Estado:** cierre técnico parcial; demo final y aceptación pendientes

**Objetivo:** completar trazabilidad y concurrencia, elevar la cobertura,
automatizar Docker/Kubernetes/CI y cerrar la documentación verificable.

### Planning Game

| Bloque | Historias | Resultado al corte | Puntos |
|---|---|---|---:|
| Persistencia avanzada | HU-40 a HU-43 | 4 de 4 completadas | 11 |
| TDD y pruebas | HU-44 a HU-47 | 4 de 4 completadas | 24 |
| Docker, Kubernetes y CI | HU-48 a HU-50 | 3 de 3 completadas | 18 |
| Documentación | HU-51 a HU-54 | HU-51 completada; HU-52 a HU-54 pendientes | 3 de 10 |
| **Total** | **HU-40 a HU-54** | **12 de 15 historias** | **56 de 63** |

**Velocidad planificada:** 63 puntos

**Velocidad observada al corte:** 56 puntos

**Desviación provisional:** -7 puntos, correspondientes a HU-52, HU-53 y
HU-54. La velocidad final se registrará al cerrar la iteración.

### Desarrollo: TDD, diseño simple y trabajo colaborativo

- Las pruebas de auditoría se escribieron con un reloj falso y verifican fechas
  exactas de creación, actualización y borrado lógico.
- Las pruebas de concurrencia verifican el mapeo `row_version` de las cinco
  entidades y el contrato HTTP 409 sin filtrar detalles de EF Core.
- La ampliación de reglas de dominio elevó la suite unitaria a 219 casos y la
  cobertura medida de Domain/Application superó el 80% acordado.
- Docker, los manifiestos y el workflow se validaron con pruebas ejecutables
  (`health`, persistencia real, Kubeconform, Actionlint y auditoría NuGet), no
  solo mediante inspección documental.

### Refactorizaciones relevantes

- Se centralizó la auditoría en `AppDbContext` con `IClock`, retirando accesos
  directos al reloj de Application.
- El mapeo repetido de auditoría/concurrencia se consolidó en
  `ConfigurationHelpers` y las excepciones de concurrencia convergen en un
  `ProblemDetails` estándar.
- El arranque distingue migración automática, migración exclusiva para Job y
  ejecución normal, permitiendo reutilizar una sola imagen en Compose y K8s.
- El CI se reorganizó como cadena de verificaciones con un gate final estable.

### HU-40 — Auditoría CreatedAt/UpdatedAt/DeletedAt

- `AppDbContext` completa los campos de auditoría en `SaveChanges` y
  `SaveChangesAsync` utilizando `IClock`. En una inserción asigna `CreatedAt`
  y `UpdatedAt`; en una modificación conserva el `CreatedAt` original y
  actualiza `UpdatedAt`.
- Los intentos de establecer manualmente las fechas son reemplazados por el
  reloj de la aplicación. Cuando una entidad compatible se marca como
  `Deleted`, el contexto la transforma en una modificación y asigna su fecha
  de borrado lógico, sin eliminar físicamente el registro.
- La migración `20260807120000_HU40AuditCreatedAtImmutable` incorpora triggers
  PostgreSQL que preservan `created_at` y respaldan la asignación de
  `deleted_at`. Los triggers existentes continúan actualizando `updated_at`.
- Se agregaron pruebas para creación y actualización, sustitución de una fecha
  de borrado proporcionada por el llamador y conversión de un borrado físico
  solicitado a borrado lógico.

### Evidencia de HU-40

- Compilación Release: 0 errores y 0 advertencias.
- Pruebas específicas de auditoría: 3 de 3 aprobadas.
- Pruebas unitarias: 149 de 149 aprobadas.
- Pruebas funcionales: 8 de 8 aprobadas.
- El modelo de EF Core no presenta cambios pendientes respecto de las
  migraciones y el script SQL de la migración se genera correctamente.
- Las 17 pruebas de integración basadas en Testcontainers no pudieron iniciarse
  en esta ejecución porque una directiva de Control de aplicaciones de Windows
  bloqueó `Testcontainers.PostgreSql.dll` (`0x800711C7`); las 6 pruebas de
  integración que no dependen de esa DLL sí aprobaron.

### HU-41 — Concurrencia optimista

- Las cinco entidades editables (`Licitacion`, `Proveedor`, `Oferta`,
  `NivelAprobacion` y `TipoCambio`) mapean la columna entera `row_version`
  mediante `IsRowVersion()`. EF Core la incluye como token de concurrencia y
  PostgreSQL la incrementa con `fn_set_audit_fields` en cada actualización.
- Los flujos de edición de licitaciones y proveedores conservan la versión
  leída en el formulario/DTO y la establecen como valor original antes de
  guardar, por lo que una versión desactualizada no puede sobrescribir cambios.
- El middleware de la API traduce tanto las excepciones de concurrencia de
  aplicación como `DbUpdateConcurrencyException` de EF Core a HTTP 409 con
  `ProblemDetails`, `errorCode=concurrency_conflict`, correlación y un mensaje
  seguro orientado al usuario.
- Una prueba de modelo verifica el token en cada entidad editable y una prueba
  funcional valida que la excepción técnica se convierta en el contrato 409 sin
  filtrar su detalle interno.

### Evidencia de HU-41

- Pruebas de modelo de concurrencia: 5 de 5 aprobadas.
- Prueba funcional nueva del HTTP 409: aprobada.
- Pruebas funcionales acumuladas: 9 de 9 aprobadas.
- EF Core informa que no existen cambios pendientes en el modelo.

### HU-42 — Reloj inyectable para pruebas deterministas

- `IClock` permanece definido en Application con la propiedad
  `DateTimeOffset UtcNow`, mientras que Infrastructure aporta `SystemClock` y
  lo registra como singleton para producción.
- Los manejadores de borrado de licitaciones y proveedores, que eran los dos
  accesos directos restantes a `DateTimeOffset.UtcNow` en Application, ahora
  reciben `IClock` y utilizan la hora inyectada.
- Se incorporó un `FakeClock` compartido para pruebas, con operaciones `Set` y
  `Advance`, y las pruebas de borrado comprueban la fecha exacta asignada.
- Domain y Application no contienen llamadas directas a `DateTime.Now`,
  `DateTime.UtcNow`, `DateTimeOffset.Now` ni `DateTimeOffset.UtcNow`.

### Evidencia de HU-42

- Prueba del reloj falso controlable: aprobada.
- Pruebas unitarias acumuladas: 150 de 150 aprobadas.
- Búsqueda estática de accesos directos al reloj en Domain/Application: sin
  coincidencias.

### HU-43 — Migraciones versionadas y datos semilla reproducibles

- Los hosts Web y API crean un alcance de servicios al arrancar y ejecutan
  `Database.MigrateAsync()` antes de configurar el pipeline que acepta
  solicitudes. El proceso registra inicio y finalización; ante un error escribe
  un log crítico y cancela el arranque para no operar sobre un esquema parcial.
- Se agregó la migración incremental
  `20260807150000_HU43LicitacionLifecycleColumns`, que incorpora de forma
  idempotente `publicada_en`, `cerrada_en` y `motivo_cierre`. Estas propiedades
  ya estaban en el modelo actual, pero faltaban en la secuencia de migraciones.
- Los datos semilla de niveles de aprobación y tipo de cambio permanecen en la
  migración inicial con identificadores y fechas deterministas, de modo que una
  base nueva obtiene siempre los mismos valores iniciales.

### Evidencia de HU-43

- EF Core enumera tres migraciones versionadas y no reporta cambios pendientes
  del modelo.
- `docker compose up -d --build app` aplicó las migraciones pendientes antes de
  que el host registrara `Application started`.
- PostgreSQL contiene las tres entradas esperadas en
  `__EFMigrationsHistory` y las tres columnas de ciclo de vida de licitación.
- Un segundo arranque registró `No migrations were applied. The database is
  already up to date`, confirmando que el proceso es idempotente.
- Los contenedores `app` y `db` quedaron saludables y `GET /health` respondió
  HTTP 200.

### HU-48 — Dockerfile multi-stage y Compose completo

- El Dockerfile separa restauración/publicación con el SDK de .NET 9 del stage
  final basado exclusivamente en ASP.NET Runtime 9.
- Los archivos publicados se copian con propiedad `app:app` y el proceso final
  se ejecuta con `USER app`, no como root.
- Compose levanta la aplicación y PostgreSQL 16, usa variables de `.env`, espera
  el health check de la base, expone health check de la aplicación y conserva
  PostgreSQL en el volumen nombrado `postgres-data`.
- El procedimiento de construcción, arranque y conservación del volumen está
  documentado en `docs/docker.md`.

### Evidencia de HU-48

- `docker compose up -d --build`: construcción y arranque correctos.
- Usuario efectivo del contenedor de aplicación: `uid=1654(app)`.
- Se insertó un registro temporal, se ejecutó `docker compose down` sin `-v`,
  se levantaron contenedores nuevos y se recuperó el mismo registro desde el
  volumen persistente. El dato de prueba se retiró después de verificarlo.
- Contenedores `app` y `db`: saludables. `GET /health`: HTTP 200.

### HU-49 — Manifiestos de Kubernetes completos

- `k8s/` contiene Namespace, ConfigMap, Secret con marcadores, PVC, Service y
  StatefulSet para PostgreSQL, Service y Deployment para la aplicación, y un
  Job dedicado a migraciones.
- Ambos workloads definen `startupProbe`, `readinessProbe`, `livenessProbe`,
  solicitudes y límites de CPU/memoria, contextos sin privilegios y
  capacidades Linux eliminadas.
- La imagen admite `Database__MigrationsOnly=true`: aplica migraciones y termina
  con código de salida. El Deployment desactiva la migración automática y un
  initContainer espera la migración esperada en `__EFMigrationsHistory` antes
  de iniciar cada pod.
- PostgreSQL usa un StatefulSet y el PVC `licitaciones-db-data`; las
  credenciales y la cadena de conexión provienen del Secret y no del ConfigMap.
- `docs/kubernetes.md` documenta preparación de imagen/Secret, aplicación,
  verificación, actualización del Job y eliminación segura.

### Evidencia de HU-49

- Kubeconform en modo estricto: 9 recursos válidos, 0 inválidos, 0 errores.
- Análisis sintáctico YAML: 9 de 9 archivos aprobados.
- Imagen ejecutada localmente en modo `MigrationsOnly`: migraciones verificadas
  y proceso finalizado con código 0 sin iniciar el servidor HTTP.

### HU-50 — Pipeline de CI/CD completo

- `.github/workflows/ci.yml` organiza una cadena bloqueante de restauración y
  build, pruebas/cobertura, formato/análisis, imagen Docker, validación
  Kubernetes, auditoría de dependencias y pruebas Playwright.
- El job de dependencias convierte las alertas NuGet `NU1901` a `NU1904` en
  errores e incorpora `actions/dependency-review-action` para pull requests.
- Kubeconform valida en modo estricto los manifiestos y el job Docker confirma
  que la imagen final se ejecuta con el usuario no privilegiado `app`.
- El check final `CI obligatorio` usa `if: always()` y falla cuando cualquiera
  de los jobs requeridos falla o queda omitido, ofreciendo un nombre estable
  para la protección de `main`.
- `docs/ci-cd.md` documenta el orden, los artefactos, las comprobaciones locales
  y la configuración requerida de branch protection.

### Evidencia de HU-50

- Actionlint: workflow válido y sin observaciones.
- `dotnet format --verify-no-changes`: aprobado.
- Auditoría NuGet directa y transitiva: sin paquetes vulnerables conocidos.
- Docker build: aprobado; usuario final `app`.
- Kubeconform v0.6.7 estricto: 9 recursos válidos, 0 errores.

### HU-47 — Cobertura mínima de pruebas

- Se definió el alcance de la medición con el cliente: `Domain` y `Application`
  deben superar el 80% de cobertura de líneas cada una, y el núcleo
  (`Domain` + `Application` + `Infrastructure`) debe superar el 70%.
- Se agregaron pruebas unitarias de entidades para cubrir las líneas sin probar
  de `Licitacion`, `Proveedor`, `Oferta`, `NivelAprobacion` y `TipoCambio`
  (validaciones del constructor, métodos `Actualizar`, `CambiarNombre`,
  `ActualizarFechaCierre`, `ActualizarPresupuesto`, doble eliminación y fechas
  por defecto).
- Se creó `tools/check-coverage.py`, que combina los reportes `cobertura` de
  coverlet, excluye código generado (`obj/**` y `Persistence/Migrations/**`) y
  hace fallar el pipeline si no se alcanzan los umbrales.
- El job `cobertura` del pipeline de CI compila en Release, ejecuta las
  pruebas unitarias, funcionales y de integración con `--collect:"XPlat Code
  Coverage"`, genera un reporte HTML/Cobertura con `reportgenerator`, lo sube
  como artefacto y verifica los umbrales.
- En esta máquina (Windows con Smart App Control activo), el recopilador de
  cobertura que reescribe ensamblados y `Testcontainers.dll` fueron bloqueados
  (`0x800711C7`), por lo que la medición local se hizo con el recopilador de
  perfil y `dotnet-coverage`.

### Evidencia de HU-47

- Pruebas unitarias acumuladas: 219 de 219 aprobadas (se añadieron 30 métodos
  de dominio).
- Cobertura local medida (unitarias): `Domain` 92,41% y `Application` 84,28%,
  por encima del umbral del 80%.
- Cobertura de integración local: solo 11 de 41 pruebas pudieron ejecutarse por
  el bloqueo de `Testcontainers`; el umbral del núcleo (70%) se verifica en CI
  sobre Ubuntu, donde las pruebas de integración con contenedor sí corren.
- `reportgenerator` y `tools/check-coverage.py` coinciden en el cálculo sobre
  los mismos reportes.

### HU-51 — Historias, plan de liberación y bitácora XP

- Se verificó automáticamente que `historias-usuario.md` contiene las 54
  tarjetas consecutivas, todas con prioridad, estimación y criterios de
  aceptación.
- `plan-xp.md` se consolidó como plan real de liberación, con Iteración 0
  habilitante, cuatro iteraciones XP uniformes, puntos planificados/observados,
  candidatas de versión y reglas de integración, TDD, pairing y Planning Game.
- Esta bitácora contiene para cada iteración velocidad, evidencia TDD,
  refactorizaciones, resultado, pequeña liberación, feedback y retrospectiva.
  La Iteración 4 distingue claramente evidencia terminada de pasos pendientes.

### Evidencia de HU-51

- Catálogo: 54 historias, 0 identificadores faltantes y 0 tarjetas
  estructuralmente incompletas.
- Bitácora: 4 de 4 iteraciones con las secciones XP requeridas.
- Coherencia de velocidad: 23/23, 49/49, 40/40 y 56/63 al corte actual.
- No se declararon como realizados tags, despliegues o aceptaciones pendientes.

### Resultado técnico al corte

- Compilación Release sin errores ni advertencias.
- 219 pruebas unitarias y 9 pruebas funcionales aprobadas en la última
  ejecución completa de esas suites.
- Domain alcanzó 92,41% y Application 84,28% de cobertura local unitaria.
- Docker Compose mantiene aplicación y PostgreSQL saludables, ejecuta como
  usuario no privilegiado y conserva datos en un volumen nombrado.
- Los 9 recursos Kubernetes pasan Kubeconform estricto; el despliegue en un
  clúster real queda pendiente porque el contexto local no dispone de API.
- El workflow CI/CD pasa Actionlint y contiene un gate final para bloquear
  fallos u omisiones.

### Pequeña liberación

**Candidata:** `v1.0.0`

**Estado:** no liberada todavía. La aplicación es demostrable mediante Docker
Compose y tiene manifiestos Kubernetes validados, pero el tag final requiere
terminar HU-52 a HU-54, ejecutar CI en GitHub y realizar la demo del cliente.

La candidata incluye auditoría, concurrencia optimista, reloj inyectable,
migraciones reproducibles, cobertura automatizada, imagen no privilegiada,
persistencia Docker, manifiestos Kubernetes y pipeline bloqueante.

### Retroalimentación del cliente

**Estado:** pendiente de la demo final.

- **Fecha prevista de revisión:** al completar HU-52 a HU-54.
- **Funcionalidad preparada para revisión:** HU-40 a HU-51.
- **Observaciones disponibles:** el cliente solicitó ejecutar la Iteración 4
  historia por historia y revisar la documentación; no se registra aceptación
  final hasta recibirla explícitamente.
- **Decisión:** pendiente.

### Retrospectiva provisional

- **Qué funcionó bien:** las verificaciones ejecutables detectaron vacíos que
  la inspección superficial no mostraba, como columnas ausentes en migraciones,
  ejecución Docker como root y falta de un modo exclusivo de migración para K8s.
- **Qué debe mejorar:** ejecutar el despliegue sobre un clúster Kubernetes real
  y confirmar el pipeline en GitHub, no solo sus validaciones locales.
- **Acción para el cierre:** completar HU-52 a HU-54, recopilar la decisión del
  cliente y actualizar velocidad, liberación y retrospectiva como definitivas.

### Condición de cierre

La Iteración 4 permanece abierta hasta que:

- HU-52, HU-53 y HU-54 estén completadas;
- el workflow de GitHub finalice en verde;
- se ejecute o documente la verificación Kubernetes disponible;
- el cliente revise la pequeña liberación y su feedback quede registrado; y
- se cree el tag `v1.0.0` sobre el commit aceptado.
