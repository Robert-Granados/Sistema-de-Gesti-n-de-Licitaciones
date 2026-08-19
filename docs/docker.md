# Ejecución con Docker Compose

## Requisitos

- Docker Desktop o Docker Engine con Compose v2.
- Un archivo `.env` creado a partir de `.env.example`, con credenciales locales.

## Construcción y arranque

```powershell
Copy-Item .env.example .env
# El archivo .env contiene la configuración local.
docker compose up -d --build
docker compose ps
```

La aplicación queda disponible en `http://localhost:8080` por defecto. El
contenedor espera a que PostgreSQL esté saludable, aplica las migraciones
pendientes y solo entonces inicia el servidor HTTP.

La imagen utiliza el SDK de .NET 9 exclusivamente para compilar. El stage final
contiene el runtime ASP.NET 9 y ejecuta la aplicación como el usuario no
privilegiado `app` (`uid=1654`).

## Persistencia

PostgreSQL almacena sus datos en el volumen nombrado
`proyecto_xp_postgres-data`. Para reiniciar los contenedores conservando la
base de datos:

```powershell
docker compose down
docker compose up -d
```

Puede confirmar que el volumen continúa presente con:

```powershell
docker volume inspect proyecto_xp_postgres-data
```

La opción `-v` de `docker compose down` elimina el volumen nombrado y, por
tanto, no forma parte del procedimiento de reinicio con persistencia.

## Evidencia de la prueba HU-48

El 09/08/2026 se insertó un registro temporal identificado por
`48000000-0000-0000-0000-000000000001`, se ejecutó `docker compose down` sin
`-v`, se levantó nuevamente la solución y se recuperó el mismo identificador y
nombre desde PostgreSQL. El registro temporal se eliminó al terminar la prueba.
Ambos servicios quedaron saludables y `/health` respondió HTTP 200.
