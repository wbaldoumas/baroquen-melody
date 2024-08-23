using System.Text.RegularExpressions;

namespace BaroquenMelody.Library.Infrastructure.Extensions;

/// <summary>
///     A home for string extension methods.
/// </summary>
public static partial class StringExtensions
{
    /// <summary>
    ///     Converts a PascalCase string to a space-separated string.
    /// </summary>
    /// <param name="source">The PascalCase string to convert.</param>
    /// <returns>The space-separated string.</returns>
    public static string ToSpaceSeparatedString(this string source) => PascalRegex().Replace(source, " ");

    [GeneratedRegex("(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PascalRegex();
}
