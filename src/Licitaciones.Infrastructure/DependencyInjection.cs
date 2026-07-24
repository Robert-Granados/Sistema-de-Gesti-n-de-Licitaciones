using Licitaciones.Domain.Enums;
using Licitaciones.Application.Proveedores.Ports;
using Licitaciones.Infrastructure.Persistence;
using Licitaciones.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Licitaciones.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No se configuró la cadena de conexión 'DefaultConnection'.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MapEnum<EstadoLicitacion>(
                    "estado_licitacion",
                    nameTranslator: PreserveCaseNameTranslator.Instance)));
        services.AddScoped<IProveedorRepository, ProveedorRepository>();
        services.AddScoped<IProveedorReadRepository, ProveedorReadRepository>();
        services.AddScoped<IProveedorDetalleRepository, ProveedorDetalleRepository>();
        services.AddScoped<IProveedorEditRepository, ProveedorEditRepository>();
        services.AddScoped<IProveedorDeleteRepository, ProveedorDeleteRepository>();

        return services;
    }
}
