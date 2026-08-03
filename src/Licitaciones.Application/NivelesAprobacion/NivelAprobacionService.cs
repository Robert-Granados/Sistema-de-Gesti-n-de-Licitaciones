using Licitaciones.Domain.Entities;

namespace Licitaciones.Application.NivelesAprobacion;

public sealed class NivelAprobacionService(INivelAprobacionRepository repository)
{
    public async Task<IReadOnlyList<NivelAprobacionDto>> ListarAsync(
        CancellationToken cancellationToken = default) =>
        (await repository.ListarOrdenadosAsync(cancellationToken))
        .Select(Mapear)
        .ToList();

    public async Task<NivelAprobacionDto?> ObtenerAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var nivel = await repository.ObtenerAsync(id, cancellationToken);
        return nivel is null ? null : Mapear(nivel);
    }

    public async Task<NivelAprobacionDto> CrearAsync(
        decimal montoMinimoCrc,
        decimal? montoMaximoCrc,
        string aprobador,
        CancellationToken cancellationToken = default)
    {
        var existentes = await repository.ListarOrdenadosAsync(cancellationToken);
        ValidarDisponibilidad(existentes, montoMinimoCrc, montoMaximoCrc);

        var nivel = new NivelAprobacion(montoMinimoCrc, montoMaximoCrc, aprobador);
        await repository.AgregarAsync(nivel, cancellationToken);
        return Mapear(nivel);
    }

    public async Task<NivelAprobacionDto> EditarAsync(
        Guid id,
        decimal montoMinimoCrc,
        decimal? montoMaximoCrc,
        string aprobador,
        CancellationToken cancellationToken = default)
    {
        var nivel = await repository.ObtenerAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("El nivel de aprobación no existe.");
        var existentes = await repository.ListarOrdenadosAsync(cancellationToken);
        ValidarDisponibilidad(existentes, montoMinimoCrc, montoMaximoCrc, id);

        nivel.Actualizar(montoMinimoCrc, montoMaximoCrc, aprobador);
        await repository.GuardarAsync(cancellationToken);
        return Mapear(nivel);
    }

    public async Task EliminarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var nivel = await repository.ObtenerAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException("El nivel de aprobación no existe.");
        await repository.EliminarAsync(nivel, cancellationToken);
    }

    private static void ValidarDisponibilidad(
        IEnumerable<NivelAprobacion> existentes,
        decimal minimo,
        decimal? maximo,
        Guid? excluirId = null)
    {
        var otros = existentes.Where(n => n.Id != excluirId).ToList();

        if (maximo is null && otros.Any(n => n.MontoMaximoCrc is null))
        {
            throw new NivelAprobacionException(
                "Solo puede existir un rango abierto sin monto máximo.");
        }

        var seTraslapa = otros.Any(n =>
            minimo <= (n.MontoMaximoCrc ?? decimal.MaxValue)
            && n.MontoMinimoCrc <= (maximo ?? decimal.MaxValue));

        if (seTraslapa)
        {
            throw new NivelAprobacionException(
                "El rango indicado se traslapa con un nivel de aprobación existente.");
        }
    }

    private static NivelAprobacionDto Mapear(NivelAprobacion nivel) =>
        new(nivel.Id, nivel.MontoMinimoCrc, nivel.MontoMaximoCrc, nivel.Aprobador);
}
