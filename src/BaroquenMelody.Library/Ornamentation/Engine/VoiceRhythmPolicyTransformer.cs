using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Rhythm;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Engine;

/// <inheritdoc cref="IVoiceRhythmPolicyTransformer"/>
/// <remarks>
///     The substitution is by policy instance, not by position: the probability gate is the first input policy
///     of most processors but sits behind a deterministic precondition in the interval-decoration family, and
///     an in-place replacement is reach- and draw-identical wherever the gate lives. Held notes take weight
///     zero everywhere (any figure on a held run's note would block its sustain tie — the draw still happens,
///     preserving the stream), florid notes boost only the beat-subdividing figure tier, and the sustain gate
///     ties held pairs deterministically.
/// </remarks>
internal sealed class VoiceRhythmPolicyTransformer(
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    IVoiceRhythmLedger voiceRhythmLedger,
    CompositionConfiguration compositionConfiguration) : IVoiceRhythmPolicyTransformer
{
    internal const int FloridSubdividingProbability = 70;

    internal const int HeldNoteProbability = 0;

    internal const int HeldSustainProbability = 100;

    private static readonly FrozenSet<OrnamentationType> SubdividingOrnamentationTypes = new[]
    {
        OrnamentationType.DoubleTurn,
        OrnamentationType.DoubleInvertedTurn,
        OrnamentationType.DoubleRun,
        OrnamentationType.SequencedThirds,
        OrnamentationType.DoublePedalPassingTone,
        OrnamentationType.Trill
    }.ToFrozenSet();

    private readonly bool _isEnabled = (compositionConfiguration.VoiceRhythmConfiguration ?? VoiceRhythmConfiguration.Default).Enabled;

    public IInputPolicy<OrnamentationItem>[] Transform(OrnamentationConfiguration ornamentationConfiguration, IInputPolicy<OrnamentationItem>[] inputPolicies)
    {
        if (!_isEnabled)
        {
            return inputPolicies;
        }

        return inputPolicies
            .Select(inputPolicy => inputPolicy is WantsToOrnament
                ? CreateRoleAwareGate(ornamentationConfiguration)
                : inputPolicy)
            .ToArray();
    }

    public IInputPolicy<OrnamentationItem> CreateSustainGate() => _isEnabled
        ? new RoleAwareWantsToOrnament(
            weightedRandomBooleanGenerator,
            voiceRhythmLedger,
            probability: WantsToOrnament.DefaultProbability,
            heldProbability: HeldSustainProbability,
            floridProbability: WantsToOrnament.DefaultProbability)
        : new WantsToOrnament(weightedRandomBooleanGenerator);

    private RoleAwareWantsToOrnament CreateRoleAwareGate(OrnamentationConfiguration ornamentationConfiguration) => new(
        weightedRandomBooleanGenerator,
        voiceRhythmLedger,
        probability: ornamentationConfiguration.Probability,
        heldProbability: HeldNoteProbability,
        floridProbability: SubdividingOrnamentationTypes.Contains(ornamentationConfiguration.OrnamentationType)
            ? FloridSubdividingProbability
            : ornamentationConfiguration.Probability);
}
