# Bitácora XP

Este documento registra el resultado observable de cada iteración. Una iteración
solo se considera cerrada cuando el cliente revisa la pequeña liberación y su
retroalimentación queda registrada.

## Iteración 1 — Cimientos del dominio y proveedores

**Periodo:** cierre técnico realizado el 23 de julio de 2026

**Estado:** pendiente de demo y retroalimentación del cliente

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
**Estado:** construida y disponible localmente; tag pendiente de aceptación.

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

**Estado:** pendiente.

Registrar durante o inmediatamente después de la demo:

- **Fecha de revisión: 23/07/2026**
- **Nombre o rol del cliente: Robert Granados**
- **Funcionalidad aceptada: Editar proveedor, Eliminar proveedor, Consultar detalle, Registrar proveedor, Listar proveedores, Buscar proveedores**
- **Observaciones: La funcionalidad es adecuada y cumple con las expectativas para la iteración 1, pero se requiere mejorar la experiencia del usuario, tmabién es necesario un que la pruebas se puedan realizar con más datos en los diferentes apartados**
- **Cambios solicitados: Mejorar la experiencia del usuario (UX/UI)**
- **Prioridad de los cambios: Media**
- **Decisión:** Aceptada con ajustes

### Retrospectiva del equipo

Completar después de recibir la retroalimentación:

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

**Estado:** pendiente de demo y retroalimentación del cliente

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

**Estado:** pendiente.

Registrar durante o inmediatamente después de la demo:

- **Fecha de revisión:** 31/07/2026
- **Nombre o rol del cliente:** Robert Granados
- **Funcionalidad aceptada:** *(por completar)*
- **Observaciones:** *(por completar)*
- **Cambios solicitados:** *(por completar)*
- **Prioridad de los cambios:** *(por completar)*
- **Decisión:** pendiente

### Retrospectiva del equipo

Completar después de recibir la retroalimentación:

- **Qué funcionó bien:** *(por completar)*
- **Qué debe mejorar:** *(por completar)*
- **Acción concreta para la Iteración 3:** *(por completar)*
- **Responsable y fecha de seguimiento:** *(por completar)*

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

**Inicio:** 02/08/2026
**Estado:** en curso

### Planning Game y alcance iniciado

| Historia | Estado | Estimación |
|---|---|---:|
| HU-26 — CRUD de niveles de aprobación sin traslapes | Completada | 5 |
| HU-27 — Resolver aprobador según monto | Completada | 2 |
| HU-28 — CRUD de tipo de cambio con un único activo | Completada | 3 |
| HU-29 — Alternar visualización CRC/USD sin modificar datos | Completada | 3 |
| HU-30 — Landing page explicativa | Completada | 2 |
| HU-31 — Menú de navegación principal | Completada | 1 |
| HU-32 — Modo claro y modo oscuro persistente | Completada | 2 |

### Evidencia técnica de HU-26 y HU-27

- El servicio de aplicación permite listar, obtener, crear, editar y eliminar
  niveles de aprobación.
- La creación y edición validan los traslapes antes de persistir y rechazan un
  segundo rango abierto.
- PostgreSQL conserva una segunda defensa mediante
  `ex_niveles_rango_sin_traslape` y
  `ux_niveles_aprobacion_unico_abierto`.
- Los tres rangos semilla requeridos permanecen configurados tanto en EF Core
  como en `database_schema.sql`.
- El resolutor consulta los niveles ordenados por monto mínimo, sin una cadena
  fija de `if/else`, y retorna explícitamente `Sin aprobador configurado` cuando
  ningún rango contiene el monto.
- Se agregaron pruebas unitarias para rango traslapado, segundo rango abierto,
  resolución correcta y ausencia de configuración.
- El CRUD de tipos de cambio registra el valor CRC/USD y su fecha de vigencia;
  el dominio rechaza valores menores o iguales a cero.
- La activación se ejecuta en una transacción: desactiva el registro previamente
  activo, activa el seleccionado y confirma ambos cambios juntos.
- El índice parcial `ux_tipos_cambio_unico_activo` y el trigger
  `trg_tipos_cambio_desactivar_previos` respaldan la regla en PostgreSQL.
- El detalle de licitación ofrece un selector CRC/USD que convierte presupuesto
  y ofertas en el navegador usando únicamente la tasa activa cargada desde la
  base de datos local.
- Cada conversión muestra la tasa y su fecha de vigencia. Los formularios y DTO
  de escritura continúan recibiendo CRC, por lo que alternar la vista no
  modifica ni persiste montos en USD y no requiere conexión a Internet.
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

### Validación inicial

- Pruebas unitarias: 149 aprobadas.
- Pruebas de integración: 20 aprobadas.
- Pruebas funcionales base: 1 aprobada.
- Total: 170 de 170 pruebas aprobadas.
