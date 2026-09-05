using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

/// <summary>
///     Continues only when both scale neighbors of the item's current note (one step up and one step down) are
///     playable by the instrument — the guard a neighbor-oscillating figure (trill, mordent) needs before it can
///     land on a note. Both checks are evaluated eagerly (Atrea's <c>And</c> never short-circuits), and each indexes
///     the scale's note list unguarded — safe for every reachable caller today because the trill/mordent sites run
///     before tonicization introduces chromatic notes; a caller that could see an out-of-scale note needs a bounds
///     guard here first.
/// </summary>
internal sealed class HasNeighborNotesWithinInstrumentRange(CompositionConfiguration compositionConfiguration) : IInputPolicy<OrnamentationItem>
{
    private readonly IInputPolicy<OrnamentationItem> _neighborsAreWithinInstrumentRange =
        new IsIntervalWithinInstrumentRange(compositionConfiguration, 1).And(new IsIntervalWithinInstrumentRange(compositionConfiguration, -1));

    public InputPolicyResult ShouldProcess(OrnamentationItem item) => _neighborsAreWithinInstrumentRange.ShouldProcess(item);
}
