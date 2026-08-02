using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Rhythm;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

/// <summary>
///     The role-aware sibling of <see cref="WantsToOrnament"/>: draws exactly once per item like its sibling —
///     the weight can silence, boost, or scale, never add or remove a draw — but resolves the effective weight
///     from the note's recorded rhythm role, then scales it by the note's division-escalation intensity when
///     one is recorded and scaling is enabled. A note the ledger does not know (the fugal exposition and
///     ending, the ground's composed close, every deep copy) gets the standard weight, so unrecorded
///     material behaves exactly as it does without roles.
/// </summary>
/// <param name="weightedRandomBooleanGenerator">The weighted random boolean generator used to draw.</param>
/// <param name="voiceRhythmLedger">The ledger of notes carrying rhythm roles and escalation intensities.</param>
/// <param name="probability">The weight for notes carrying no role.</param>
/// <param name="heldProbability">The weight for notes recorded as held.</param>
/// <param name="floridProbability">The weight for notes recorded as florid.</param>
/// <param name="scaleByIntensity">
///     Whether a recorded division intensity scales the resolved weight. Deliberately required, never
///     defaulted: only the subdividing ornamentation gates scale, and the sustain gate must not — a calm
///     statement's ties ARE its calm, so scaling the sustain gate down would suppress exactly the effect
///     the escalation's quiet end is for. Every construction site decides.
/// </param>
internal sealed class RoleAwareWantsToOrnament(
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    IVoiceRhythmLedger voiceRhythmLedger,
    int probability,
    int heldProbability,
    int floridProbability,
    bool scaleByIntensity) : IInputPolicy<OrnamentationItem>
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

        var weight = voiceRhythmLedger.IsFloridNote(note) ? floridProbability : probability;

        return scaleByIntensity && voiceRhythmLedger.TryGetDivisionIntensity(note, out var intensity)
            ? Math.Clamp(weight * intensity / 100, 0, 100)
            : weight;
    }
}
