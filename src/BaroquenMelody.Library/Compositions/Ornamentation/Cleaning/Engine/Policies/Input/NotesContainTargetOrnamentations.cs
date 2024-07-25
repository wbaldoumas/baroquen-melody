using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Policies.Input;

internal sealed class NotesContainTargetOrnamentations(OrnamentationType targetOrnamentationType, OrnamentationType otherTargetOrnamentationType) : IInputPolicy<OrnamentationCleaningItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationCleaningItem item)
    {
        if (item.Note.OrnamentationType == targetOrnamentationType && item.OtherNote.OrnamentationType == otherTargetOrnamentationType)
        {
            return InputPolicyResult.Continue;
        }

        if (item.Note.OrnamentationType == otherTargetOrnamentationType && item.OtherNote.OrnamentationType == targetOrnamentationType)
        {
            return InputPolicyResult.Continue;
        }

        return InputPolicyResult.Reject;
    }
}
