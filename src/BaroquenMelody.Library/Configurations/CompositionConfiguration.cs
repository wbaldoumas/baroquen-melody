using BaroquenMelody.Library.Configurations.Serialization.JsonConverters;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Enums.Extensions;
using BaroquenMelody.Library.MusicTheory.Enums;
using LazyCart;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections.Frozen;
using System.Text.Json.Serialization;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Configurations;

/// <summary>
///     The composition configuration.
/// </summary>
/// <param name="InstrumentConfigurations"> The instrument configurations to be used in the composition. </param>
/// <param name="PhrasingConfiguration"> The phrasing configuration to be used in the composition. </param>
/// <param name="AggregateCompositionRuleConfiguration"> The configuration of the composition rules to use in the composition. </param>
/// <param name="AggregateOrnamentationConfiguration"> The configuration of the ornamentations to use in the composition. </param>
/// <param name="Tonic"> The tonic note of the composition. </param>
/// <param name="Mode"> The mode of the composition. </param>
/// <param name="MinimumMeasures"> The length of the composition in measures. </param>
/// <param name="Meter"> The meter to be used in the composition. </param>
/// <param name="DefaultNoteTimeSpan"> The default note time span to be used in the composition. </param>
/// <param name="CompositionContextSize"> The size of the context to be used in the composition. </param>
/// <param name="Tempo"> The tempo of the composition, in beats per minute. </param>
/// <param name="ShuffleOrnamentationProcessors"> Whether to shuffle the ornamentation processor order between beats. Defaults to <see langword="true"/> (production variety). The shuffle draws from its own random provider, so under a seed it is reproducible on its own and never shifts the composition's shared draw stream; set to <see langword="false"/> to keep the configured processor order at every beat. </param>
/// <param name="MaxLookAheadDepth"> How many chords ahead the composition strategy searches to avoid dead-ends. Defaults to 1 (the prior hardcoded behavior); higher values constrain choices more strictly at a search-cost premium. </param>
/// <param name="AggregateScoringRuleConfiguration"> The configuration of the scoring rules used to rank rule-passing candidate chords. When <see langword="null"/> (including configurations saved before scoring existed), <see cref="Configurations.AggregateScoringRuleConfiguration.Default"/> is used. </param>
/// <param name="MotifDevelopmentConfiguration"> The configuration of how recurring themes/phrases are developed rather than repeated verbatim. When <see langword="null"/> (including configurations saved before motivic development existed), <see cref="Configurations.MotifDevelopmentConfiguration.Default"/> is used. </param>
/// <param name="HarmonicRhythmConfiguration"> The configuration of how the harmonic rhythm of the composition body varies. When <see langword="null"/> (including configurations saved before variable harmonic rhythm existed), <see cref="Configurations.HarmonicRhythmConfiguration.Default"/> is used. </param>
/// <param name="SuspensionConfiguration"> The configuration of prepared suspensions at strong-beat harmonic changes. When <see langword="null"/> (including configurations saved before suspensions existed), <see cref="Configurations.SuspensionConfiguration.Default"/> is used. </param>
/// <param name="TonicizationConfiguration"> The configuration of tonicization (raised thirds turning minor triads into true dominants of the chords they approach). When <see langword="null"/> (including configurations saved before tonicization existed), <see cref="Configurations.TonicizationConfiguration.Default"/> is used. </param>
/// <param name="GroundBassConfiguration"> The configuration of the ground bass form (a repeating bass pattern under freshly composed variations, replacing the fugal form). When <see langword="null"/> (including configurations saved before the form existed), <see cref="Configurations.GroundBassConfiguration.Default"/> is used. </param>
/// <param name="VoiceRhythmConfiguration"> The configuration of per-voice rhythm roles (a held voice and a florid voice rotating over phrase blocks of the fugal body; also the master switch for the ground bass form's divisions). When <see langword="null"/> (including configurations saved before voice rhythm existed), <see cref="Configurations.VoiceRhythmConfiguration.Default"/> is used. </param>
/// <param name="TextureConfiguration"> The configuration of the fugal body's accompaniment texture (melody over walking, broken-chord, or chordal accompaniment; master-switched by the voice-rhythm configuration). When <see langword="null"/> (including configurations saved before textures existed), <see cref="Configurations.TextureConfiguration.Default"/> is used. </param>
public sealed record CompositionConfiguration(
    ISet<InstrumentConfiguration> InstrumentConfigurations,
    PhrasingConfiguration PhrasingConfiguration,
    AggregateCompositionRuleConfiguration AggregateCompositionRuleConfiguration,
    AggregateOrnamentationConfiguration AggregateOrnamentationConfiguration,
    NoteName Tonic,
    Mode Mode,
    Meter Meter,
    [property: JsonConverter(typeof(MusicalTimespanJsonConverter))]
    MusicalTimeSpan DefaultNoteTimeSpan,
    int MinimumMeasures,
    int CompositionContextSize = 4,
    int Tempo = 120,
    bool ShuffleOrnamentationProcessors = true,
    int MaxLookAheadDepth = 1,
    AggregateScoringRuleConfiguration? AggregateScoringRuleConfiguration = null,
    MotifDevelopmentConfiguration? MotifDevelopmentConfiguration = null,
    HarmonicRhythmConfiguration? HarmonicRhythmConfiguration = null,
    SuspensionConfiguration? SuspensionConfiguration = null,
    TonicizationConfiguration? TonicizationConfiguration = null,
    GroundBassConfiguration? GroundBassConfiguration = null,
    VoiceRhythmConfiguration? VoiceRhythmConfiguration = null,
    TextureConfiguration? TextureConfiguration = null)
{
    public const int MaxScaleStepChange = 5;

    public const int MinInstrumentRange = 7;

    public const int MaxInstrumentRange = 21;

    public FrozenDictionary<Instrument, InstrumentConfiguration> InstrumentConfigurationsByInstrument { get; } = InstrumentConfigurations.ToFrozenDictionary(
        instrumentConfiguration => instrumentConfiguration.Instrument
    );

    public LazyCartesianProduct<Instrument, Instrument> InstrumentPairs { get; } = new(
        InstrumentConfigurations.Select(instrumentConfiguration => instrumentConfiguration.Instrument).ToList(),
        InstrumentConfigurations.Select(instrumentConfiguration => instrumentConfiguration.Instrument).ToList()
    );

    // The enum tie-break keeps this order a pure function of the configuration when two instruments share a
    // MinNote: the stable sort would otherwise fall back to the set's enumeration order, and this order is
    // draw-bearing (the dynamics pass walks it with per-instrument draws).
    public List<Instrument> Instruments { get; } = InstrumentConfigurations
        .OrderByDescending(static instrumentConfiguration => instrumentConfiguration.MinNote)
        .ThenBy(static instrumentConfiguration => instrumentConfiguration.Instrument)
        .Select(static instrumentConfiguration => instrumentConfiguration.Instrument)
        .ToList();

    public int BeatsPerMeasure => Meter.BeatsPerMeasure();

    public BaroquenScale Scale { get; } = new(Tonic, Mode);

    /// <summary>
    ///     Determine if the given note is within the range of the given instrument for the composition.
    /// </summary>
    /// <param name="instrument">The instrument to check the note against.</param>
    /// <param name="note">The note to check against the instrument.</param>
    /// <returns>Whether the note is within the range of the instrument.</returns>
    public bool IsNoteInInstrumentRange(Instrument instrument, Note note) => InstrumentConfigurationsByInstrument[instrument].IsNoteWithinInstrumentRange(note);
}
