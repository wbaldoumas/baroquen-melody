using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Rhythm;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

/// <summary>
///     The role-aware sibling of <see cref="WantsToOrnament"/>: draws exactly once per item like its sibling —
///     the weight can silence or boost, never add or remove a draw — but resolves the effective weight from
///     the note's recorded rhythm role. A note the ledger does not know (the exposition, the ending, the
///     ground bass form, every deep copy) gets the standard weight, so unrecorded material behaves exactly as
///     it does without roles.
/// </summary>
/// <param name="weightedRandomBooleanGenerator">The weighted random boolean generator used to draw.</param>
/// <param name="voiceRhythmLedger">The ledger of notes carrying the held or florid rhythm role.</param>
/// <param name="probability">The weight for notes carrying no role.</param>
/// <param name="heldProbability">The weight for notes recorded as held.</param>
/// <param name="floridProbability">The weight for notes recorded as florid.</param>
internal sealed class RoleAwareWantsToOrnament(
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    IVoiceRhythmLedger voiceRhythmLedger,
    int probability,
    int heldProbability,
    int floridProbability) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) =>
        weightedRandomBooleanGenerator.IsTrue(ResolveWeight(item)) ? InputPolicyResult.Continue : InputPolicyResult.Reject;

    private int ResolveWeight(OrnamentationItem item)
    {
        if (!item.CurrentBeat.Chord.ContainsInstrument(item.Instrument))
        {
            return probability;
        }

        var note = item.CurrentBeat.Chord[item.Instrument];

        if (voiceRhythmLedger.IsHeldNote(note))
        {
            return heldProbability;
        }

        return voiceRhythmLedger.IsFloridNote(note) ? floridProbability : probability;
    }
}
