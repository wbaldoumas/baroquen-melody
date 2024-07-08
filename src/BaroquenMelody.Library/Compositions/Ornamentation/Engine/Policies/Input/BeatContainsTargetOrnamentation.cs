using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;

internal sealed class BeatContainsTargetOrnamentation(OrnamentationType targetOrnamentation) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) =>
        item.CurrentBeat.Chord.Notes.Exists(note => note.OrnamentationType == targetOrnamentation) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
