using System.Text.Json;
using Licitaciones.Api;
using Licitaciones.Application.Licitaciones.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Licitaciones.FunctionalTests;

public sealed class ApiExceptionMiddlewareTests
{
    [Fact]
    public async Task ReglaDeNegocio_Retorna422ConProblemDetailsYCorrelacion()
    {
        var middleware = new ApiExceptionMiddleware(
            _ => throw new PresupuestoInsuficienteException(
                "El presupuesto no cubre las ofertas registradas."),
            NullLogger<ApiExceptionMiddleware>.Instance);
        var context = CrearContexto();
        context.Request.Headers[ApiExceptionMiddleware.CorrelationHeader] = "cliente-123";

        await middleware.InvokeAsync(context);

        var body = await LeerBodyAsync(context);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        Assert.Equal("cliente-123", context.Response.Headers[ApiExceptionMiddleware.CorrelationHeader]);
        Assert.Equal("business_rule_violation", body.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal("cliente-123", body.RootElement.GetProperty("correlationId").GetString());
        Assert.Equal(422, body.RootElement.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task ErrorDesconocido_Retorna500ControladoSinDetalleSensible()
    {
        const string sensitive =
            "SELECT password FROM users; ConnectionString=Host=internal; at C:\\secret\\file.cs:42";
        var middleware = new ApiExceptionMiddleware(
            _ => throw new Exception(sensitive),
            NullLogger<ApiExceptionMiddleware>.Instance);
        var context = CrearContexto();

        await middleware.InvokeAsync(context);

        var body = await LeerBodyAsync(context);
        var serialized = body.RootElement.GetRawText();
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("internal_error", body.RootElement.GetProperty("errorCode").GetString());
        Assert.False(string.IsNullOrWhiteSpace(
            body.RootElement.GetProperty("correlationId").GetString()));
        Assert.DoesNotContain("SELECT", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionString", serialized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", serialized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task DbUpdateConcurrencyException_Retorna409Controlado()
    {
        var middleware = new ApiExceptionMiddleware(
            _ => throw new DbUpdateConcurrencyException("Detalle técnico de EF Core."),
            NullLogger<ApiExceptionMiddleware>.Instance);
        var context = CrearContexto();

        await middleware.InvokeAsync(context);

        var body = await LeerBodyAsync(context);
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        Assert.Equal("concurrency_conflict", body.RootElement.GetProperty("errorCode").GetString());
        Assert.Equal(
            "El registro fue modificado por otro proceso. Actualice los datos e intente nuevamente.",
            body.RootElement.GetProperty("detail").GetString());
        Assert.DoesNotContain("EF Core", body.RootElement.GetRawText(), StringComparison.Ordinal);
    }

    private static DefaultHttpContext CrearContexto()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/v1/recurso";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<JsonDocument> LeerBodyAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(context.Response.Body);
    }
}
