using Licitaciones.Application.Proveedores.Editar;
using Licitaciones.Application.Proveedores.Exceptions;
using Licitaciones.Application.Proveedores.Ports;
using Licitaciones.Domain.Entities;

namespace Licitaciones.UnitTests.Application.Proveedores;

public sealed class EditarProveedorHandlerTests
{
    [Fact]
    public async Task Handle_ConNombreValido_ActualizaProveedorYUsaRowVersion()
    {
        var proveedor = new Proveedor("Nombre anterior", "NOMBRE ANTERIOR");
        var repository = new FakeProveedorEditRepository(proveedor, rowVersion: 4);
        var handler = new EditarProveedorHandler(repository);

        await handler.HandleAsync(new EditarProveedorCommand(
            proveedor.Id,
            "  Compañía   Actualizada  ",
            RowVersion: 4));

        Assert.Equal("Compañía Actualizada", proveedor.Nombre);
        Assert.Equal("COMPANIA ACTUALIZADA", proveedor.NombreNormalizado);
        Assert.Equal(4, repository.RowVersionGuardado);
    }

    [Fact]
    public async Task Handle_ConElMismoNombre_NoSeConsideraDuplicado()
    {
        var proveedor = new Proveedor("Proveedor Uno", "PROVEEDOR UNO");
        var repository = new FakeProveedorEditRepository(proveedor, rowVersion: 2);
        var handler = new EditarProveedorHandler(repository);

        await handler.HandleAsync(new EditarProveedorCommand(
            proveedor.Id,
            " proveedor   uno ",
            RowVersion: 2));

        Assert.Equal(proveedor.Id, repository.IdExcluidoEnValidacion);
        Assert.True(repository.GuardarInvocado);
    }

    [Fact]
    public async Task Handle_ConNombreDeOtroProveedor_RechazaDuplicado()
    {
        var proveedor = new Proveedor("Proveedor Uno", "PROVEEDOR UNO");
        var repository = new FakeProveedorEditRepository(proveedor, rowVersion: 2)
        {
            ExisteDuplicado = true
        };
        var handler = new EditarProveedorHandler(repository);

        await Assert.ThrowsAsync<ProveedorDuplicadoException>(() =>
            handler.HandleAsync(new EditarProveedorCommand(
                proveedor.Id,
                "Proveedor Dos",
                RowVersion: 2)));

        Assert.False(repository.GuardarInvocado);
    }

    private sealed class FakeProveedorEditRepository(
        Proveedor proveedor,
        int rowVersion) : IProveedorEditRepository
    {
        public bool ExisteDuplicado { get; init; }

        public Guid? IdExcluidoEnValidacion { get; private set; }

        public int? RowVersionGuardado { get; private set; }

        public bool GuardarInvocado { get; private set; }

        public Task<ProveedorEdicion?> ObtenerPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                id == proveedor.Id
                    ? new ProveedorEdicion(proveedor, rowVersion)
                    : null);

        public Task<bool> ExisteNombreNormalizadoAsync(
            string nombreNormalizado,
            Guid excluirProveedorId,
            CancellationToken cancellationToken = default)
        {
            IdExcluidoEnValidacion = excluirProveedorId;
            return Task.FromResult(ExisteDuplicado);
        }

        public Task GuardarAsync(
            Proveedor proveedorActualizado,
            int expectedRowVersion,
            CancellationToken cancellationToken = default)
        {
            GuardarInvocado = true;
            RowVersionGuardado = expectedRowVersion;
            return Task.CompletedTask;
        }
    }
}
