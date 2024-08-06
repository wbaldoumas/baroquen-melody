using Atrea.PolicyEngine.Policies.Input;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;

internal sealed class NextBeatHasTargetNotes(HashSet<NoteName> noteNames) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) =>
        item.NextBeat!.Chord.Notes.TrueForAll(note => noteNames.Contains(note.NoteName)) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
