using Licitaciones.Application.Licitaciones.Cerrar;
using Licitaciones.Application.Licitaciones.Crear;
using Licitaciones.Application.Licitaciones.Detalle;
using Licitaciones.Application.Licitaciones.Editar;
using Licitaciones.Application.Licitaciones.Eliminar;
using Licitaciones.Application.Licitaciones.Listar;
using Licitaciones.Application.Licitaciones.Publicar;
using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Application.Ofertas.Common;
using Licitaciones.Application.Ofertas.Editar;
using Licitaciones.Application.Ofertas.Eliminar;
using Licitaciones.Application.Ofertas.Listar;
using Licitaciones.Application.Ofertas.Registrar;
using Licitaciones.Application.Proveedores.Crear;
using Licitaciones.Application.Proveedores.Detalle;
using Licitaciones.Application.Proveedores.Editar;
using Licitaciones.Application.Proveedores.Eliminar;
using Licitaciones.Application.Proveedores.Listar;
using Licitaciones.Application.TiposCambio;

namespace Licitaciones.Api;

public static class ApiServiceRegistration
{
    public static IServiceCollection AddLicitacionesApplication(this IServiceCollection services)
    {
        services.AddScoped<CrearProveedorHandler>();
        services.AddScoped<ListarProveedoresHandler>();
        services.AddScoped<ObtenerProveedorPorIdHandler>();
        services.AddScoped<EditarProveedorHandler>();
        services.AddScoped<EliminarProveedorHandler>();
        services.AddScoped<CrearLicitacionHandler>();
        services.AddScoped<ListarLicitacionesHandler>();
        services.AddScoped<ObtenerLicitacionPorIdHandler>();
        services.AddScoped<EditarLicitacionHandler>();
        services.AddScoped<PublicarLicitacionHandler>();
        services.AddScoped<CerrarLicitacionHandler>();
        services.AddScoped<EliminarLicitacionHandler>();
        services.AddScoped<RegistrarOfertaHandler>();
        services.AddScoped<ListarOfertasHandler>();
        services.AddScoped<EditarOfertaHandler>();
        services.AddScoped<EliminarOfertaHandler>();
        services.AddScoped<OfertaValidador>();
        services.AddScoped<NivelAprobacionService>();
        services.AddScoped<ResolverAprobadorService>();
        services.AddScoped<TipoCambioService>();
        return services;
    }
}
