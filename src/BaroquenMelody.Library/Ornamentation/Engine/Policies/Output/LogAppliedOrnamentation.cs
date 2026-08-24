using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Library.Logging;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;

/// <summary>
///     Logs the ornamentation an engine has just applied to the item's note. One instance serves every processor:
///     it reads the type off the note after the processor has stamped it, rather than carrying a type of its own.
/// </summary>
internal sealed class LogAppliedOrnamentation(ILogger logger) : IOutputPolicy<OrnamentationItem>
{
    public void Apply(OrnamentationItem item) => logger.LogOrnamentationApplied(item.CurrentBeat[item.Instrument].OrnamentationType, item.Instrument);
}
