namespace BaroquenMelody.Infrastructure.Random;

/// <summary>
///     Random-selection extensions over <see cref="IRandomProvider"/> that mirror the existing
///     <c>MinBy(_ =&gt; ThreadLocalRandom.Next())</c> / <c>OrderBy(_ =&gt; ThreadLocalRandom.Next())</c> idioms,
///     routed through an injectable seam so composition-time randomness becomes deterministic under a seed.
/// </summary>
public static class RandomProviderExtensions
{
    /// <summary>
    ///     Returns a random element from <paramref name="source"/>, or <see langword="null"/> if it is empty.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="randomProvider">The random provider used to select an element.</param>
    /// <returns>A randomly selected element, or <see langword="null"/> when <paramref name="source"/> is empty.</returns>
    public static T? MinByRandom<T>(this IEnumerable<T> source, IRandomProvider randomProvider) =>
        source.MinBy(_ => randomProvider.Next());

    /// <summary>
    ///     Orders <paramref name="source"/> into a random order.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="randomProvider">The random provider used to determine the ordering.</param>
    /// <returns>The elements of <paramref name="source"/> in a random order.</returns>
    public static IOrderedEnumerable<T> OrderByRandom<T>(this IEnumerable<T> source, IRandomProvider randomProvider) =>
        source.OrderBy(_ => randomProvider.Next());
}
