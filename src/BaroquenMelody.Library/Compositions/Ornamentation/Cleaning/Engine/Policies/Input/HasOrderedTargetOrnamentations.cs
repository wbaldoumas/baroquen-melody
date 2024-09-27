using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Policies.Input;

internal sealed class HasOrderedTargetOrnamentations(OrnamentationType primaryOrnamentationType, OrnamentationType secondaryOrnamentationType) : IInputPolicy<OrnamentationCleaningItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationCleaningItem item)
    {
        if (item.Note.OrnamentationType == primaryOrnamentationType && item.OtherNote.OrnamentationType == secondaryOrnamentationType)
        {
            return InputPolicyResult.Continue;
        }

        return InputPolicyResult.Reject;
    }
}
