using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Licitaciones.Detalle;
using Licitaciones.Application.Ofertas.Editar;
using Licitaciones.Application.Ofertas.Listar;
using Licitaciones.Application.Ofertas.OpcionesFiltro;
using Licitaciones.Application.Ofertas.Ports;

namespace Licitaciones.UnitTests.Application.Ofertas;

public sealed class OpcionesFiltroOfertasHandlerTests
{
    [Fact]
    public async Task Handle_ObtieneOpcionesDelRepositorio()
    {
        var opcionLicitacion = new OpcionLicitacionDto(Guid.NewGuid(), "LIC-001");
        var opcionProveedor = new ProveedorBasicoDto(Guid.NewGuid(), "Proveedor A");
        var repository = new FakeOfertaReadRepository(
            new OpcionesFiltroOfertasDto([opcionLicitacion], [opcionProveedor]));
        var handler = new OpcionesFiltroOfertasHandler(repository);

        var opciones = await handler.HandleAsync(new OpcionesFiltroOfertasQuery());

        Assert.True(repository.FueConsultado);
        var licitacion = Assert.Single(opciones.Licitaciones);
        var proveedor = Assert.Single(opciones.Proveedores);
        Assert.Equal("LIC-001", licitacion.Codigo);
        Assert.Equal("Proveedor A", proveedor.Nombre);
    }

    private sealed class FakeOfertaReadRepository(
        OpcionesFiltroOfertasDto opciones) : IOfertaReadRepository
    {
        public bool FueConsultado { get; private set; }

        public Task<PaginaResultado<OfertaListadoDto>> ListarAsync(
            OfertasConsulta consulta,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new PaginaResultado<OfertaListadoDto>(
                [],
                totalRegistros: 0,
                paginaActual: consulta.Page,
                tamanoPagina: consulta.PageSize));

        public Task<OpcionesFiltroOfertasDto> ObtenerOpcionesFiltroAsync(
            CancellationToken cancellationToken = default)
        {
            FueConsultado = true;
            return Task.FromResult(opciones);
        }

        public Task<EditarOfertaDto?> ObtenerParaEdicionAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<EditarOfertaDto?>(null);
    }
}
