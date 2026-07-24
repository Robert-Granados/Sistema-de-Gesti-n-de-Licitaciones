using Licitaciones.Infrastructure;
using Licitaciones.Application.Proveedores.Crear;
using Licitaciones.Application.Proveedores.Listar;
using Licitaciones.Application.Proveedores.Detalle;
using Licitaciones.Application.Proveedores.Editar;
using Licitaciones.Application.Proveedores.Eliminar;
using Licitaciones.Application.Licitaciones.Crear;
using Licitaciones.Application.Licitaciones.Listar;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<CrearProveedorHandler>();
builder.Services.AddScoped<ListarProveedoresHandler>();
builder.Services.AddScoped<ObtenerProveedorPorIdHandler>();
builder.Services.AddScoped<EditarProveedorHandler>();
builder.Services.AddScoped<EliminarProveedorHandler>();
builder.Services.AddScoped<CrearLicitacionHandler>();
builder.Services.AddScoped<ListarLicitacionesHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
