using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Enums;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

internal sealed class HasTargetOrnamentation(OrnamentationType ornamentation) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) =>
        item.CurrentBeat.Chord.Notes.Exists(note => note.OrnamentationType == ornamentation) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
