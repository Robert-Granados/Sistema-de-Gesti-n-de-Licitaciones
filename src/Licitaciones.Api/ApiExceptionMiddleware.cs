using System.Text.RegularExpressions;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Licitaciones.Api;

public sealed partial class ApiExceptionMiddleware(
    RequestDelegate next,
    ILogger<ApiExceptionMiddleware> logger)
{
    public const string CorrelationHeader = "X-Correlation-ID";
    public const string CorrelationItem = "CorrelationId";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ObtenerCorrelationId(context);
        context.TraceIdentifier = correlationId;
        context.Items[CorrelationItem] = correlationId;
        context.Response.Headers[CorrelationHeader] = correlationId;

        using var scope = logger.BeginScope(
            new Dictionary<string, object> { ["CorrelationId"] = correlationId });

        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await EscribirErrorAsync(context, exception, correlationId);
        }
    }

    private async Task EscribirErrorAsync(
        HttpContext context,
        Exception exception,
        string correlationId)
    {
        var error = Clasificar(exception);

        if (error.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Error no controlado al procesar {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            logger.LogWarning(
                "Solicitud rechazada. ErrorCode={ErrorCode}, Status={Status}, ExceptionType={ExceptionType}",
                error.Code,
                error.Status,
                exception.GetType().Name);
        }

        if (context.Response.HasStarted)
        {
            logger.LogError("No fue posible escribir ProblemDetails porque la respuesta ya había iniciado.");
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = error.Status;
        context.Response.ContentType = "application/problem+json";
        context.Response.Headers[CorrelationHeader] = correlationId;

        var problem = new ProblemDetails
        {
            Status = error.Status,
            Title = error.Title,
            Detail = error.Detail
        };
        problem.Extensions["errorCode"] = error.Code;
        problem.Extensions["correlationId"] = correlationId;

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            problem,
            cancellationToken: context.RequestAborted);
    }

    internal static ApiError Clasificar(Exception exception)
    {
        var name = exception.GetType().Name;

        if (exception is KeyNotFoundException
            || name.Contains("NoEncontrad", StringComparison.Ordinal))
        {
            return new(
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                "resource_not_found",
                MensajeSeguro(exception, "El recurso solicitado no existe."));
        }

        if (exception is DbUpdateConcurrencyException
            || name.Contains("Concurrencia", StringComparison.Ordinal))
        {
            return new(
                StatusCodes.Status409Conflict,
                "Conflicto de concurrencia",
                "concurrency_conflict",
                "El registro fue modificado por otro proceso. Actualice los datos e intente nuevamente.");
        }

        if (name.Contains("Duplicad", StringComparison.Ordinal))
        {
            return new(
                StatusCodes.Status409Conflict,
                "Recurso duplicado",
                "duplicate_resource",
                MensajeSeguro(exception, "Ya existe un registro con los mismos datos únicos."));
        }

        if (name.Contains("Cerrada", StringComparison.Ordinal)
            || name.Contains("NoDisponible", StringComparison.Ordinal))
        {
            return new(
                StatusCodes.Status409Conflict,
                "Operación incompatible con el estado actual",
                "invalid_resource_state",
                MensajeSeguro(exception, "El estado actual del recurso no permite la operación."));
        }

        if (name.Contains("PresupuestoInsuficiente", StringComparison.Ordinal)
            || name.Contains("NivelAprobacion", StringComparison.Ordinal)
            || name.Contains("Invalido", StringComparison.Ordinal))
        {
            return new(
                StatusCodes.Status422UnprocessableEntity,
                "Regla de negocio no satisfecha",
                "business_rule_violation",
                MensajeSeguro(exception, "Los datos no satisfacen una regla de negocio."));
        }

        if (exception is ArgumentException)
        {
            return new(
                StatusCodes.Status400BadRequest,
                "Solicitud inválida",
                "invalid_request",
                MensajeSeguro(exception, "Uno o más parámetros son inválidos."));
        }

        if (exception is InvalidOperationException)
        {
            return new(
                StatusCodes.Status409Conflict,
                "Conflicto con el estado actual",
                "operation_conflict",
                "La operación no puede completarse debido al estado actual del recurso.");
        }

        return new(
            StatusCodes.Status500InternalServerError,
            "Error interno",
            "internal_error",
            "Ocurrió un error interno al procesar la solicitud.");
    }

    private static string ObtenerCorrelationId(HttpContext context)
    {
        var supplied = context.Request.Headers[CorrelationHeader].FirstOrDefault();
        return supplied is not null
            && supplied.Length <= 128
            && CorrelationIdRegex().IsMatch(supplied)
                ? supplied
                : Guid.NewGuid().ToString("N");
    }

    private static string MensajeSeguro(Exception exception, string fallback) =>
        exception.Message.Length is > 0 and <= 500
        && !exception.Message.Contains(" at ", StringComparison.OrdinalIgnoreCase)
        && !exception.Message.Contains("SELECT ", StringComparison.OrdinalIgnoreCase)
        && !exception.Message.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase)
            ? exception.Message
            : fallback;

    [GeneratedRegex("^[A-Za-z0-9._-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex CorrelationIdRegex();
}

public sealed record ApiError(
    int Status,
    string Title,
    string Code,
    string Detail);
