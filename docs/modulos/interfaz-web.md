# Módulo de interfaz web

## Propósito y responsabilidades

Ofrece la experiencia MVC/Razor: landing page, navegación y CRUD de los cinco recursos con formularios, filtros, paginación, confirmaciones, notificaciones y tema claro/oscuro.

## Dependencias, entradas y salidas

- Entrada: solicitudes HTML, formularios y parámetros de consulta.
- Componentes: controllers, ViewModels, vistas Razor, `FechaFuturaAttribute`, CSS y JavaScript.
- Depende de handlers/services de Application e implementaciones registradas por Infrastructure.
- Salida: HTML, redirecciones y mensajes de validación/operación; también aloja los controladores API compartidos y Swagger.

## Reglas y errores

- Data Annotations y validación no intrusiva muestran errores junto al campo; el servidor repite las reglas críticas.
- Los tokens antifalsificación protegen POST MVC; las eliminaciones requieren confirmación visual.
- Los controladores conservan el modelo ante errores previsibles y muestran notificaciones sin revelar detalles técnicos.

## Pruebas

Las suites Browser cubren landing, navegación, CRUD, formularios, proveedores, licitaciones, ofertas, conversión y tema. Las pruebas funcionales cubren el middleware y los contratos HTTP alojados.
