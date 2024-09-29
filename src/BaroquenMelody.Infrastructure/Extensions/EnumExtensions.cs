namespace BaroquenMelody.Infrastructure.Extensions;

/// <summary>
///     A home for general enum extension methods.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    ///    Converts an enum to a space-separated string.
    /// </summary>
    /// <param name="source">The enum to convert.</param>
    /// <returns>The space-separated string.</returns>
    public static string ToSpaceSeparatedString(this Enum source) => source.ToString().ToSpaceSeparatedString();
}
