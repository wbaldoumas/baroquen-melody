using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;

internal sealed class IsFifthOfChord(
    IChordNumberIdentifier chordNumberIdentifier,
    CompositionConfiguration compositionConfiguration
) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item)
    {
        var chordNumber = chordNumberIdentifier.IdentifyChordNumber(item.CurrentBeat.Chord);
        var noteName = item.CurrentBeat[item.Voice].Raw.NoteName;

        return chordNumber switch
        {
            ChordNumber.I when noteName == compositionConfiguration.Scale.Dominant => InputPolicyResult.Continue,
            ChordNumber.II when noteName == compositionConfiguration.Scale.Submediant => InputPolicyResult.Continue,
            ChordNumber.III when noteName == compositionConfiguration.Scale.LeadingTone => InputPolicyResult.Continue,
            ChordNumber.IV when noteName == compositionConfiguration.Scale.Tonic => InputPolicyResult.Continue,
            ChordNumber.V when noteName == compositionConfiguration.Scale.Supertonic => InputPolicyResult.Continue,
            ChordNumber.VI when noteName == compositionConfiguration.Scale.Mediant => InputPolicyResult.Continue,
            ChordNumber.VII when noteName == compositionConfiguration.Scale.Subdominant => InputPolicyResult.Continue,
            _ => InputPolicyResult.Reject
        };
    }
}
