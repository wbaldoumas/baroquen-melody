namespace BaroquenMelody.App.Components.Shared;

/// <summary>
///     Shared copy for the ground bass feasibility warnings, raised by every surface whose edits can empty
///     the feasible pattern bank: the instrumentation panel for range and voice edits, and the composition
///     configuration card for key and form edits.
/// </summary>
internal static class GroundBassWarnings
{
    /// <summary>
    ///     Warns that no ground bass pattern fits the current configuration, so a ground bass composition
    ///     will fall back to the fugue form.
    /// </summary>
    public const string FugueFallback = "No ground bass pattern fits the lowest voice's range in the selected key. Ground Bass compositions will fall back to the fugue form.";
}
