using System.Reflection;
using Licitaciones.Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace Licitaciones.FunctionalTests;

public sealed class ApiContractTests
{
    private static readonly Type[] ResourceControllers =
    [
        typeof(ApiLicitacionesController),
        typeof(ApiProveedoresController),
        typeof(ApiOfertasController),
        typeof(ApiNivelesAprobacionController),
        typeof(ApiTiposCambioController)
    ];

    [Fact]
    public void TodosLosRecursos_UsanRutaVersionada()
    {
        foreach (var controller in ResourceControllers)
        {
            var route = controller.GetCustomAttribute<RouteAttribute>();
            Assert.NotNull(route);
            Assert.StartsWith("api/v1/", route.Template);
            Assert.NotNull(controller.GetCustomAttribute<ApiControllerAttribute>());
        }
    }

    [Fact]
    public void NingunEndpoint_ExponeEntidadesDeDominio()
    {
        var endpointTypes = ResourceControllers
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            .Where(method => method.DeclaringType != typeof(object))
            .SelectMany(method => method.GetParameters()
                .Select(parameter => parameter.ParameterType)
                .Append(method.ReturnType));

        Assert.DoesNotContain(
            endpointTypes,
            type => type.FullName?.StartsWith(
                "Licitaciones.Domain.Entities.",
                StringComparison.Ordinal) == true);
    }

    [Fact]
    public void Licitaciones_ExponeAccionesPublicarYCerrar()
    {
        var routes = typeof(ApiLicitacionesController)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .SelectMany(method => method.GetCustomAttributes<HttpPostAttribute>())
            .Select(attribute => attribute.Template)
            .ToList();

        Assert.Contains("{id:guid}/publicar", routes);
        Assert.Contains("{id:guid}/cerrar", routes);
    }

    [Fact]
    public void ControladoresApi_NoColisionanConNombresDeControladoresMvc()
    {
        var mvcControllerNames = new[]
        {
            typeof(Licitaciones.Web.Controllers.LicitacionesController).Name,
            typeof(Licitaciones.Web.Controllers.ProveedoresController).Name,
            typeof(Licitaciones.Web.Controllers.OfertasController).Name,
            typeof(Licitaciones.Web.Controllers.NivelesAprobacionController).Name,
            typeof(Licitaciones.Web.Controllers.TiposCambioController).Name
        };

        foreach (var apiController in ResourceControllers)
        {
            Assert.StartsWith("Api", apiController.Name);
            Assert.DoesNotContain(apiController.Name, mvcControllerNames);
        }
    }

    [Fact]
    public void OpenApi_ContieneTodosLosRecursosYAccionesMinimas()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(ApiLicitacionesController).Assembly);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();
            options.SwaggerDoc("v1", new() { Title = "Test API", Version = "v1" });
        });
        using var app = builder.Build();

        var document = app.Services
            .GetRequiredService<ISwaggerProvider>()
            .GetSwagger("v1");

        Assert.Contains("/api/v1/licitaciones", document.Paths.Keys);
        Assert.Contains("/api/v1/proveedores", document.Paths.Keys);
        Assert.Contains("/api/v1/ofertas", document.Paths.Keys);
        Assert.Contains("/api/v1/nivelesaprobacion", document.Paths.Keys);
        Assert.Contains("/api/v1/tiposcambio", document.Paths.Keys);
        Assert.Contains("/api/v1/licitaciones/{id}/publicar", document.Paths.Keys);
        Assert.Contains("/api/v1/licitaciones/{id}/cerrar", document.Paths.Keys);
    }
}
