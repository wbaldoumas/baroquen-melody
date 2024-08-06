using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;

internal sealed class HasTargetOrnamentation(OrnamentationType ornamentation) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) =>
        item.CurrentBeat.Chord.Notes.Exists(note => note.OrnamentationType == ornamentation) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
