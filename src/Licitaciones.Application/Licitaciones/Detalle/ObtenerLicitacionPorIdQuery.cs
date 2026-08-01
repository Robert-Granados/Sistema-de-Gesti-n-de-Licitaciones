using Licitaciones.Application.Common.Models;
using Licitaciones.Application.Licitaciones.Detalle;
using Licitaciones.Domain.Enums;

namespace Licitaciones.Application.Licitaciones.Detalle;

public sealed record ObtenerLicitacionPorIdQuery(Guid Id);
