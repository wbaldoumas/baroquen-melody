using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Tests.TestData;

/// <summary>
///     An <see cref="ILogger"/> that records every formatted message it is given, so tests can assert on what the
///     composition actually logs (the source-generated log methods make NSubstitute's generic <c>Log</c> matching awkward).
/// </summary>
internal sealed class CapturingLogger : ILogger
{
    public List<string> Messages { get; } = [];

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
        Messages.Add(formatter(state, exception));
}
