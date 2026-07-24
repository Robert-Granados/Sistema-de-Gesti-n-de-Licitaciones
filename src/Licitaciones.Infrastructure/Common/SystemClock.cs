using Licitaciones.Application.Common.Clock;

namespace Licitaciones.Infrastructure.Common;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
