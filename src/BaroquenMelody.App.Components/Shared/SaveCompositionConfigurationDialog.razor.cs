using System.Text.RegularExpressions;

namespace BaroquenMelody.App.Components.Shared;

/// <summary>
///     A dialog for saving a composition configuration.
/// </summary>
public partial class SaveCompositionConfigurationDialog
{
    [GeneratedRegex(@"^[\w\s-]+$", RegexOptions.Compiled, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ValidFilenameRegex();
}
