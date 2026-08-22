using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Rhythm;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

/// <summary>
///     The role-aware sibling of <see cref="WantsToOrnament"/>: draws exactly once per item like its sibling —
///     the weight can silence, boost, or scale, never add or remove a draw — but resolves the effective weight
///     from the note's recorded rhythm role, then scales it by the note's division-escalation intensity when
///     one is recorded and scaling is enabled. Pad, held, and texture-figuration notes short-circuit to
///     their own weights (a texture's fabric is never scaled by the ground's escalation — a decided
///     contract, not an accident of today's disjoint recording paths); the pad branch resolves first
///     because every pad is also held. A note the ledger does not know (the fugal exposition
///     and ending, the ground's composed close, every deep copy) gets the standard weight, so unrecorded
///     material behaves exactly as it does without roles.
/// </summary>
/// <param name="weightedRandomBooleanGenerator">The weighted random boolean generator used to draw.</param>
/// <param name="voiceRhythmLedger">The ledger of notes carrying rhythm roles and escalation intensities.</param>
/// <param name="probability">The weight for notes carrying no role.</param>
/// <param name="heldProbability">The weight for notes recorded as held.</param>
/// <param name="padProbability">
///     The weight for notes recorded as texture pads, resolved before the held branch (every pad is also
///     held). Deliberately required, never defaulted: the gentle-figure ornamentation gates let a pad
///     breathe while every other figure stays silenced, and the sustain gate keeps the held tie weight so
///     pads tie exactly as before. Every construction site decides.
/// </param>
/// <param name="textureProbability">
///     The weight for notes recorded as texture figuration. Deliberately required, never defaulted: each
///     ornamentation gate takes its build-time family weight (in-family near-certainty, out-of-family
///     silence) while the sustain gate stays neutral. Every construction site decides.
/// </param>
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
    int padProbability,
    int textureProbability,
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

        if (voiceRhythmLedger.IsTexturePadNote(note))
        {
            return padProbability;
        }

        if (voiceRhythmLedger.IsHeldNote(note))
        {
            return heldProbability;
        }

        if (voiceRhythmLedger.IsTextureFigurationNote(note))
        {
            return textureProbability;
        }

        var weight = voiceRhythmLedger.IsFloridNote(note) ? floridProbability : probability;

        return scaleByIntensity && voiceRhythmLedger.TryGetDivisionIntensity(note, out var intensity)
            ? Math.Clamp(weight * intensity / 100, 0, 100)
            : weight;
    }
}
