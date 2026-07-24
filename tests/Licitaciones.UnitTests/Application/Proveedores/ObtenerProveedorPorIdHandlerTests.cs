using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Proveedores.Detalle;
using Licitaciones.Application.Proveedores.Ports;
using Licitaciones.Domain.Enums;

namespace Licitaciones.UnitTests.Application.Proveedores;

public sealed class ObtenerProveedorPorIdHandlerTests
{
    [Fact]
    public async Task Handle_ConParametrosValidos_RetornaDetalle()
    {
        var proveedorId = Guid.NewGuid();
        var repository = new FakeProveedorDetalleRepository(
            new ProveedorDetalleDto(
                proveedorId,
                "Proveedor Uno",
                new PaginaResultado<OfertaProveedorDto>(
                    [
                        new(
                            Guid.NewGuid(),
                            Guid.NewGuid(),
                            "LIC-001",
                            "Compra de equipo",
                            500m,
                            new DateTimeOffset(2026, 7, 23, 12, 0, 0, TimeSpan.Zero),
                            EstadoLicitacion.Publicada)
                    ],
                    1,
                    1,
                    10)));
        var handler = new ObtenerProveedorPorIdHandler(repository);

        var resultado = await handler.HandleAsync(
            new ObtenerProveedorPorIdQuery(proveedorId, 1, 10));

        Assert.NotNull(resultado);
        Assert.Equal(proveedorId, resultado.Id);
        Assert.Single(resultado.Ofertas.Elementos);
        Assert.Equal(EstadoLicitacion.Publicada, resultado.Ofertas.Elementos[0].Estado);
    }

    [Fact]
    public async Task Handle_ConPaginacionFueraDeRango_NormalizaParametros()
    {
        var repository = new FakeProveedorDetalleRepository(null);
        var handler = new ObtenerProveedorPorIdHandler(repository);

        await handler.HandleAsync(
            new ObtenerProveedorPorIdQuery(Guid.NewGuid(), 0, 500));

        Assert.NotNull(repository.UltimaConsulta);
        Assert.Equal(1, repository.UltimaConsulta.Page);
        Assert.Equal(100, repository.UltimaConsulta.PageSize);
    }

    [Fact]
    public async Task Handle_ConIdVacio_NoConsultaRepositorio()
    {
        var repository = new FakeProveedorDetalleRepository(null);
        var handler = new ObtenerProveedorPorIdHandler(repository);

        var resultado = await handler.HandleAsync(
            new ObtenerProveedorPorIdQuery(Guid.Empty));

        Assert.Null(resultado);
        Assert.Null(repository.UltimaConsulta);
    }

    private sealed class FakeProveedorDetalleRepository(
        ProveedorDetalleDto? resultado) : IProveedorDetalleRepository
    {
        public ProveedorDetalleConsulta? UltimaConsulta { get; private set; }

        public Task<ProveedorDetalleDto?> ObtenerPorIdAsync(
            ProveedorDetalleConsulta consulta,
            CancellationToken cancellationToken = default)
        {
            UltimaConsulta = consulta;
            return Task.FromResult(resultado);
        }
    }
}
