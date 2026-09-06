using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;

/// <summary>
///     The arpeggio cells: the broken-chord figure the broken-chord texture wears as its fabric, one cell per chord
///     degree the principal can sit on.
/// </summary>
internal sealed partial class OrnamentationProcessorConfigurationFactory
{
    // The arpeggio cells traverse the sounding chord's own tones from each degree: from the root the
    // textbook Alberti cell (root-fifth-third-fifth), from the third and fifth the traversals that stay
    // nearest the principal's register. Offsets are scale steps, exact chord tones under the degree gate.
    private static readonly int[] RootArpeggioTranslations = [4, 2, 4];

    private static readonly int[] ThirdArpeggioTranslations = [-2, 2, -2];

    private static readonly int[] FifthArpeggioTranslations = [-2, -4, -2];

    private IEnumerable<OrnamentationProcessorConfiguration> CreateArpeggios(OrnamentationConfiguration configuration) =>
    [
        CreateArpeggio(_isRootOfChord, RootArpeggioTranslations, configuration),
        CreateArpeggio(_isThirdOfChord, ThirdArpeggioTranslations, configuration),
        CreateArpeggio(_isFifthOfChord, FifthArpeggioTranslations, configuration)
    ];

    // Deliberately no next-motion condition: the cell ends on a chord tone of the CURRENT harmony, so any
    // continuation works - that breadth is what lets the broken-chord texture wear it as a fabric rather
    // than an accent. The range guards are derived from the cell's distinct offsets, so a re-voiced cell
    // can never leave an offset unchecked; the target-ornamentation guard is the same-beat cross-voice
    // dedup the pedal uses, so consecutive-beat cells stay legal.
    private OrnamentationProcessorConfiguration CreateArpeggio(
        IInputPolicy<OrnamentationItem> scaleDegreePolicy,
        int[] translations,
        OrnamentationConfiguration configuration
    ) => new(
        OrnamentationType.Arpeggio,
        InputPolicies:
        [
            new WantsToOrnament(_weightedRandomBooleanGenerator, configuration.Probability),
            _hasNoOrnamentation,
            scaleDegreePolicy,
            new Not<OrnamentationItem>(new HasTargetOrnamentation(OrnamentationType.Arpeggio)),
            .. translations.Distinct().Select(translation => new IsIntervalWithinInstrumentRange(_compositionConfiguration, translation))
        ],
        OutputPolicies: [new LogOrnamentation(configuration.OrnamentationType, _logger)],
        Translations: [.. translations],
        ShouldNotInvert,
        TranslationInversionIndices: new HashSet<int>().ToFrozenSet()
    );
}
