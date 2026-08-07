using Licitaciones.Application.Common.Clock;

namespace Licitaciones.UnitTests.Common;

public sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; private set; } = utcNow;

    public void Set(DateTimeOffset utcNow) => UtcNow = utcNow;

    public void Advance(TimeSpan amount) => UtcNow = UtcNow.Add(amount);
}
