using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
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
///     ties held pairs deterministically. Texture-figuration notes take each gate's build-time family weight:
///     near-certainty for the configured texture's figure family, silence for everything else, so the
///     figuration voice renders only its family — the texture is a configuration constant, resolved here
///     once, never per note. The ornamentation gates additionally scale by any recorded division-escalation
///     intensity; the sustain gate never scales, because a calm statement's reappearing ties are the
///     escalation's quiet end — scaling them away would defeat it.
/// </remarks>
internal sealed class VoiceRhythmPolicyTransformer(
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    IVoiceRhythmLedger voiceRhythmLedger,
    CompositionConfiguration compositionConfiguration) : IVoiceRhythmPolicyTransformer
{
    internal const int FloridSubdividingProbability = 70;

    internal const int HeldNoteProbability = 0;

    internal const int HeldSustainProbability = 100;

    // Near-certain so the family reads as a persistent fabric, while leaving rare eligible sites unfired so
    // the tread breathes; a single ear-tunable constant.
    internal const int TextureFigureProbability = 95;

    // Very low: accent-tier family members stay in the fabric's palette (same eighth grid) but appear as
    // occasional color rather than competing with the carrying figures - the ear reported the octave-pedal
    // bounce as a mannerism when it recurs, so within the texture it must stay rare.
    internal const int TextureAccentFigureProbability = 10;

    private readonly bool _isEnabled = (compositionConfiguration.VoiceRhythmConfiguration ?? VoiceRhythmConfiguration.Default).Enabled;

    private readonly TextureType _texture = (compositionConfiguration.TextureConfiguration ?? TextureConfiguration.Default).Texture;

    // Deliberately an explicit set rather than a runtime derivation: a deciding test re-derives the membership
    // from the MusicalTimeSpanCalculator's 4/4 column, so a new beat-subdividing ornamentation fails the build
    // until it is added here (or consciously excluded).
    internal static FrozenSet<OrnamentationType> SubdividingOrnamentationTypes { get; } = new[]
    {
        OrnamentationType.DoubleTurn,
        OrnamentationType.DoubleInvertedTurn,
        OrnamentationType.DoubleRun,
        OrnamentationType.SequencedThirds,
        OrnamentationType.DoublePedalPassingTone,
        OrnamentationType.Trill
    }.ToFrozenSet();

    // Family membership is onset-spacing uniformity, pinned by deciding tests against the calculator's 4/4
    // column: a texture's audible identity is the spacing of its attacks, so each family admits only figures
    // rendering the same even grid. Walking = the even quarter tread; wider stepwise figures (delayed and
    // double passing tones, runs) mix syncopation and eighths into an active figured bass instead.
    internal static FrozenSet<OrnamentationType> WalkingFigures { get; } = new[]
    {
        OrnamentationType.RepeatedNote,
        OrnamentationType.PassingTone
    }.ToFrozenSet();

    // BrokenChord = the even eighth pattern (three sub-notes per beat); DecorateThird is excluded for its
    // mixed sixteenth/eighth spacing. The Arpeggio's chord-tone cell and the chordal pedal CARRY the
    // fabric (both degree-gated, both spelling the sounding harmony); the four moving octave-pedal
    // variants are family members for spacing but ride the accent tier - a census showed the
    // passing-tone pedals otherwise outdrawing the arpeggio, and their octave-leap cells recur as a
    // mannerism. The plain octave pedals are deliberately EVICTED: their interior note is a repeat, so
    // the cell is one pitch class four times - a measured census showed that harmonically empty bounce
    // claiming ~40% of the fabric, which is what the ear reported as irritating. All six remain
    // ordinary ornaments everywhere else.
    internal static FrozenSet<OrnamentationType> BrokenChordFigures { get; } = new[]
    {
        OrnamentationType.Arpeggio,
        OrnamentationType.OctavePedalArpeggio,
        OrnamentationType.UpperOctavePedalArpeggio,
        OrnamentationType.OctavePedalPassingTone,
        OrnamentationType.UpperOctavePedalPassingTone,
        OrnamentationType.Pedal
    }.ToFrozenSet();

    // The subset of BrokenChordFigures gated at the accent tier instead of the carrying tier. A subset
    // rather than a second family: membership in BrokenChordFigures still states the spacing identity
    // (and the spacing deciding tests iterate it whole); this set only modulates weight within it.
    internal static FrozenSet<OrnamentationType> BrokenChordAccentFigures { get; } = new[]
    {
        OrnamentationType.OctavePedalArpeggio,
        OrnamentationType.UpperOctavePedalArpeggio,
        OrnamentationType.OctavePedalPassingTone,
        OrnamentationType.UpperOctavePedalPassingTone
    }.ToFrozenSet();

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
            textureProbability: WantsToOrnament.DefaultProbability,
            floridProbability: WantsToOrnament.DefaultProbability,
            scaleByIntensity: false)
        : new WantsToOrnament(weightedRandomBooleanGenerator);

    // Only the subdividing tier scales by intensity: decoration coverage is near-saturated, so scaling
    // every figure's weight mostly reshuffles which processor claims a note (measured, not assumed - the
    // uniform scale produced no statement arc), while a tier-only scale ramps the division figures
    // themselves against constant competition.
    private RoleAwareWantsToOrnament CreateRoleAwareGate(OrnamentationConfiguration ornamentationConfiguration) => new(
        weightedRandomBooleanGenerator,
        voiceRhythmLedger,
        probability: ornamentationConfiguration.Probability,
        heldProbability: HeldNoteProbability,
        textureProbability: ResolveTextureProbability(ornamentationConfiguration),
        floridProbability: SubdividingOrnamentationTypes.Contains(ornamentationConfiguration.OrnamentationType)
            ? FloridSubdividingProbability
            : ornamentationConfiguration.Probability,
        scaleByIntensity: SubdividingOrnamentationTypes.Contains(ornamentationConfiguration.OrnamentationType));

    // With no texture configured the weight is neutral (the store stays empty, but a spurious mark would
    // then behave standardly rather than silently silencing); with one configured, carrying in-family
    // figures are near-certain, accent-tier members are rare color, and everything else is silenced -
    // Chordal's empty family silences the whole voice. Every texture is classified explicitly so a new
    // TextureType fails loudly here instead of silently inheriting Chordal's silence - the same
    // discipline the subdividing set demands.
    private int ResolveTextureProbability(OrnamentationConfiguration ornamentationConfiguration) => _texture switch
    {
        TextureType.None => ornamentationConfiguration.Probability,
        TextureType.Walking => WalkingFigures.Contains(ornamentationConfiguration.OrnamentationType) ? TextureFigureProbability : HeldNoteProbability,
        TextureType.BrokenChord when BrokenChordAccentFigures.Contains(ornamentationConfiguration.OrnamentationType) => TextureAccentFigureProbability,
        TextureType.BrokenChord => BrokenChordFigures.Contains(ornamentationConfiguration.OrnamentationType) ? TextureFigureProbability : HeldNoteProbability,
        TextureType.Chordal => HeldNoteProbability,
        _ => throw new InvalidOperationException("The configured texture type has no figure-family classification.")
    };
}
