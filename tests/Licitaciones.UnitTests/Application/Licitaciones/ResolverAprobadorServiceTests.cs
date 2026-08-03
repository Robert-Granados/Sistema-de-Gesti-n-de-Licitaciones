using Licitaciones.Application.Licitaciones.Detalle;
using Licitaciones.Application.NivelesAprobacion;
using Licitaciones.Domain.Entities;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class ResolverAprobadorServiceTests
{
    [Fact]
    public async Task Resolver_ConsultaRangosOrdenados_YRetornaNivelCorrespondiente()
    {
        var repository = new RepositoryFake(
            new NivelAprobacion(10_000_000m, null, "Junta Directiva"),
            new NivelAprobacion(0.01m, 999_999.99m, "Encargado de área"),
            new NivelAprobacion(1_000_000m, 9_999_999.99m, "Gerencia"));
        var service = new ResolverAprobadorService(repository);

        var resultado = await service.Resolver(1_000_000m);

        Assert.True(resultado.Configurado);
        Assert.Equal("Gerencia", resultado.Aprobador);
        Assert.NotNull(resultado.NivelAprobacionId);
    }

    [Fact]
    public async Task Resolver_SinRango_RetornaResultadoExplicito()
    {
        var service = new ResolverAprobadorService(new RepositoryFake());

        var resultado = await service.Resolver(100m);

        Assert.False(resultado.Configurado);
        Assert.Equal(ResolverAprobadorService.SinAprobadorConfigurado, resultado.Aprobador);
        Assert.Null(resultado.NivelAprobacionId);
    }

    private sealed class RepositoryFake(params NivelAprobacion[] niveles)
        : INivelAprobacionRepository
    {
        public Task<IReadOnlyList<NivelAprobacion>> ListarOrdenadosAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<NivelAprobacion>>(
                niveles.OrderBy(n => n.MontoMinimoCrc).ToList());

        public Task<NivelAprobacion?> ObtenerAsync(Guid id, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task AgregarAsync(NivelAprobacion nivel, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task GuardarAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task EliminarAsync(NivelAprobacion nivel, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
