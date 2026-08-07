using Licitaciones.Application.Proveedores.Eliminar;
using Licitaciones.Application.Proveedores.Exceptions;
using Licitaciones.Application.Proveedores.Ports;
using Licitaciones.Domain.Entities;
using Licitaciones.UnitTests.Common;

namespace Licitaciones.UnitTests.Application.Proveedores;

public sealed class EliminarProveedorHandlerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task HandleAsync_ProveedorConOfertas_AplicaBorradoLogico()
    {
        var proveedor = new Proveedor("Proveedor Histórico", "PROVEEDOR HISTORICO");
        var repository = new FakeProveedorDeleteRepository(proveedor, tieneOfertas: true);
        var handler = new EliminarProveedorHandler(repository, new FakeClock(Now));

        var resultado = await handler.HandleAsync(
            new EliminarProveedorCommand(proveedor.Id));

        Assert.True(resultado.TeniaOfertas);
        Assert.True(proveedor.EstaEliminado);
        Assert.Equal(Now, proveedor.EliminadoEn);
        Assert.True(repository.Guardado);
        Assert.False(repository.EliminacionFisicaIntentada);
    }

    [Fact]
    public async Task HandleAsync_ProveedorSinOfertas_AplicaMismaPoliticaDeBorradoLogico()
    {
        var proveedor = new Proveedor("Proveedor Nuevo", "PROVEEDOR NUEVO");
        var repository = new FakeProveedorDeleteRepository(proveedor, tieneOfertas: false);
        var handler = new EliminarProveedorHandler(repository, new FakeClock(Now));

        var resultado = await handler.HandleAsync(
            new EliminarProveedorCommand(proveedor.Id));

        Assert.False(resultado.TeniaOfertas);
        Assert.True(proveedor.EstaEliminado);
        Assert.True(repository.Guardado);
    }

    [Fact]
    public async Task HandleAsync_ProveedorInexistente_LanzaExcepcionControlada()
    {
        var handler = new EliminarProveedorHandler(
            new FakeProveedorDeleteRepository(null, tieneOfertas: false),
            new FakeClock(Now));

        await Assert.ThrowsAsync<ProveedorNoEncontradoException>(
            () => handler.HandleAsync(
                new EliminarProveedorCommand(Guid.NewGuid())));
    }

    private sealed class FakeProveedorDeleteRepository(
        Proveedor? proveedor,
        bool tieneOfertas) : IProveedorDeleteRepository
    {
        public bool Guardado { get; private set; }

        public bool EliminacionFisicaIntentada { get; private set; }

        public Task<Proveedor?> ObtenerActivoPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(proveedor?.Id == id ? proveedor : null);

        public Task<bool> TieneOfertasAsync(
            Guid proveedorId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(tieneOfertas);

        public Task GuardarBorradoLogicoAsync(
            Proveedor proveedorAEliminar,
            CancellationToken cancellationToken = default)
        {
            Guardado = true;
            return Task.CompletedTask;
        }
    }
}
