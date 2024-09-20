using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Infrastructure.Serialization.JsonConverters;
using LazyCart;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using System.Collections.Frozen;
using System.Text.Json.Serialization;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Compositions.Configurations;

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
    int Tempo = 120)
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

    public List<Instrument> Instruments { get; } = InstrumentConfigurations.Select(static instrumentConfiguration => instrumentConfiguration.Instrument).ToList();

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
