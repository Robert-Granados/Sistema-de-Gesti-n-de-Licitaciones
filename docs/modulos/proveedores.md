# Módulo de proveedores

## Propósito y responsabilidades

Administra las organizaciones que pueden ofertar. Permite crear, listar, filtrar, consultar detalle con ofertas, editar y eliminar lógicamente.

## Dependencias, entradas y salidas

- Entrada: controladores MVC/API, `Crear/Editar/EliminarProveedorCommand` y queries de listado/detalle.
- Aplicación: handlers bajo `Application/Proveedores` y normalizador de nombres.
- Salida: DTO de listado/detalle/edición y resultados de escritura.
- Persistencia: puertos `IProveedor*Repository`, implementados por repositorios EF Core sobre `proveedores` y consultas relacionadas con `ofertas`.

## Reglas y errores

- El nombre es obligatorio, máximo 200 caracteres y sólo admite los caracteres definidos por el dominio/SQL.
- Se normalizan espacios, acentos y mayúsculas; no puede repetirse entre proveedores activos.
- La eliminación es lógica y no borra ofertas históricas. Un proveedor eliminado no puede recibir ofertas nuevas.
- Errores: nombre inválido (422), duplicado (409), inexistente (404) y edición concurrente (409).

## Pruebas

`Application/Proveedores/*Tests`, `ProveedorTests`, pruebas de integración de eliminación y `ProveedorFlowTests` cubren normalización, CRUD, detalle, borrado lógico y navegación.
