using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Licitaciones.Listar;
using Licitaciones.Application.Licitaciones.Ports;
using Licitaciones.Domain.Enums;

namespace Licitaciones.UnitTests.Application.Licitaciones;

public sealed class ListarLicitacionesHandlerTests
{
    private static readonly FakeClock Clock = new(new(2026, 7, 24, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_ConParametrosInvalidos_UsaValoresSeguros()
    {
        var repository = new FakeLicitacionReadRepository();
        var handler = new ListarLicitacionesHandler(repository, Clock);

        await handler.HandleAsync(new ListarLicitacionesQuery(
            Page: 0,
            PageSize: 500,
            Search: "  lic-001  ",
            SortBy: "desconocido"));

        Assert.NotNull(repository.UltimaConsulta);
        Assert.Equal(1, repository.UltimaConsulta.Page);
        Assert.Equal(100, repository.UltimaConsulta.PageSize);
        Assert.Equal("LIC-001", repository.UltimaConsulta.Search);
        Assert.Equal(OrdenLicitacion.FechaCierreAscendente, repository.UltimaConsulta.SortBy);
    }

    [Fact]
    public async Task Handle_ConFiltroEstado_PasaAlRepository()
    {
        var repository = new FakeLicitacionReadRepository();
        var handler = new ListarLicitacionesHandler(repository, Clock);

        await handler.HandleAsync(new ListarLicitacionesQuery(
            FiltroEstado: "Publicada"));

        Assert.NotNull(repository.UltimaConsulta);
        Assert.Equal("Publicada", repository.UltimaConsulta.FiltroEstado);
    }

    [Fact]
    public async Task Handle_ConFechas_PasaAlRepository()
    {
        var repository = new FakeLicitacionReadRepository();
        var handler = new ListarLicitacionesHandler(repository, Clock);
        var desde = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var hasta = new DateTimeOffset(2026, 12, 31, 23, 59, 59, TimeSpan.Zero);

        await handler.HandleAsync(new ListarLicitacionesQuery(
            FechaDesde: desde,
            FechaHasta: hasta));

        Assert.NotNull(repository.UltimaConsulta);
        Assert.Equal(desde, repository.UltimaConsulta.FechaDesde);
        Assert.Equal(hasta, repository.UltimaConsulta.FechaHasta);
    }

    [Fact]
    public void PaginaResultado_CalculaTotalDePaginas()
    {
        var pagina = new PaginaResultado<LicitacionListadoDto>(
            [],
            totalRegistros: 21,
            paginaActual: 2,
            tamanoPagina: 10);

        Assert.Equal(3, pagina.TotalPaginas);
        Assert.True(pagina.TienePaginaAnterior);
        Assert.True(pagina.TienePaginaSiguiente);
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }

    private sealed class FakeLicitacionReadRepository : ILicitacionReadRepository
    {
        public LicitacionesConsulta? UltimaConsulta { get; private set; }

        public Task<PaginaResultado<LicitacionListadoDto>> ListarAsync(
            LicitacionesConsulta consulta,
            DateTimeOffset ahoraUtc,
            CancellationToken cancellationToken = default)
        {
            UltimaConsulta = consulta;
            return Task.FromResult(new PaginaResultado<LicitacionListadoDto>(
                [],
                totalRegistros: 0,
                paginaActual: consulta.Page,
                tamanoPagina: consulta.PageSize));
        }
    }
}
