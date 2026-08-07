using Licitaciones.Api;
using Licitaciones.Api.Controllers;
using Licitaciones.Infrastructure;
using Licitaciones.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(ApiLicitacionesController).Assembly);
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddLicitacionesApplication();
builder.Services.AddStandardApiValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.SwaggerDoc("v1", new()
    {
        Title = "Sistema de Licitaciones API",
        Version = "v1",
        Description = "API REST para licitaciones, proveedores, ofertas, niveles de aprobación y tipos de cambio."
    });
});

var app = builder.Build();

await using (var migrationScope = app.Services.CreateAsyncScope())
{
    var logger = migrationScope.ServiceProvider
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("DatabaseMigration");

    try
    {
        logger.LogInformation("Aplicando migraciones pendientes de la base de datos.");
        var dbContext = migrationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Migraciones de base de datos aplicadas correctamente.");
    }
    catch (Exception exception)
    {
        logger.LogCritical(exception, "No fue posible aplicar las migraciones de la base de datos.");
        throw;
    }
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema de Licitaciones API v1");
    options.RoutePrefix = "swagger";
});
app.UseHttpsRedirection();
app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

public partial class Program;
