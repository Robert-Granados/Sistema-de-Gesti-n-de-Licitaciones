# Historias de Usuario — Sistema de Gestión de Licitaciones (XP)

Formato de tarjeta XP: **Como** \<rol\> **quiero** \<capacidad\> **para** \<valor de negocio\>.
Cada historia incluye prioridad (MoSCoW), estimación en puntos ideales (Fibonacci: 1, 2, 3, 5, 8) y criterios de aceptación verificables. Todas las historias deben vincularse a commits, pruebas e issues, según lo exige el enunciado del proyecto.

Convención de nombres de historia: `HU-XX`. Convención de rama sugerida: `feature/HU-XX-slug`.

**Control del catálogo:** 54 historias numeradas consecutivamente de HU-01 a
HU-54. Cada tarjeta conserva rol/objetivo/valor, prioridad, estimación, criterios
de aceptación. Última verificación documental: 09/08/2026.

---

## Épica 0 — Fundación del proyecto

### HU-01 — Inicializar estructura de solución modular
**Como** equipo de desarrollo **quiero** una solución .NET 9 organizada en proyectos separados **para** aplicar separación de responsabilidades desde el inicio.
- Prioridad: Alta (Debe) · Estimación: 3

**Criterios de aceptación**
- Existen los proyectos `Domain`, `Application`, `Infrastructure`, `Web` (MVC), `Api`, `Tests.Unit`, `Tests.Integration`, `Tests.Functional`.
- `Domain` no referencia ningún paquete de infraestructura ni ORM.
- La solución compila con `dotnet build` sin errores ni advertencias evitables.
- Existen carpetas raíz `/docs` y `/k8s` con un `README.md` inicial en cada una.

---

### HU-02 — Configurar Docker Compose base
**Como** desarrollador **quiero** levantar la aplicación y PostgreSQL con un solo comando **para** tener un entorno reproducible.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- `docker compose up --build` levanta servicio `app` y servicio `db` (postgres:16).
- El servicio `db` expone un volumen persistente nombrado.
- Variables sensibles (cadena de conexión, credenciales) se inyectan por variables de entorno, no están hardcodeadas.
- Existe healthcheck configurado para `db` y para `app`.

---

### HU-03 — Pipeline de integración continua base
**Como** equipo **quiero** que cada cambio se compile y pruebe automáticamente **para** detectar errores temprano (XP: integración continua).
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- Workflow de GitHub Actions se ejecuta en cada push y pull request contra `main`.
- El workflow ejecuta `dotnet restore`, `dotnet build`, `dotnet test`.
- El pipeline falla (bloquea el merge) si alguna prueba falla o si la compilación genera advertencias marcadas como error.

---

## Épica 1 — Dominio y modelo de datos

### HU-04 — Modelar entidades de dominio
**Como** desarrollador **quiero** entidades de dominio ricas y sin dependencias externas **para** encapsular las reglas de negocio (XP: diseño simple).
- Prioridad: Alta · Estimación: 5

**Criterios de aceptación**
- Existen las clases `Licitacion`, `Proveedor`, `Oferta`, `NivelAprobacion`, `TipoCambio` en `Domain`.
- Cada entidad expone únicamente comportamientos válidos (por ejemplo, `Licitacion.Publicar()`, no un setter público de `Estado`).
- Existe un enum `EstadoLicitacion { Borrador, Publicada, Cerrada }`.
- Existen pruebas unitarias que instancian cada entidad y verifican sus invariantes básicas (campos obligatorios, valores por defecto).

---

### HU-05 — Configurar EF Core, migraciones y datos semilla
**Como** desarrollador **quiero** persistir el dominio en PostgreSQL mediante EF Core 9 **para** contar con almacenamiento relacional confiable.
- Prioridad: Alta · Estimación: 5

**Criterios de aceptación**
- Existe `AppDbContext` en `Infrastructure` con `DbSet` para cada entidad.
- Las entidades de EF Core no se exponen directamente en controladores (se usan DTOs).
- La primera migración crea el esquema equivalente al script SQL entregado (ver `database_schema.sql`).
- Existen datos semilla para: los tres estados de licitación (si se modela como catálogo), los tres niveles de aprobación iniciales y un tipo de cambio activo inicial.
- `dotnet ef database update` aplica correctamente contra PostgreSQL 16 en contenedor.

---

## Épica 2 — Proveedores

### HU-06 — Registrar proveedor
**Como** administrador **quiero** registrar un proveedor con nombre válido y único **para** poder asociarle ofertas.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- Dado un nombre con letras, números, espacios, punto, coma o paréntesis, cuando se registra, entonces se crea el proveedor.
- Dado un nombre con símbolos no permitidos, cuando se intenta registrar, entonces la operación se rechaza con mensaje claro.
- Dado un nombre que normalizado (trim, espacios repetidos colapsados, sin distinción de mayúsculas/minúsculas, Unicode normalizado) coincide con uno existente, cuando se intenta registrar, entonces se rechaza por duplicado (validado en UI, servidor y base de datos).
- El `Id` se genera automáticamente y no es editable.

---

### HU-07 — Listar proveedores con paginación, filtro y orden
**Como** administrador **quiero** ver la lista de proveedores paginada, filtrable y ordenable **para** ubicar proveedores rápidamente.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- La lista soporta parámetros `page`, `pageSize`, `search` (por nombre) y `sortBy`.
- Respuesta incluye metadatos de paginación (total de registros, página actual, total de páginas).
- Los proveedores con borrado lógico no aparecen por defecto.

---

### HU-08 — Consultar detalle de proveedor con ofertas relacionadas
**Como** administrador **quiero** ver el detalle de un proveedor y sus ofertas **para** conocer su participación histórica.
- Prioridad: Media · Estimación: 2

**Criterios de aceptación**
- La vista de detalle muestra datos del proveedor y una tabla de sus ofertas (licitación, monto, fecha, estado de la licitación).
- Si el proveedor no existe o fue eliminado lógicamente, se responde 404.

---

### HU-09 — Editar proveedor
**Como** administrador **quiero** editar el nombre de un proveedor **para** corregir datos manteniendo la unicidad.
- Prioridad: Media · Estimación: 2

**Criterios de aceptación**
- La edición revalida unicidad del nombre normalizado excluyendo el propio registro.
- Se actualiza `UpdatedAt` automáticamente.
- Conflictos de concurrencia optimista devuelven un mensaje controlado (no un error técnico crudo).

---

### HU-10 — Eliminar (borrado lógico) proveedor
**Como** administrador **quiero** eliminar un proveedor sin perder el historial de ofertas **para** mantener integridad referencial.
- Prioridad: Media · Estimación: 3

**Criterios de aceptación**
- Si el proveedor tiene ofertas asociadas, la eliminación física se rechaza; se aplica borrado lógico (`DeletedAt`).
- Si el proveedor no tiene ofertas, puede eliminarse físicamente o aplicarse el mismo borrado lógico de forma consistente con la política elegida.
- Se solicita confirmación explícita antes de eliminar (UI).
- El proveedor eliminado lógicamente no aparece en listados ni puede recibir nuevas ofertas.

---

## Épica 3 — Licitaciones

### HU-11 — Crear licitación
**Como** administrador **quiero** crear una licitación con código único y fecha de cierre futura **para** iniciar el proceso de recepción de ofertas.
- Prioridad: Alta · Estimación: 5

**Criterios de aceptación**
- El código es único ignorando espacios laterales y mayúsculas/minúsculas (validado en UI, servidor y base de datos).
- El presupuesto estimado es mayor que cero.
- La fecha y hora de cierre se seleccionan mediante control de calendario/hora, no solo texto libre, y deben ser futuras al momento de creación.
- La licitación se crea en estado `Borrador`.
- Las fechas se almacenan con `DateTimeOffset`/`timestamptz`; las comparaciones internas usan UTC y la presentación usa zona horaria `America/Costa_Rica`.

---

### HU-12 — Listar licitaciones con paginación, filtro y orden
**Como** usuario **quiero** listar licitaciones filtrando por estado, código o rango de fechas **para** ubicar procesos específicos.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- Soporta filtros combinables por estado y por texto de código/título.
- Una licitación cuya `FechaCierre` ya pasó se muestra como "cerrada funcionalmente" en la UI aunque el campo `Estado` almacenado todavía diga `Publicada`.

---

### HU-13 — Consultar detalle de licitación con mejor oferta y aprobador
**Como** usuario **quiero** ver el detalle completo de una licitación **para** conocer sus ofertas, la mejor oferta, su clasificación y el aprobador correspondiente.
- Prioridad: Alta · Estimación: 5

**Criterios de aceptación**
- Se listan todas las ofertas válidas de la licitación.
- Se muestra la mejor oferta (menor monto en CRC; en empate, la registrada primero) o "Sin ofertas válidas" si no hay ofertas.
- Se muestra la clasificación de ahorro: "Oferta conveniente" (≥10% de ahorro), "Oferta aceptable" (>0% y <10%), "Oferta válida sin ahorro" (igual al presupuesto).
- Se muestra el aprobador correspondiente según el monto de la mejor oferta y la tabla de niveles de aprobación.
- Los montos se muestran en CRC con formato cultural `es-CR`, con opción de alternar a USD usando el tipo de cambio activo, mostrando la fecha de dicho tipo de cambio.

---

### HU-14 — Editar licitación
**Como** administrador **quiero** editar los datos de una licitación mientras esté en estado editable **para** corregir información antes del cierre.
- Prioridad: Media · Estimación: 3

**Criterios de aceptación**
- No se permite reducir el presupuesto por debajo del monto de una oferta ya registrada.
- No se permite editar una licitación cerrada (formal o funcionalmente).
- Conflictos de concurrencia se manejan igual que en HU-09.

---

### HU-15 — Publicar licitación
**Como** administrador **quiero** publicar una licitación en `Borrador` **para** habilitar la recepción de ofertas.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- Solo se permite `Borrador → Publicada`.
- Se exige presupuesto válido (>0) y fecha de cierre futura para permitir la transición.
- Un intento de publicar una licitación con datos incompletos se rechaza con mensaje claro.

---

### HU-16 — Cerrar licitación (manual y por vencimiento)
**Como** sistema/administrador **quiero** cerrar una licitación manualmente o reconocer su cierre automático por vencimiento **para** impedir nuevas ofertas fuera de plazo.
- Prioridad: Alta · Estimación: 5

**Criterios de aceptación**
- `Publicada → Cerrada` es permitida por acción autorizada o al alcanzar la fecha de cierre.
- `Borrador → Cerrada` es permitida como cancelación documentada.
- `Publicada → Borrador` y `Cerrada → Publicada/Borrador` no están permitidas (salvo regla de reapertura aprobada explícitamente, fuera de alcance por defecto).
- Las ofertas de una licitación cerrada quedan inmutables (no se pueden crear, editar ni eliminar).

---

### HU-17 — Eliminar (borrado lógico) licitación
**Como** administrador **quiero** eliminar una licitación sin ofertas relacionadas, o aplicar borrado lógico si las tiene **para** mantener la integridad del historial.
- Prioridad: Media · Estimación: 3

**Criterios de aceptación**
- Igual patrón que HU-10 pero para licitaciones y sus ofertas asociadas.
- Se solicita confirmación antes de eliminar.

---

## Épica 4 — Ofertas

### HU-18 — Registrar oferta válida
**Como** proveedor/administrador **quiero** registrar una oferta económica para una licitación publicada **para** participar en el proceso.
- Prioridad: Alta · Estimación: 5

**Criterios de aceptación**
- La licitación debe estar `Publicada` y no vencida (`FechaCierre > ahora UTC`).
- El monto debe ser mayor que cero y menor o igual al presupuesto estimado.
- Un proveedor no puede tener más de una oferta por licitación (índice único compuesto `LicitacionId + ProveedorId`).
- El `Id` y `FechaRegistro` se generan automáticamente.

---

### HU-19 — Rechazar oferta duplicada
**Como** sistema **quiero** rechazar una segunda oferta del mismo proveedor para la misma licitación **para** garantizar unicidad de participación.
- Prioridad: Alta · Estimación: 2

**Criterios de aceptación**
- Un segundo intento de oferta del mismo proveedor/licitación se rechaza con mensaje claro, sin afectar la oferta original.

---

### HU-20 — Rechazar oferta que excede el presupuesto
**Como** sistema **quiero** rechazar ofertas superiores al presupuesto estimado **para** cumplir la regla de negocio de tope presupuestario.
- Prioridad: Alta · Estimación: 2

**Criterios de aceptación**
- Un monto igual al presupuesto es válido.
- Un monto mayor al presupuesto se rechaza.

---

### HU-21 — Rechazar oferta vencida o de licitación cerrada
**Como** sistema **quiero** impedir ofertas cuando la fecha de cierre ya pasó o la licitación está cerrada **para** respetar los plazos del proceso.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- Igual fecha/hora actual que la de cierre se considera vencida (no se acepta la oferta).
- Ofertas ya registradas en licitaciones cerradas no pueden editarse ni eliminarse.

---

### HU-22 — Listar y filtrar ofertas por licitación y proveedor
**Como** usuario **quiero** filtrar ofertas por licitación y/o proveedor **para** analizar la participación.
- Prioridad: Media · Estimación: 2

**Criterios de aceptación**
- Filtros combinables `licitacionId` y `proveedorId`, con paginación y orden por monto o fecha.

---

### HU-23 — Editar oferta
**Como** administrador **quiero** editar el monto de una oferta mientras la licitación siga abierta y no vencida **para** corregir errores de captura.
- Prioridad: Media · Estimación: 3

**Criterios de aceptación**
- Se revalidan todas las reglas de HU-18 (monto positivo, ≤ presupuesto, licitación publicada y no vencida) al editar.
- No se permite editar ofertas de licitaciones cerradas.

---

### HU-24 — Eliminar oferta
**Como** administrador **quiero** eliminar una oferta únicamente mientras la licitación esté abierta **para** mantener como evidencia las ofertas de licitaciones cerradas.
- Prioridad: Media · Estimación: 2

**Criterios de aceptación**
- Se rechaza la eliminación si la licitación está cerrada (formal o funcionalmente).
- Se solicita confirmación antes de eliminar.

---

### HU-25 — Calcular mejor oferta y clasificación de ahorro
**Como** usuario **quiero** ver automáticamente la mejor oferta y su clasificación de ahorro **para** tomar decisiones sin cálculos manuales.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- Cubre los cinco casos: sin ofertas, ahorro ≥10%, ahorro entre 0% y 10%, oferta igual al presupuesto, y desempate por orden de registro.

---

## Épica 5 — Niveles de aprobación

### HU-26 — CRUD de niveles de aprobación con rangos no traslapados
**Como** administrador **quiero** definir rangos de monto y su aprobador correspondiente **para** parametrizar la aprobación sin lógica condicional fija en el código.
- Prioridad: Alta · Estimación: 5

**Criterios de aceptación**
- Los rangos no pueden traslaparse entre sí.
- Solo puede existir un rango abierto (sin monto máximo).
- Datos semilla iniciales: `0.01–999999.99 → Encargado de área`, `1000000.00–9999999.99 → Gerencia`, `10000000.00–sin límite → Junta Directiva`.

---

### HU-27 — Resolver aprobador según monto
**Como** sistema **quiero** determinar el aprobador correspondiente a partir del monto de la mejor oferta **para** enrutar la aprobación automáticamente.
- Prioridad: Alta · Estimación: 2

**Criterios de aceptación**
- Dado un monto, el sistema retorna el nivel de aprobación cuyo rango lo contiene.
- Si ningún rango contiene el monto (dato inconsistente), se retorna un resultado explícito de "sin aprobador configurado", nunca una excepción no controlada visible al usuario.

---

## Épica 6 — Tipo de cambio

### HU-28 — CRUD de tipo de cambio con un único activo
**Como** administrador **quiero** gestionar los tipos de cambio CRC/USD **para** mantener actualizada la referencia de conversión.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- `CRCporUSD` debe ser mayor que cero.
- Solo puede existir un registro `Activo = true` a la vez; activar uno nuevo desactiva el anterior en la misma transacción.
- Se registra `FechaVigencia`.

---

### HU-29 — Alternar visualización CRC/USD sin modificar datos
**Como** usuario **quiero** alternar la visualización de montos entre CRC y USD **para** interpretar la información en la moneda que prefiera.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- El valor almacenado siempre permanece en CRC; el cambio de vista es puramente de presentación.
- Se muestra la fecha del tipo de cambio utilizado junto al monto convertido.
- El sistema funciona sin conexión a Internet usando el tipo de cambio activo administrado localmente (no se consulta un servicio externo).

---

## Épica 7 — Landing page, navegación y experiencia de usuario

### HU-30 — Landing page explicativa
**Como** visitante **quiero** entender el propósito de la aplicación al ingresar **para** orientarme antes de operar el sistema.
- Prioridad: Alta · Estimación: 2

**Criterios de aceptación**
- La página explica el flujo de licitación, ofertas, mejor oferta, nivel de aprobación y conversión monetaria.
- El diseño es adaptable (responsive) a escritorio y móvil.

---

### HU-31 — Menú de navegación principal
**Como** usuario **quiero** un menú visible con acceso a los módulos principales **para** desplazarme fácilmente por el sistema.
- Prioridad: Alta · Estimación: 1

**Criterios de aceptación**
- El menú incluye: Inicio, Licitaciones, Proveedores, Ofertas, Niveles de aprobación, Tipo de cambio y documentación interactiva de la API (Swagger).
- El menú es visible y usable en dispositivos móviles (colapsable).

---

### HU-32 — Modo claro y modo oscuro persistente
**Como** usuario **quiero** alternar entre modo claro y oscuro y que se recuerde mi preferencia **para** una experiencia visual cómoda.
- Prioridad: Media · Estimación: 2

**Criterios de aceptación**
- Control visible para alternar el tema.
- La preferencia persiste entre sesiones (por ejemplo, `localStorage` o cookie).

---

### HU-33 — Formularios con validación junto al campo
**Como** usuario **quiero** ver errores de validación justo al lado del campo correspondiente **para** corregir datos rápidamente.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- Cada formulario (proveedor, licitación, oferta, nivel de aprobación, tipo de cambio) muestra mensajes de validación por campo, tanto del lado cliente como del servidor.

---

### HU-34 — Tablas con paginación, filtrado y ordenamiento en la interfaz
**Como** usuario **quiero** que las tablas de listados permitan paginar, filtrar y ordenar **para** encontrar información eficientemente.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- Aplica a los listados de licitaciones, proveedores y ofertas.
- Los controles de paginación/orden/filtro son consistentes entre módulos.

---

### HU-35 — Mensajes de éxito, advertencia y error
**Como** usuario **quiero** recibir retroalimentación visual clara tras cada operación **para** saber si mi acción tuvo éxito o falló.
- Prioridad: Media · Estimación: 2

**Criterios de aceptación**
- Cada operación CRUD muestra una notificación (toast o alerta) categorizada como éxito, advertencia o error, con texto comprensible (sin detalles técnicos internos).

---

### HU-36 — Confirmación antes de eliminar
**Como** usuario **quiero** confirmar explícitamente antes de eliminar un registro **para** evitar eliminaciones accidentales.
- Prioridad: Media · Estimación: 1

**Criterios de aceptación**
- Toda acción de eliminación (proveedor, licitación, oferta, nivel de aprobación, tipo de cambio) exige un diálogo de confirmación previo.

---

## Épica 8 — API REST

### HU-37 — Exponer API REST versionada con DTOs y Swagger
**Como** integrador **quiero** consumir una API REST documentada **para** operar el sistema desde otros clientes.
- Prioridad: Alta · Estimación: 8

**Criterios de aceptación**
- Endpoints mínimos para Licitaciones, Proveedores, Ofertas, Niveles de aprobación y Tipo de cambio (CRUD + acciones específicas como publicar/cerrar).
- La API está versionada (`/api/v1/...`).
- Ningún endpoint expone directamente entidades de EF Core; todos usan DTOs de entrada/salida.
- Documentación OpenAPI/Swagger disponible y navegable desde el menú (HU-31).
- Listados soportan paginación, filtrado y ordenamiento vía query string.

---

### HU-38 — Manejo estandarizado de errores en la API
**Como** integrador **quiero** recibir respuestas de error consistentes **para** manejar fallos de forma predecible desde mi cliente.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- Se usan los códigos: `200 OK`, `201 Created`, `204 No Content`, `400 Bad Request`, `404 Not Found`, `409 Conflict`, `422 Unprocessable Entity` (cuando corresponda) y `500` mediante respuesta controlada.
- Todas las respuestas de error siguen el formato `ProblemDetails` con título, estado, detalle seguro, código de error interno e identificador de correlación.
- No se exponen stack traces, rutas internas, consultas SQL ni secretos al cliente.

---

### HU-39 — Colección reproducible de solicitudes de API
**Como** desarrollador **quiero** una colección de solicitudes de ejemplo para cada endpoint **para** probar la API sin escribirlas manualmente.
- Prioridad: Media · Estimación: 2

**Criterios de aceptación**
- Existe un archivo (colección Postman/Insomnia o `.http` de VS Code) dentro de `/docs` con ejemplos de cada endpoint, incluyendo casos de error representativos.

---

## Épica 9 — Persistencia, auditoría y concurrencia

### HU-40 — Auditoría CreatedAt/UpdatedAt/DeletedAt
**Como** sistema **quiero** registrar automáticamente cuándo se crea, actualiza o elimina lógicamente un registro **para** tener trazabilidad.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- `CreatedAt` se asigna una sola vez al insertar y nunca se modifica.
- `UpdatedAt` se actualiza en cada modificación.
- `DeletedAt` se asigna solo cuando aplica borrado lógico.

---

### HU-41 — Concurrencia optimista
**Como** sistema **quiero** detectar ediciones concurrentes sobre el mismo registro **para** evitar sobrescrituras silenciosas.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- Cada entidad editable tiene una columna de versión de concurrencia.
- Una edición basada en una versión desactualizada es rechazada con un mensaje controlado, no con una excepción técnica cruda.

---

### HU-42 — Reloj inyectable para pruebas deterministas
**Como** desarrollador **quiero** abstraer el acceso a la fecha/hora actual **para** poder escribir pruebas deterministas de vencimiento y cierre.
- Prioridad: Alta · Estimación: 2

**Criterios de aceptación**
- Existe una interfaz `IClock` con implementación real (`UtcNow` del sistema) y una implementación falsa/controlable para pruebas.
- Ningún componente de dominio o aplicación llama directamente a `DateTime.Now`/`DateTime.UtcNow`.

---

### HU-43 — Migraciones versionadas y datos semilla reproducibles
**Como** desarrollador **quiero** que el esquema de base de datos se cree y actualice mediante migraciones versionadas **para** reproducir el entorno en cualquier máquina.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- `dotnet ef migrations add` genera migraciones incrementales versionadas en el repositorio.
- Al iniciar el contenedor `app`, las migraciones pendientes se aplican de forma controlada antes de aceptar tráfico.

---

## Épica 10 — TDD y pruebas automatizadas

### HU-44 — Suite de pruebas unitarias de reglas de negocio
**Como** equipo **quiero** cubrir con pruebas unitarias todas las reglas críticas de negocio **para** prevenir regresiones (XP: TDD).
- Prioridad: Alta · Estimación: 8

**Criterios de aceptación**
- Existen pruebas unitarias, escritas antes o junto con la implementación, para: presupuesto/oferta mayores que cero, rechazo de oferta superior al presupuesto, oferta duplicada, estado no publicado, vencimiento, normalización y duplicidad de proveedor, código único, mejor oferta y desempate, clasificación de ahorro, nivel de aprobación, conversión CRC/USD y transiciones de estado.
- El historial de commits evidencia el ciclo rojo-verde-refactorización (commit de prueba que falla, commit de implementación mínima, commit de refactorización).

---

### HU-45 — Pruebas de integración contra PostgreSQL real
**Como** equipo **quiero** validar migraciones, índices, restricciones, transacciones y concurrencia contra una instancia real de PostgreSQL **para** garantizar que las reglas también se cumplen en la base de datos.
- Prioridad: Alta · Estimación: 5

**Criterios de aceptación**
- Las pruebas de integración usan Testcontainers (u otro mecanismo equivalente) con PostgreSQL real, no SQLite ni un motor en memoria.
- Se cubren: aplicación de migraciones, violación de índices únicos, violación de claves foráneas, restricciones `CHECK`, transacciones multi-registro y conflictos de concurrencia optimista.

---

### HU-46 — Pruebas funcionales de extremo a extremo
**Como** equipo **quiero** automatizar los flujos completos desde el navegador **para** validar la experiencia real del usuario.
- Prioridad: Alta · Estimación: 8

**Criterios de aceptación**
- Se cubren con Playwright o Selenium: landing page y navegación, creación/edición de proveedor, creación/publicación/cierre de licitación, registro y rechazo de ofertas, modo claro/oscuro, conversión CRC/USD, mensajes de validación y CRUD completo desde navegador.

---

### HU-47 — Cobertura mínima de pruebas
**Como** equipo **quiero** medir y exigir un umbral mínimo de cobertura **para** asegurar un nivel base de verificación automatizada.
- Prioridad: Media · Estimación: 3

**Criterios de aceptación**
- `Domain` y `Application` alcanzan al menos 80% de cobertura de líneas.
- El proyecto completo alcanza al menos 70% de cobertura de líneas.
- El pipeline de CI reporta la cobertura y falla si no se alcanza el umbral.

---

## Épica 11 — Docker, Kubernetes e integración continua

### HU-48 — Dockerfile multi-stage y Compose completo
**Como** equipo **quiero** una imagen Docker optimizada y un Compose completo con persistencia **para** ejecutar la solución de forma reproducible.
- Prioridad: Alta · Estimación: 5

**Criterios de aceptación**
- `Dockerfile` multi-stage (build + runtime) compatible con .NET 9, usuario no privilegiado en el stage final cuando sea viable.
- `docker compose up --build` levanta app + PostgreSQL con volumen persistente, variables de entorno y health checks.
- Los datos persisten después de reiniciar los contenedores.

---

### HU-49 — Manifiestos de Kubernetes completos
**Como** equipo **quiero** desplegar la solución en Kubernetes con persistencia y configuración segura **para** demostrar un despliegue productivo.
- Prioridad: Alta · Estimación: 8

**Criterios de aceptación**
- Existen: `Deployment` para la app, `StatefulSet` (o equivalente) para PostgreSQL, `Service` para ambos, `PersistentVolumeClaim`, `ConfigMap` y `Secret`.
- Se configuran `startupProbe`, `readinessProbe` y `livenessProbe`.
- Se definen `resources.requests` y `resources.limits`.
- Las migraciones se ejecutan de forma controlada (por ejemplo, `Job`/`initContainer`) antes de que el `Deployment` reciba tráfico.

---

### HU-50 — Pipeline de CI/CD completo
**Como** equipo **quiero** un pipeline que compile, pruebe, analice, construya la imagen y valide los manifiestos **para** bloquear cambios defectuosos antes de integrarlos.
- Prioridad: Alta · Estimación: 5

**Criterios de aceptación**
- El workflow ejecuta, en orden: restaurar/compilar, pruebas y cobertura, formato/análisis estático, construcción de imagen Docker, validación de manifiestos de Kubernetes, revisión de dependencias vulnerables.
- El pipeline bloquea la integración si cualquiera de estos pasos falla.

---

## Épica 12 — Documentación y prácticas XP

### HU-51 — Documentar historias, plan de liberación y bitácora XP
**Como** equipo **quiero** documentar las historias, el plan de iteraciones y los resultados de cada iteración **para** dejar evidencia verificable del proceso XP.
- Prioridad: Alta · Estimación: 3

**Criterios de aceptación**
- `/docs/historias-usuario.md` contiene todas las historias con prioridad, estimación y criterios de aceptación (este documento).
- `/docs/plan-xp.md` documenta el plan de liberación, las iteraciones (al menos tres/cuatro) y las reglas de trabajo XP.
- `/docs/bitacora-xp.md` registra, por iteración: resultados, velocidad observada vs. planificada, retroalimentación del cliente, evidencia de TDD y refactorizaciones, y la pequeña liberación entregada.

---

### HU-52 — Documentación de arquitectura y modelo de datos
**Como** equipo **quiero** documentar la arquitectura general y el modelo de datos con diagramas **para** facilitar el entendimiento del sistema.
- Prioridad: Media · Estimación: 3

**Criterios de aceptación**
- `/docs/arquitectura-general.md` y `/docs/modelo-datos.md` incluyen diagramas Mermaid (o imágenes en `/docs/assets`) que reflejan fielmente la implementación.

---

### HU-53 — Documentación por módulo, integración y API
**Como** equipo **quiero** un archivo Markdown por módulo, uno de integración y uno de API **para** que cualquier persona entienda propósito, dependencias y contratos.
- Prioridad: Media · Estimación: 3

**Criterios de aceptación**
- Existe un `.md` por módulo/servicio (propósito, responsabilidades, dependencias, entradas, salidas, reglas, errores, pruebas).
- `integracion-modulos.md` explica cómo cooperan los módulos y los flujos de extremo a extremo.
- `api.md` documenta endpoints, contratos, ejemplos y errores.
- `/docs/README.md` funciona como índice de navegación de toda la documentación.

---

### HU-54 — Declaración de uso responsable de IA
**Como** equipo **quiero** declarar el uso de herramientas de IA como asistencia **para** cumplir la política de uso responsable del curso.
- Prioridad: Media · Estimación: 1

**Criterios de aceptación**
- `/docs/uso-ia.md` indica herramienta utilizada, finalidad, módulos asistidos, ejemplos relevantes y validaciones realizadas por el equipo.
- No existen comentarios artificiales ni contenido ajeno a la funcionalidad insertado con el propósito de identificar la herramienta.

---

## Resumen de estimación por épica (referencia para Planning Game)

| Épica | Historias | Puntos ideales totales |
|---|---|---|
| 0. Fundación | HU-01 a HU-03 | 9 |
| 1. Dominio y modelo de datos | HU-04, HU-05 | 10 |
| 2. Proveedores | HU-06 a HU-10 | 13 |
| 3. Licitaciones | HU-11 a HU-17 | 27 |
| 4. Ofertas | HU-18 a HU-25 | 22 |
| 5. Niveles de aprobación | HU-26, HU-27 | 7 |
| 6. Tipo de cambio | HU-28, HU-29 | 6 |
| 7. UI/UX | HU-30 a HU-36 | 14 |
| 8. API REST | HU-37 a HU-39 | 13 |
| 9. Persistencia/auditoría | HU-40 a HU-43 | 11 |
| 10. TDD y pruebas | HU-44 a HU-47 | 24 |
| 11. Docker/K8s/CI | HU-48 a HU-50 | 18 |
| 12. Documentación | HU-51 a HU-54 | 10 |
| **Total** | **54 historias** | **184** |

Esta tabla es el insumo para el Planning Game: úsese junto con la velocidad observada en la primera iteración para ajustar el plan de liberación en `plan-xp.md`.
