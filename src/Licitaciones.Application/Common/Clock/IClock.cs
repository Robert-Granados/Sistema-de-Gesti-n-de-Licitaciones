namespace Licitaciones.Application.Common.Clock;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
