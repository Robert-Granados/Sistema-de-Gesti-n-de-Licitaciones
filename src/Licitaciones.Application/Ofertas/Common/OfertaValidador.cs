using Licitaciones.Application.Common.Clock;
using Licitaciones.Application.Ofertas.Exceptions;
using Licitaciones.Application.Ofertas.Ports;

namespace Licitaciones.Application.Ofertas.Common;

/// <summary>
/// Reglas compartidas por registrar, editar y eliminar ofertas
/// (HU-18/HU-21/HU-23/HU-24): la licitación debe existir, estar
/// publicada y no vencida, y el monto debe ser válido y no superar
/// el presupuesto. Se evita duplicar estas reglas en cada comando.
/// </summary>
public sealed class OfertaValidador(
    IOfertaValidacionRepository validacionRepository,
    IClock clock)
{
    private const string ParametroMonto = "MontoOfertadoCrc";

    public async Task ValidarLicitacionAbiertaAsync(
        Guid licitacionId,
        string verboOferta,
        CancellationToken cancellationToken = default)
    {
        var fechaCierre = await validacionRepository.ObtenerFechaCierreAsync(
            licitacionId,
            cancellationToken);

        if (fechaCierre is null)
        {
            throw new LicitacionNoDisponibleException(
                "La licitación indicada no existe.");
        }

        if (!await validacionRepository.ExisteLicitacionPublicadaAsync(
                licitacionId,
                cancellationToken))
        {
            throw new LicitacionNoDisponibleException(
                $"Solo se pueden {verboOferta} ofertas para licitaciones publicadas.");
        }

        if (fechaCierre.Value <= clock.UtcNow)
        {
            throw new LicitacionNoDisponibleException(
                $"No se pueden {verboOferta} ofertas para licitaciones vencidas.");
        }
    }

    public async Task ValidarMontoAsync(
        decimal montoOfertadoCrc,
        Guid licitacionId,
        CancellationToken cancellationToken = default)
    {
        if (montoOfertadoCrc <= 0)
        {
            throw new ArgumentOutOfRangeException(
                ParametroMonto,
                "El monto ofertado debe ser mayor que cero.");
        }

        var presupuesto = await validacionRepository.ObtenerPresupuestoAsync(
            licitacionId,
            cancellationToken);

        if (montoOfertadoCrc > presupuesto)
        {
            throw new ArgumentOutOfRangeException(
                ParametroMonto,
                $"El monto ofertado ({montoOfertadoCrc:N2} CRC) no puede superar el presupuesto estimado ({presupuesto:N2} CRC).");
        }
    }
}
