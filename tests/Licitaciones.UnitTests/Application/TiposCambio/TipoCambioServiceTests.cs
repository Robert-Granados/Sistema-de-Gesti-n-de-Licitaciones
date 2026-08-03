using Licitaciones.Application.TiposCambio;
using Licitaciones.Domain.Entities;

namespace Licitaciones.UnitTests.Application.TiposCambio;

public sealed class TipoCambioServiceTests
{
    [Fact]
    public async Task Crear_RegistraValorYFechaVigencia()
    {
        var repository = new RepositoryFake();
        var service = new TipoCambioService(repository);
        var fecha = new DateTimeOffset(2026, 8, 2, 10, 0, 0, TimeSpan.FromHours(-6));

        var resultado = await service.CrearAsync(515.75m, fecha);

        Assert.Equal(515.75m, resultado.CrcPorUsd);
        Assert.Equal(fecha, resultado.FechaVigencia);
        Assert.False(resultado.Activo);
    }

    [Fact]
    public async Task Crear_ConValorNoPositivo_EsRechazadoAntesDePersistir()
    {
        var repository = new RepositoryFake();
        var service = new TipoCambioService(repository);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.CrearAsync(0m, DateTimeOffset.UtcNow));

        Assert.Empty(repository.TiposCambio);
    }

    [Fact]
    public async Task Activar_UsaOperacionTransaccionalYDejaUnSoloActivo()
    {
        var anterior = new TipoCambio(510m, DateTimeOffset.UtcNow.AddDays(-1), activo: true);
        var nuevo = new TipoCambio(515m, DateTimeOffset.UtcNow);
        var repository = new RepositoryFake(anterior, nuevo);
        var service = new TipoCambioService(repository);

        var resultado = await service.ActivarAsync(nuevo.Id);

        Assert.True(repository.ActivacionTransaccionalInvocada);
        Assert.True(resultado.Activo);
        Assert.False(anterior.Activo);
        Assert.Single(repository.TiposCambio, t => t.Activo);
    }

    private sealed class RepositoryFake(params TipoCambio[] tiposCambio)
        : ITipoCambioRepository
    {
        public List<TipoCambio> TiposCambio { get; } = [.. tiposCambio];
        public bool ActivacionTransaccionalInvocada { get; private set; }

        public Task<IReadOnlyList<TipoCambio>> ListarAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<TipoCambio>>(
                TiposCambio.OrderByDescending(t => t.FechaVigencia).ToList());

        public Task<TipoCambio?> ObtenerAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(TiposCambio.FirstOrDefault(t => t.Id == id));

        public Task AgregarAsync(
            TipoCambio tipoCambio,
            CancellationToken cancellationToken = default)
        {
            TiposCambio.Add(tipoCambio);
            return Task.CompletedTask;
        }

        public Task GuardarAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task EliminarAsync(
            TipoCambio tipoCambio,
            CancellationToken cancellationToken = default)
        {
            TiposCambio.Remove(tipoCambio);
            return Task.CompletedTask;
        }

        public Task ActivarEnTransaccionAsync(
            TipoCambio tipoCambio,
            CancellationToken cancellationToken = default)
        {
            ActivacionTransaccionalInvocada = true;
            foreach (var otro in TiposCambio.Where(t => t.Id != tipoCambio.Id))
            {
                otro.Desactivar();
            }

            tipoCambio.Activar();
            return Task.CompletedTask;
        }
    }
}
