using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Enums;

namespace BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Policies.Input;

internal sealed class HasTargetOrnamentations(OrnamentationType primaryOrnamentationType, OrnamentationType secondaryOrnamentationType) : IInputPolicy<OrnamentationCleaningItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationCleaningItem item) => item.Note.OrnamentationType == primaryOrnamentationType && item.OtherNote.OrnamentationType == secondaryOrnamentationType
        ? InputPolicyResult.Continue
        : InputPolicyResult.Reject;
}
