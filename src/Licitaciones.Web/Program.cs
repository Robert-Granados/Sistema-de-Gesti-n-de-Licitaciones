using Licitaciones.Infrastructure;
using Licitaciones.Application.Proveedores.Crear;
using Licitaciones.Application.Proveedores.Listar;
using Licitaciones.Application.Proveedores.Detalle;
using Licitaciones.Application.Proveedores.Editar;
using Licitaciones.Application.Proveedores.Eliminar;
using Licitaciones.Application.Licitaciones.Cerrar;
using Licitaciones.Application.Licitaciones.Crear;
using Licitaciones.Application.Licitaciones.Detalle;
using Licitaciones.Application.Licitaciones.Editar;
using Licitaciones.Application.Licitaciones.Eliminar;
using Licitaciones.Application.Licitaciones.Listar;
using Licitaciones.Application.Licitaciones.Publicar;
using Licitaciones.Application.Ofertas.Common;
using Licitaciones.Application.Ofertas.Editar;
using Licitaciones.Application.Ofertas.Eliminar;
using Licitaciones.Application.Ofertas.Listar;
using Licitaciones.Application.Ofertas.OpcionesFiltro;
using Licitaciones.Application.Ofertas.Registrar;
using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Application.TiposCambio;
using Licitaciones.Api;
using Licitaciones.Api.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllersWithViews()
    .AddApplicationPart(typeof(ApiLicitacionesController).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddStandardApiValidation();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.SwaggerDoc("v1", new()
    {
        Title = "Sistema de Licitaciones API",
        Version = "v1",
        Description = "API REST versionada para la integración con el sistema."
    });
});
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<CrearProveedorHandler>();
builder.Services.AddScoped<ListarProveedoresHandler>();
builder.Services.AddScoped<ObtenerProveedorPorIdHandler>();
builder.Services.AddScoped<EditarProveedorHandler>();
builder.Services.AddScoped<EliminarProveedorHandler>();
builder.Services.AddScoped<CrearLicitacionHandler>();
builder.Services.AddScoped<ListarLicitacionesHandler>();
builder.Services.AddScoped<ObtenerLicitacionPorIdHandler>();
builder.Services.AddScoped<EditarLicitacionHandler>();
builder.Services.AddScoped<PublicarLicitacionHandler>();
builder.Services.AddScoped<CerrarLicitacionHandler>();
builder.Services.AddScoped<EliminarLicitacionHandler>();
builder.Services.AddScoped<RegistrarOfertaHandler>();
builder.Services.AddScoped<ListarOfertasHandler>();
builder.Services.AddScoped<OpcionesFiltroOfertasHandler>();
builder.Services.AddScoped<EditarOfertaHandler>();
builder.Services.AddScoped<EliminarOfertaHandler>();
builder.Services.AddScoped<OfertaValidador>();
builder.Services.AddScoped<NivelAprobacionService>();
builder.Services.AddScoped<ResolverAprobadorService>();
builder.Services.AddScoped<TipoCambioService>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema de Licitaciones API v1");
    options.RoutePrefix = "swagger";
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapHealthChecks("/health");
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
