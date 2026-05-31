using BaroquenMelody.Infrastructure.Random;

namespace BaroquenMelody.Infrastructure.Tests.Random;

/// <summary>
///     An <see cref="IRandomProvider"/> test double that returns a predetermined sequence of values, enabling
///     deterministic assertions about random-selection extensions. Throws when the sequence is exhausted so that
///     over-consumption is surfaced rather than silently masked.
/// </summary>
/// <param name="values">The values to return, in order, from each call to a <c>Next</c> overload.</param>
internal sealed class QueuedRandomProvider(params int[] values) : IRandomProvider
{
    private readonly Queue<int> _values = new(values);

    public int Next() => _values.Dequeue();

    public int Next(int maxValue) => Next();

    public int Next(int minValue, int maxValue) => Next();
}
