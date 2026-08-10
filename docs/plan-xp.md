# Plan de liberación XP — Sistema de Gestión de Licitaciones

Este documento registra el plan de liberación adoptado por el equipo: cómo se
divide el alcance en iteraciones XP, cómo se integra y versiona el trabajo y qué
evidencia debe existir al cerrar cada pequeña liberación.

---

## 1. Iteración 0 — Preparación (no cuenta como iteración XP, es habilitante)

Antes del Planning Game formal necesitas un mínimo esqueleto para que las iteraciones produzcan software demostrable desde la primera. Esto corresponde a **HU-01, HU-02, HU-03** (fundación) y a dejar el repositorio listo.

**Qué se hace:**
1. Crear el repositorio en GitHub (único repo del proyecto).
2. Ejecutar HU-01 (estructura de solución), HU-02 (Docker Compose base) y HU-03 (CI base).
3. Aplicar el script `database_schema.sql` como primera migración manual de referencia (luego EF Core generará las migraciones formales en HU-05).
4. Configurar el entorno local (sección 3 de esta guía).
5. Redactar `/docs/plan-xp.md` (este documento) y `/docs/historias-usuario.md` (ya generado).

**Salida esperada:** repo con `dotnet build` verde, `docker compose up --build` funcionando, CI ejecutando en cada push. Aún no hay funcionalidad de negocio — eso empieza en la Iteración 1.

---

## 2. Las cuatro iteraciones XP

El proyecto se organiza en **4 iteraciones de 2 semanas**, con pequeñas
liberaciones demostrables. Las 54 historias se agrupan por dependencia técnica
y la Iteración 0 se considera preparación habilitante, no velocidad funcional.

### Resumen del plan de liberación

| Iteración | Alcance | Plan | Observado | Liberación/estado |
|---|---|---:|---:|---|
| 0 | HU-01 a HU-03 | 9 | 9 | Base técnica habilitante |
| 1 | HU-04 a HU-10 | 23 | 23 | Aceptada; tag candidato `v0.1.0-iteracion1` |
| 2 | HU-11 a HU-25 | 49 | 49 | Aceptada; tag candidato `v0.2.0-iteracion2` |
| 3 | HU-26 a HU-39 | 40 | 40 | Aceptada con ajustes; tag candidato `v0.3.0-iteracion3` |
| 4 | HU-40 a HU-54 | 63 | 56 al 09/08/2026 | En curso; candidata `v1.0.0` |

Los valores observados representan puntos con evidencia técnica terminada;
un tag solo se crea después de demo y aceptación. La Iteración 4 conserva 7
puntos pendientes (HU-52 a HU-54).

Cada iteración sigue el mismo ciclo interno:

```
Planning Game (medio día) → Iteración (desarrollo con TDD + pair programming)
→ Pequeña liberación (demo funcional) → Retroalimentación del cliente
→ Registro en bitacora-xp.md
```

### Iteración 1 — Cimientos del dominio: Proveedores
**Historias:** HU-04, HU-05 (dominio + EF Core), HU-06 a HU-10 (CRUD de Proveedores).
**Objetivo de negocio:** poder registrar y administrar proveedores con las reglas de unicidad y normalización ya funcionando de extremo a extremo (dominio → base de datos → UI).
**Por qué primero:** Proveedor es la entidad más simple y no depende de ninguna otra; valida que el esqueleto EF Core + PostgreSQL + patrón de validación funcione antes de construir sobre él.
**Pequeña liberación:** aplicación desplegable con Docker Compose donde se puede crear, listar, editar y eliminar (lógicamente) proveedores, con pruebas unitarias y de integración pasando en CI.
**Evidencia XP mínima:** historias con estimación y criterios de aceptación (ya documentado), ciclo rojo-verde-refactor visible en el historial de HU-06, al menos una sesión de pair programming documentada.

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
**Pequeña liberación:** versión etiquetada `v1.0.0`/`entrega-final`, desplegable con `kubectl apply -f k8s/`, con pipeline de CI completo en verde.
**Evidencia XP mínima:** reporte de cobertura (≥80% Domain/Application, ≥70% global), evidencia de despliegue en Kubernetes (pods, PVC, logs), `bitacora-xp.md` cerrada con las cuatro iteraciones y su velocidad comparada.

> Nota: si el curso exige explícitamente solo 3 iteraciones, fusiona la Iteración 3 y 4 en una sola de mayor duración, pero documenta la razón en `plan-xp.md` — lo importante es que la duración sea uniforme y quede justificada.

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
Usa **Conventional Commits**, con el `HU-XX` como referencia explícita en el cuerpo o en un scope, para que cada commit se pueda vincular a la historia (requisito explícito del proyecto).

Formato sugerido:
```
<tipo>(HU-XX): <descripción corta en imperativo>

<cuerpo opcional con detalle>
```

Tipos: `feat`, `fix`, `test`, `refactor`, `docs`, `chore`, `ci`, `build`.

**El historial de cada historia debe mostrar el ciclo rojo-verde-refactor como commits separados**, no como un único commit gigante:

```bash
git commit -m "test(HU-06): agrega prueba de rechazo por nombre duplicado (rojo)"
git commit -m "feat(HU-06): implementa CrearProveedorCommand con validacion de unicidad (verde)"
git commit -m "refactor(HU-06): extrae normalizador de nombre a servicio reutilizable"
```

Esto es evidencia directa de TDD para el evaluador y para tu propia bitácora.

### 3.3 Programación en parejas: evidencia en el historial
Si trabajas en pareja, **alterna quién hace el commit** y usa el trailer `Co-authored-by` de GitHub en cada commit, sin importar quién tecleó:

```
feat(HU-11): implementa creacion de licitacion con validacion de fecha

Co-authored-by: Nombre Compañero <correo@ejemplo.com>
```

Esto evita que el historial se concentre en una sola cuenta (requisito explícito) y deja evidencia de propiedad colectiva.

### 3.4 Pull Requests como Planning Game en miniatura
Cada PR de una historia debe:
1. Referenciar el issue de la historia (`Closes #HU-06` o el número de issue asociado).
2. Incluir en la descripción los criterios de aceptación copiados de `historias-usuario.md`, marcados como cumplidos.
3. Mostrar CI en verde (build, pruebas, cobertura, análisis estático) antes de poder mergear.
4. Ser revisado (aunque sea por el propio compañero de pareja) antes del merge — esto sustituye la revisión de código formal cuando el equipo es de dos personas.

### 3.5 Issues y Milestones (permitidos, con vocabulario XP)
- Un **Issue** por historia de usuario, título `HU-XX: <nombre>`, con la descripción completa de `historias-usuario.md` pegada (rol/quiero/para + criterios de aceptación).
- Etiquetas por épica (`epica:proveedores`, `epica:ofertas`, etc.) y por tipo (`tipo:defecto` para bugs encontrados durante el desarrollo).
- Un **Milestone por iteración** (`Iteración 1`, `Iteración 2`, ...), nunca "Sprint N". Cada Milestone agrupa las historias planificadas en el Planning Game de esa iteración.
- Nada de "Product Backlog" ni tablero Scrum: si usas GitHub Projects, nómbralo "Historias" o "Tablero XP" y organiza columnas como `Por hacer / En pareja / En revisión / Liberado`, evitando terminología de Scrum/Kanban formal (WIP limits, etc.).

### 3.6 Etiquetas (tags) de liberación
- Al cierre de cada iteración, crea un tag ligero de referencia interna: `v0.1.0-iteracion1`, `v0.2.0-iteracion2`, etc. — esto materializa la "pequeña liberación" exigida.
- Al final del proyecto, la entrega evaluable se marca con `v1.0.0` **o** `entrega-final` (ambos aceptados según el enunciado, elige uno y sé consistente):

```bash
git tag -a v1.0.0 -m "Entrega final - Sistema de Gestion de Licitaciones"
git push origin v1.0.0
```

### 3.7 Qué nunca debe llegar al repositorio
- Archivos `.env`, `appsettings.Development.json` con credenciales reales, carpetas `bin/`, `obj/`, `node_modules/`.
- Confirma tu `.gitignore` desde el primer commit de la Iteración 0, no después.

---

## 4. Configuración del entorno: VS Code + agente de código

### 4.1 Instalación base (una sola vez por máquina)
1. **.NET 9 SDK** — verifica con `dotnet --version`.
2. **Docker Desktop** (o Docker Engine + Compose plugin en Linux) — necesario desde la Iteración 0 y obligatorio para Testcontainers en HU-45.
3. **Git** configurado con tu nombre/correo y, si trabajas en pareja, decide si vas a alternar cuentas locales o usar `Co-authored-by` (recomendado: una sola cuenta activa por sesión + trailer).
4. **cliente de PostgreSQL** (opcional pero útil): `psql` o una extensión de VS Code para inspeccionar la base de datos durante el desarrollo.
5. **VS Code** actualizado.

### 4.2 Extensiones de VS Code recomendadas
- **C# Dev Kit** (incluye C#, IntelliCode, soporte de pruebas de .NET) — esencial para Domain/Application/Infrastructure/Web/Api.
- **Docker** (extensión oficial de Microsoft) — para construir/inspeccionar imágenes y ver logs de contenedores desde el editor.
- **PostgreSQL** (por ejemplo, la extensión de Microsoft o `ckolkman.vscode-postgres`) — consultar tablas y validar los índices/constraints de `database_schema.sql` sin salir del editor.
- **EditorConfig for VS Code** — respeta el `.editorconfig` de HU-01.
- **GitLens** — muy útil para verificar que el historial de commits realmente evidencia el ciclo TDD y la alternancia de pareja (te lo van a evaluar).
- **YAML** (Red Hat) — para editar los manifiestos de `/k8s` y los workflows de GitHub Actions con autocompletado.
- **REST Client** o el uso del archivo `.http` nativo de VS Code — para ejecutar la colección de `/docs/api-requests.http` (HU-39) directamente desde el editor.
- Extensión del **agente de código** que vayas a usar (ver 4.4).

### 4.3 Configuración de workspace
Crea en la raíz del repo:

**`.editorconfig`** (mínimo):
```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
insert_final_newline = true
charset = utf-8-bom

[*.{yml,yaml,json,md}]
indent_style = space
indent_size = 2
```

**`.vscode/tasks.json`** (tareas rápidas para el ciclo TDD):
```json
{
  "version": "2.0.0",
  "tasks": [
    { "label": "build", "type": "shell", "command": "dotnet build" },
    { "label": "test-unit", "type": "shell", "command": "dotnet test Tests.Unit" },
    { "label": "test-watch", "type": "shell", "command": "dotnet watch test --project Tests.Unit" },
    { "label": "compose-up", "type": "shell", "command": "docker compose up --build" },
    { "label": "ef-migrate", "type": "shell", "command": "dotnet ef database update --project Infrastructure" }
  ]
}
```
`test-watch` es la tarea que más vas a usar en TDD: deja las pruebas corriendo en rojo/verde mientras escribes.

**`.vscode/settings.json`** (mínimo):
```json
{
  "dotnet.defaultSolution": "SistemaLicitaciones.sln",
  "editor.formatOnSave": true,
  "files.exclude": { "**/bin": true, "**/obj": true }
}
```





---

## 5. Orden de arranque (resumen accionable)

1. Crear repo, `.gitignore`, `.editorconfig`, tablero de historias (Issues + Milestones).
2. Ejecutar Iteración 0: estructura de solución, Docker Compose base, CI base, entorno VS Code configurado.
3. Planning Game de la Iteración 1 (estimar/confirmar HU-04 a HU-10) y arrancar con `feature/HU-04-...` en pareja, TDD desde el primer commit.
4. Repetir el ciclo Planning Game → desarrollo → pequeña liberación → retro → bitácora para las Iteraciones 2, 3 y 4 descritas arriba.
5. Cerrar con etiqueta `v1.0.0`/`entrega-final` solo cuando `/docs` esté completo y el pipeline de CI (incluyendo validación de Kubernetes) esté en verde.
