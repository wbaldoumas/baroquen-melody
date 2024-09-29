using BaroquenMelody.Infrastructure.Equality;

namespace BaroquenMelody.Infrastructure.Collections.Extensions;

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

    /// <summary>
    ///     Generates the power set of a given <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to generate the power set of.</param>
    /// <returns>A <see cref="IEnumerable{T}"/> containing all subsets of the given <see cref="IEnumerable{T}"/>.</returns>
    public static IEnumerable<HashSet<T>> ToPowerSet<T>(this IEnumerable<T> source)
    {
        var sourceArray = source.ToArray();
        var sourceArrayLength = sourceArray.Length;
        var sourceArrayPowerSetLength = 1 << sourceArrayLength;
        var powerSet = new HashSet<T>[sourceArrayPowerSetLength];

        powerSet[0] = [];

        for (var i = 1; i < sourceArrayPowerSetLength; i++)
        {
            var subset = new HashSet<T>();

            for (var j = 0; j < sourceArrayLength; j++)
            {
                if ((i & (1 << j)) != 0)
                {
                    subset.Add(sourceArray[j]);
                }
            }

            powerSet[i] = subset;
        }

        return powerSet;
    }

    /// <summary>
    ///     Gets the hash code of the content of an <see cref="IEnumerable{T}"/>, irrespective of order.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IEnumerable{T}"/>.</typeparam>
    /// <param name="source">The <see cref="IEnumerable{T}"/> to generate the hash code of.</param>
    /// <returns>The hash code of the content of the <see cref="IEnumerable{T}"/>.</returns>
    public static int GetContentHashCode<T>(this IEnumerable<T> source)
        where T : notnull
    {
        unchecked
        {
            return source.Order().Aggregate(
                HashCodeGeneration.Seed,
                (current, item) => current * HashCodeGeneration.Multiplier ^ item.GetHashCode()
            );
        }
    }
}
