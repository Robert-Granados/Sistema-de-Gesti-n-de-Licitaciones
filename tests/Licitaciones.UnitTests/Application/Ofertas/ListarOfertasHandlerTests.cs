using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Ofertas.Listar;
using Licitaciones.Application.Ofertas.OpcionesFiltro;
using Licitaciones.Application.Ofertas.Ports;

namespace Licitaciones.UnitTests.Application.Ofertas;

public sealed class ListarOfertasHandlerTests
{
    [Fact]
    public async Task Handle_ConParametrosInvalidos_UsaValoresSeguros()
    {
        var repository = new FakeOfertaReadRepository();
        var handler = new ListarOfertasHandler(repository);

        await handler.HandleAsync(new ListarOfertasQuery(
            Page: 0,
            PageSize: 500,
            LicitacionId: Guid.Empty,
            ProveedorId: Guid.Empty,
            SortBy: "desconocido"));

        Assert.NotNull(repository.UltimaConsulta);
        Assert.Equal(1, repository.UltimaConsulta.Page);
        Assert.Equal(100, repository.UltimaConsulta.PageSize);
        Assert.Null(repository.UltimaConsulta.LicitacionId);
        Assert.Null(repository.UltimaConsulta.ProveedorId);
        Assert.Equal(OrdenOferta.MontoAscendente, repository.UltimaConsulta.SortBy);
    }

    [Fact]
    public async Task Handle_ConFiltros_PasaAlRepository()
    {
        var repository = new FakeOfertaReadRepository();
        var handler = new ListarOfertasHandler(repository);
        var licitacionId = Guid.NewGuid();
        var proveedorId = Guid.NewGuid();

        await handler.HandleAsync(new ListarOfertasQuery(
            LicitacionId: licitacionId,
            ProveedorId: proveedorId));

        Assert.NotNull(repository.UltimaConsulta);
        Assert.Equal(licitacionId, repository.UltimaConsulta.LicitacionId);
        Assert.Equal(proveedorId, repository.UltimaConsulta.ProveedorId);
    }

    [Theory]
    [InlineData("monto_desc", OrdenOferta.MontoDescendente)]
    [InlineData("fecha", OrdenOferta.FechaAscendente)]
    [InlineData("fecha_desc", OrdenOferta.FechaDescendente)]
    public async Task Handle_ConOrden_ConservaOrdenSolicitado(
        string sortBy,
        OrdenOferta esperado)
    {
        var repository = new FakeOfertaReadRepository();
        var handler = new ListarOfertasHandler(repository);

        await handler.HandleAsync(new ListarOfertasQuery(SortBy: sortBy));

        Assert.NotNull(repository.UltimaConsulta);
        Assert.Equal(esperado, repository.UltimaConsulta.SortBy);
    }

    [Fact]
    public void PaginaResultado_CalculaTotalDePaginas()
    {
        var pagina = new PaginaResultado<OfertaListadoDto>(
            [],
            totalRegistros: 21,
            paginaActual: 2,
            tamanoPagina: 10);

        Assert.Equal(3, pagina.TotalPaginas);
        Assert.True(pagina.TienePaginaAnterior);
        Assert.True(pagina.TienePaginaSiguiente);
    }

    private sealed class FakeOfertaReadRepository : IOfertaReadRepository
    {
        public OfertasConsulta? UltimaConsulta { get; private set; }

        public Task<PaginaResultado<OfertaListadoDto>> ListarAsync(
            OfertasConsulta consulta,
            CancellationToken cancellationToken = default)
        {
            UltimaConsulta = consulta;
            return Task.FromResult(new PaginaResultado<OfertaListadoDto>(
                [],
                totalRegistros: 0,
                paginaActual: consulta.Page,
                tamanoPagina: consulta.PageSize));
        }

        public Task<OpcionesFiltroOfertasDto> ObtenerOpcionesFiltroAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcionesFiltroOfertasDto([], []));
    }
}
