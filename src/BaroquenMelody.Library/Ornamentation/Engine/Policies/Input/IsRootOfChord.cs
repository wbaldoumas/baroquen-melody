using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.MusicTheory;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

internal sealed class IsRootOfChord(
    IChordNumberIdentifier chordNumberIdentifier,
    CompositionConfiguration compositionConfiguration
) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item)
    {
        var chordNumber = chordNumberIdentifier.IdentifyChordNumber(item.CurrentBeat.Chord);
        var noteName = item.CurrentBeat[item.Instrument].NoteName;

        return noteName == ChordTriad.FromChordNumber(compositionConfiguration.Scale, chordNumber)?.Root
            ? InputPolicyResult.Continue
            : InputPolicyResult.Reject;
    }
}
