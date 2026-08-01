using BaroquenMelody.Library.Forms.Enums;

namespace BaroquenMelody.App.Components.Shared;

/// <summary>
///     Shared copy for the ground bass feasibility warnings, raised by every surface whose edits can leave
///     a ground bass composition without a pattern to state: the instrumentation panel for range and voice
///     edits, and the composition configuration card for key, form, and pattern edits.
/// </summary>
internal static class GroundBassWarnings
{
    /// <summary>
    ///     Warns that no ground bass pattern fits the current configuration, so a ground bass composition
    ///     will fall back to the fugue form.
    /// </summary>
    public const string FugueFallback = "No ground bass pattern fits the lowest voice's range in the selected key. Ground Bass compositions will fall back to the fugue form.";

    /// <summary>
    ///     Warns that the specific pattern the user selected does not fit, so a ground bass composition
    ///     will fall back to the fugue form even though other patterns may fit.
    /// </summary>
    public const string SelectedPatternFugueFallback = "The selected ground bass pattern does not fit the lowest voice's range in the selected key. Ground Bass compositions will fall back to the fugue form.";

    /// <summary>
    ///     Picks the fallback warning matching the configured pattern selection.
    /// </summary>
    /// <param name="pattern">The configured pattern, or <see langword="null"/> for the composer's free draw.</param>
    /// <returns>The fallback warning to toast.</returns>
    public static string ForPattern(GroundBass? pattern) => pattern is null ? FugueFallback : SelectedPatternFugueFallback;
}
