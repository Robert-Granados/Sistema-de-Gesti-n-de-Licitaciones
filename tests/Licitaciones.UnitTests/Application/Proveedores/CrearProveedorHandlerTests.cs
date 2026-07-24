using Licitaciones.Application.Proveedores.Crear;
using Licitaciones.Application.Proveedores.Exceptions;
using Licitaciones.Application.Proveedores.Ports;
using Licitaciones.Domain.Entities;

namespace Licitaciones.UnitTests.Application.Proveedores;

public sealed class CrearProveedorHandlerTests
{
    [Fact]
    public async Task Handle_ConNombreValido_RegistraProveedor()
    {
        var repository = new FakeProveedorRepository();
        var handler = new CrearProveedorHandler(repository);

        var result = await handler.HandleAsync(
            new CrearProveedorCommand(" Empresa 123, S.A. (CR) "));

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Empresa 123, S.A. (CR)", result.Nombre);
        var proveedor = Assert.Single(repository.Agregados);
        Assert.Equal("EMPRESA 123, S.A. (CR)", proveedor.NombreNormalizado);
    }

    [Fact]
    public async Task Handle_ConSimboloNoPermitido_RechazaNombre()
    {
        var repository = new FakeProveedorRepository();
        var handler = new CrearProveedorHandler(repository);

        var exception = await Assert.ThrowsAsync<NombreProveedorInvalidoException>(() =>
            handler.HandleAsync(new CrearProveedorCommand("Empresa @gil")));

        Assert.Contains(
            "solo puede contener",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
        Assert.Empty(repository.Agregados);
    }

    [Fact]
    public async Task Handle_ConVariacionDeNombreExistente_RechazaDuplicado()
    {
        var repository = new FakeProveedorRepository(["COMPANIA AGIL"]);
        var handler = new CrearProveedorHandler(repository);

        var exception = await Assert.ThrowsAsync<ProveedorDuplicadoException>(() =>
            handler.HandleAsync(new CrearProveedorCommand("  compañía   ágil  ")));

        Assert.Contains("registrado", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(repository.Agregados);
    }

    private sealed class FakeProveedorRepository(
        IEnumerable<string>? nombresNormalizados = null) : IProveedorRepository
    {
        private readonly HashSet<string> _nombresNormalizados =
            new(nombresNormalizados ?? [], StringComparer.Ordinal);

        public List<Proveedor> Agregados { get; } = [];

        public Task<bool> ExisteNombreNormalizadoAsync(
            string nombreNormalizado,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_nombresNormalizados.Contains(nombreNormalizado));

        public Task AgregarAsync(
            Proveedor proveedor,
            CancellationToken cancellationToken = default)
        {
            Agregados.Add(proveedor);
            _nombresNormalizados.Add(proveedor.NombreNormalizado);
            return Task.CompletedTask;
        }
    }
}
