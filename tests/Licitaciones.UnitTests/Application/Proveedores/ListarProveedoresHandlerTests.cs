using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Proveedores.Listar;
using Licitaciones.Application.Proveedores.Ports;

namespace Licitaciones.UnitTests.Application.Proveedores;

public sealed class ListarProveedoresHandlerTests
{
    [Fact]
    public async Task Handle_ConParametrosInvalidos_UsaValoresSeguros()
    {
        var repository = new FakeProveedorReadRepository();
        var handler = new ListarProveedoresHandler(repository);

        await handler.HandleAsync(new ListarProveedoresQuery(
            Page: 0,
            PageSize: 500,
            Search: "  compañía   ágil ",
            SortBy: "desconocido"));

        Assert.NotNull(repository.UltimaConsulta);
        Assert.Equal(1, repository.UltimaConsulta.Page);
        Assert.Equal(100, repository.UltimaConsulta.PageSize);
        Assert.Equal("COMPANIA AGIL", repository.UltimaConsulta.Search);
        Assert.Equal(OrdenProveedor.NombreAscendente, repository.UltimaConsulta.SortBy);
    }

    [Fact]
    public async Task Handle_ConOrdenDescendente_ConservaOrdenSolicitado()
    {
        var repository = new FakeProveedorReadRepository();
        var handler = new ListarProveedoresHandler(repository);

        await handler.HandleAsync(new ListarProveedoresQuery(
            Page: 2,
            PageSize: 5,
            Search: null,
            SortBy: "nombre_desc"));

        Assert.NotNull(repository.UltimaConsulta);
        Assert.Equal(2, repository.UltimaConsulta.Page);
        Assert.Equal(5, repository.UltimaConsulta.PageSize);
        Assert.Equal(OrdenProveedor.NombreDescendente, repository.UltimaConsulta.SortBy);
    }

    [Fact]
    public void PaginaResultado_CalculaTotalDePaginas()
    {
        var pagina = new PaginaResultado<ProveedorListadoDto>(
            [],
            totalRegistros: 21,
            paginaActual: 2,
            tamanoPagina: 10);

        Assert.Equal(3, pagina.TotalPaginas);
        Assert.True(pagina.TienePaginaAnterior);
        Assert.True(pagina.TienePaginaSiguiente);
    }

    private sealed class FakeProveedorReadRepository : IProveedorReadRepository
    {
        public ProveedoresConsulta? UltimaConsulta { get; private set; }

        public Task<PaginaResultado<ProveedorListadoDto>> ListarAsync(
            ProveedoresConsulta consulta,
            CancellationToken cancellationToken = default)
        {
            UltimaConsulta = consulta;
            return Task.FromResult(new PaginaResultado<ProveedorListadoDto>(
                [],
                totalRegistros: 0,
                paginaActual: consulta.Page,
                tamanoPagina: consulta.PageSize));
        }
    }
}
