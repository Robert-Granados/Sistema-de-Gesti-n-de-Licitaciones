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
