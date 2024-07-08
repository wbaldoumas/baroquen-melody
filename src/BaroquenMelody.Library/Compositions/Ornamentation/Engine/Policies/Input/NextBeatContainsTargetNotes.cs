using Atrea.PolicyEngine.Policies.Input;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;

internal sealed class NextBeatContainsTargetNotes(HashSet<NoteName> targetNotes) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) =>
        item.NextBeat!.Chord.Notes.TrueForAll(note => targetNotes.Contains(note.Raw.NoteName)) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
