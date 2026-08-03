using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Domain.Entities;

namespace Licitaciones.UnitTests.Application.NivelesAprobacion;

public sealed class NivelAprobacionServiceTests
{
    [Fact]
    public async Task Crear_RangoTraslapado_EsRechazadoAntesDePersistir()
    {
        var repository = new RepositoryFake(
            new NivelAprobacion(0.01m, 999_999.99m, "Encargado de área"));
        var service = new NivelAprobacionService(repository);

        var exception = await Assert.ThrowsAsync<NivelAprobacionException>(() =>
            service.CrearAsync(500_000m, 1_500_000m, "Gerencia"));

        Assert.Contains("traslapa", exception.Message);
        Assert.False(repository.AgregarFueInvocado);
    }

    [Fact]
    public async Task Crear_SegundoRangoAbierto_EsRechazadoAntesDePersistir()
    {
        var repository = new RepositoryFake(
            new NivelAprobacion(10_000_000m, null, "Junta Directiva"));
        var service = new NivelAprobacionService(repository);

        var exception = await Assert.ThrowsAsync<NivelAprobacionException>(() =>
            service.CrearAsync(20_000_000m, null, "Asamblea"));

        Assert.Contains("Solo puede existir un rango abierto", exception.Message);
        Assert.False(repository.AgregarFueInvocado);
    }

    private sealed class RepositoryFake(params NivelAprobacion[] niveles)
        : INivelAprobacionRepository
    {
        private readonly List<NivelAprobacion> _niveles = [.. niveles];

        public bool AgregarFueInvocado { get; private set; }

        public Task<IReadOnlyList<NivelAprobacion>> ListarOrdenadosAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<NivelAprobacion>>(
                _niveles.OrderBy(n => n.MontoMinimoCrc).ToList());

        public Task<NivelAprobacion?> ObtenerAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_niveles.FirstOrDefault(n => n.Id == id));

        public Task AgregarAsync(
            NivelAprobacion nivel,
            CancellationToken cancellationToken = default)
        {
            AgregarFueInvocado = true;
            _niveles.Add(nivel);
            return Task.CompletedTask;
        }

        public Task GuardarAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task EliminarAsync(
            NivelAprobacion nivel,
            CancellationToken cancellationToken = default)
        {
            _niveles.Remove(nivel);
            return Task.CompletedTask;
        }
    }
}
