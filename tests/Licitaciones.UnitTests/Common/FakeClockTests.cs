namespace Licitaciones.UnitTests.Common;

public sealed class FakeClockTests
{
    [Fact]
    public void SetYAdvance_PermitenControlarElTiempoDeterministicamente()
    {
        var initial = new DateTimeOffset(2026, 8, 7, 12, 0, 0, TimeSpan.Zero);
        var clock = new FakeClock(initial);

        clock.Advance(TimeSpan.FromHours(2));
        Assert.Equal(initial.AddHours(2), clock.UtcNow);

        var replacement = initial.AddDays(3);
        clock.Set(replacement);
        Assert.Equal(replacement, clock.UtcNow);
    }
}
