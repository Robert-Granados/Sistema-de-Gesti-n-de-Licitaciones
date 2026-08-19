# Plan de liberación XP — Sistema de Gestión de Licitaciones

Este documento registra el plan de liberación que adopté: cómo se
divide el alcance en iteraciones XP, cómo se integra y versiona el trabajo y qué
evidencia debe existir al cerrar cada pequeña liberación.

El proyecto fue ejecutado por una persona. La aplicación de las prácticas que
requieren varios participantes y el significado de la aceptación están
declarados en [xp-individual.md](xp-individual.md).

---

## 1. Iteración 0 — Preparación (no cuenta como iteración XP, es habilitante)

Antes del Planning Game formal se preparó un esqueleto mínimo para producir
software demostrable desde la primera iteración. Esta preparación correspondió a
**HU-01, HU-02 y HU-03**.

**Trabajo realizado:**
1. Creación del repositorio único en GitHub.
2. Estructura de la solución, Docker Compose base y CI base.
3. Incorporación de `database_schema.sql` como referencia inicial del esquema.
4. Configuración del entorno local.
5. Elaboración del plan XP y del catálogo de historias de usuario.

**Resultado:** repositorio compilable, entorno reproducible con Docker Compose y
CI ejecutable en cada cambio. La funcionalidad de negocio comenzó en la
Iteración 1.

---

## 2. Las cuatro iteraciones XP

El proyecto se organiza en **4 iteraciones de 2 semanas**, con pequeñas
liberaciones demostrables. Las 54 historias se agrupan por dependencia técnica
y la Iteración 0 se considera preparación habilitante, no velocidad funcional.

### Resumen del plan de liberación

| Iteración | Alcance | Plan | Observado | Liberación/estado |
|---|---|---:|---:|---|
| 0 | HU-01 a HU-03 | 9 | 9 | Base técnica habilitante |
| 1 | HU-04 a HU-10 | 23 | 23 | Aceptada; tag `v0.1.0-iteracion1` publicado |
| 2 | HU-11 a HU-25 | 49 | 49 | Aceptada; tag `v0.2.0-iteracion2` publicado |
| 3 | HU-26 a HU-39 | 40 | 40 | Aceptada; tag `v0.3.0-iteracion3` publicado |
| 4 | HU-40 a HU-54 | 63 | 63 | Cerrada; tag final `v1.0.0` publicado |

Los valores observados representan puntos con evidencia técnica terminada. La
Iteración 4 completó HU-52 a HU-54 durante el cierre documental del 18 de agosto
de 2026. Los tags de las cuatro liberaciones están publicados en GitHub.

Cada iteración sigue el mismo ciclo interno:

```
Planning Game → Iteración (desarrollo individual con TDD)
→ Pequeña liberación (demo funcional) → Retroalimentación del cliente
→ Registro en bitacora-xp.md
```

### Iteración 1 — Cimientos del dominio: Proveedores
**Historias:** HU-04, HU-05 (dominio + EF Core), HU-06 a HU-10 (CRUD de Proveedores).
**Objetivo de negocio:** poder registrar y administrar proveedores con las reglas de unicidad y normalización ya funcionando de extremo a extremo (dominio → base de datos → UI).
**Por qué primero:** Proveedor es la entidad más simple y no depende de ninguna otra; valida que el esqueleto EF Core + PostgreSQL + patrón de validación funcione antes de construir sobre él.
**Pequeña liberación:** aplicación desplegable con Docker Compose donde se puede crear, listar, editar y eliminar (lógicamente) proveedores, con pruebas unitarias y de integración pasando en CI.
**Evidencia XP mínima:** historias con estimación y criterios de aceptación,
ciclo rojo-verde-refactor visible y verificación automática. La programación
en pareja no aplica por tratarse de un proyecto individual.

### Iteración 2 — El núcleo del negocio: Licitaciones y Ofertas
**Historias:** HU-11 a HU-25 (Licitaciones completas + Ofertas completas + mejor oferta/clasificación).
**Objetivo de negocio:** ejecutar el flujo funcional mínimo descrito en el enunciado: crear licitación, publicarla, registrar ofertas (válidas y rechazadas), calcular mejor oferta y clasificación.
**Por qué segundo:** es el corazón del sistema y el más rico en reglas de negocio (fechas, vencimiento, transiciones de estado, unicidad compuesta). Se introdujo una abstracción mínima de reloj como decisión técnica habilitante; la revisión formal y cierre completo de HU-42 permanecieron en la Iteración 4, sin contabilizar dos veces sus puntos.
**Pequeña liberación:** flujo completo licitación → publicación → ofertas (válidas y rechazadas) → mejor oferta, demostrable desde la UI y desde la API.
**Evidencia XP mínima:** matriz de transición de estados cubierta por pruebas, evidencia de TDD en los rechazos de negocio (HU-19, HU-20, HU-21), velocidad observada comparada con la Iteración 1 en `bitacora-xp.md`.

### Iteración 3 — Reglas paramétricas, moneda y experiencia de usuario
**Historias:** HU-26 a HU-29 (niveles de aprobación + tipo de cambio), HU-30 a HU-39 (UI/UX completa + API REST versionada con Swagger).
**Objetivo de negocio:** que la aplicación sea usable de principio a fin (landing, navegación, temas, validaciones visibles) y que el aprobador y la conversión CRC/USD aparezcan en el detalle de licitación (cerrando lo que quedó pendiente de HU-13).
**Pequeña liberación:** aplicación navegable completa (landing, menú, modo claro/oscuro, tablas paginadas) más API REST documentada en Swagger, ambas reflejando aprobador y conversión de moneda.
**Evidencia XP mínima:** pruebas funcionales E2E iniciales de los flujos de UI ya construidos (adelanto de HU-46 para los flujos disponibles), diseño simple evidenciado (sin condicionales `if/else` fijos para aprobador, según exige HU-27).

### Iteración 4 — Calidad, persistencia avanzada, contenerización y cierre
**Historias:** HU-40, HU-41, HU-43 (auditoría/concurrencia restante), HU-44 a HU-47 (suite completa de TDD y cobertura), HU-48 a HU-50 (Docker/Kubernetes/CI completo), HU-51 a HU-54 (documentación de cierre).
**Objetivo de negocio:** el sistema es desplegable en Kubernetes, la cobertura mínima se cumple, y toda la documentación de `/docs` queda completa y coherente con lo implementado.
**Pequeña liberación:** candidata `v1.0.0`/`entrega-final`, demostrable con
Docker Compose, preparada para `kubectl apply -f k8s/` y protegida por un
pipeline completo. El tag se publicó después de comprobar el CI remoto en verde.

**Resultado de cierre:** 15 de 15 historias y 63 de 63 puntos completados. La
documentación de arquitectura, datos, módulos, integración, API, pruebas y uso
responsable de IA quedó incorporada al producto. La candidata es demostrable
con Docker Compose; los manifiestos y el workflow están validados localmente.
La publicación del tag y la comprobación del pipeline remoto quedaron completadas.
**Evidencia XP mínima:** reporte de cobertura (≥80% Domain/Application, ≥70% global), evidencia de despliegue en Kubernetes (pods, PVC, logs), `bitacora-xp.md` cerrada con las cuatro iteraciones y su velocidad comparada.

---

## 3. Versionamiento en Git bajo XP

XP no prescribe un modelo de branching específico, pero sí exige **integración continua real** (integrar cambios frecuentemente, no acumular ramas largas) y **propiedad colectiva del código**. Esto se traduce en un flujo **trunk-based** con ramas de vida muy corta.

### 3.1 Estructura de ramas
- `main`: rama protegida. Siempre debe compilar, pasar pruebas y estar desplegable. Requiere Pull Request + CI verde para recibir cambios.
- `feature/HU-XX-slug-corto`: una rama por historia (o por tarea técnica pequeña dentro de una historia grande como HU-37 o HU-49). Vida esperada: horas a 1-2 días, nunca semanas.
- Nada de `develop`, `release/*` ni ramas por "sprint" — eso es vocabulario de Git-Flow/Scrum, no de XP.

```bash
git checkout main
git pull
git checkout -b feature/HU-06-registrar-proveedor
```

### 3.2 Commits: Conventional Commits + ciclo TDD visible
Se adoptó **Conventional Commits**, con `HU-XX` como referencia en el mensaje
cuando la historia contó con un commit individual. Los commits agrupados se
identifican como tales en la [matriz de trazabilidad](matriz-trazabilidad.md).

Formato adoptado:
```
<tipo>(HU-XX): <descripción corta en imperativo>

<cuerpo opcional con detalle>
```

Tipos: `feat`, `fix`, `test`, `refactor`, `docs`, `chore`, `ci`, `build`.

Los ciclos TDD con evidencia directa se registraron mediante commits separados:

```bash
git commit -m "test(HU-44): agrega pruebas unitarias de reglas de negocio (rojo)"
git commit -m "feat(HU-44): cubre conversión CRC/USD y normalización de proveedor (verde)"
git commit -m "refactor(HU-44): simplifica validación de nombre y guardia de conversión"
```

Esta secuencia constituye evidencia directa de TDD y se complementa con la
bitácora y la matriz de trazabilidad.

### 3.3 Programación en parejas en este proyecto individual

La programación en pareja no se aplicó porque hubo un solo desarrollador. La IA
fue una herramienta de asistencia y no se presenta como una segunda persona.
No se utilizaron trailers `Co-authored-by`, porque no existió una segunda
persona participante. El historial identifica a Robert Granados como único
autor humano del proyecto.

### 3.4 Pull Requests como Planning Game en miniatura
Los PR conservaron la integración de ramas de historia y el resultado del CI.
La plantilla incorporada durante el cierre estandariza la referencia al Issue,
los criterios de aceptación, las pruebas y la evidencia XP para cambios
posteriores. La protección de `main` exige el check `8. CI obligatorio`.

### 3.5 Issues y Milestones (permitidos, con vocabulario XP)
- Cada historia dispone de un Issue con rol, valor, estimación y criterios.
- Los cinco Milestones corresponden a la preparación y las cuatro iteraciones.
- Los Issues y Milestones se incorporaron retrospectivamente durante la
  auditoría final; organizan la evidencia, pero no se presentan como prueba de
  uso durante los Planning Games originales.

### 3.6 Etiquetas (tags) de liberación
- Cada iteración cerró con un tag y una Release. La entrega funcional quedó
  identificada mediante `v1.0.0`:

```bash
git tag -a v1.0.0 -m "Entrega final - Sistema de Gestion de Licitaciones"
git push origin v1.0.0
```

### 3.7 Higiene del repositorio

El `.gitignore` excluye archivos `.env`, configuraciones de desarrollo con
credenciales y directorios generados como `bin/`, `obj/` y `node_modules/`.

---

## 4. Entorno utilizado

El desarrollo se realizó con .NET 9, Git, Docker Compose, PostgreSQL 16 y un
editor compatible con C#. El repositorio conserva `.editorconfig` como fuente
de convenciones y el pipeline verifica formato, compilación y pruebas. Las
pruebas de integración utilizan PostgreSQL real mediante Testcontainers y las
pruebas de interfaz utilizan Playwright.

Los procedimientos reproducibles de ejecución se encuentran en
[pruebas](pruebas.md), [Docker](docker.md), [Kubernetes](kubernetes.md) y
[CI/CD](ci-cd.md).

## 5. Secuencia ejecutada

1. Preparación del repositorio, estructura, Compose y CI.
2. Planificación y ejecución de cuatro iteraciones funcionales.
3. Desarrollo incremental con pruebas y refactorización verificables.
4. Demostración, retroalimentación y retrospectiva por iteración.
5. Publicación de pequeñas liberaciones y cierre documental.
