namespace BaroquenMelody.Library.Infrastructure.Collections.Extensions;

/// <summary>
///     A home for extension methods for <see cref="IEnumerable{T}"/>.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    ///     Trims the edges (both beginning and ending) for the given <paramref name="source"/> enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the enumerable.</typeparam>
    /// <param name="source">The source enumerable to trim.</param>
    /// <param name="count">The number of elements to trim from the beginning and ending of the enumerable.</param>
    /// <returns>The trimmed enumerable.</returns>
    public static IEnumerable<T> TrimEdges<T>(this IEnumerable<T> source, int count = 1) => source.Skip(count).SkipLast(count);
}
