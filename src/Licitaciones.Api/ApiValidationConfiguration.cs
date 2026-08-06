using Microsoft.AspNetCore.Mvc;

namespace Licitaciones.Api;

public static class ApiValidationConfiguration
{
    public static IServiceCollection AddStandardApiValidation(
        this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var correlationId =
                    context.HttpContext.Items[ApiExceptionMiddleware.CorrelationItem]?.ToString()
                    ?? context.HttpContext.TraceIdentifier;
                var problem = new ValidationProblemDetails(context.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Solicitud inválida",
                    Detail = "Uno o más campos no superaron la validación."
                };
                problem.Extensions["errorCode"] = "validation_failed";
                problem.Extensions["correlationId"] = correlationId;

                return new BadRequestObjectResult(problem)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });
        return services;
    }
}
