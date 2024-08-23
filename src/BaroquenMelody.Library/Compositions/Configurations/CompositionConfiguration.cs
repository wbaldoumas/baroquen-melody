using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using Melanchall.DryWetMidi.Interaction;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Compositions.Configurations;

/// <summary>
///     The composition configuration.
/// </summary>
/// <param name="InstrumentConfigurations"> The instrument configurations to be used in the composition. </param>
/// <param name="PhrasingConfiguration"> The phrasing configuration to be used in the composition. </param>
/// <param name="AggregateCompositionRuleConfiguration"> The configuration of the composition rules to use in the composition. </param>
/// <param name="AggregateOrnamentationConfiguration"> The configuration of the ornamentations to use in the composition. </param>
/// <param name="Scale"> The scale to be used in the composition. </param>
/// <param name="CompositionLength"> The length of the composition in measures. </param>
/// <param name="Meter"> The meter to be used in the composition. </param>
/// <param name="DefaultNoteTimeSpan"> The default note time span to be used in the composition. </param>
/// <param name="CompositionContextSize"> The size of the context to be used in the composition. </param>
/// <param name="Tempo"> The tempo of the composition, in beats per minute. </param>
public sealed record CompositionConfiguration(
    ISet<InstrumentConfiguration> InstrumentConfigurations,
    PhrasingConfiguration PhrasingConfiguration,
    AggregateCompositionRuleConfiguration AggregateCompositionRuleConfiguration,
    AggregateOrnamentationConfiguration AggregateOrnamentationConfiguration,
    BaroquenScale Scale,
    Meter Meter,
    MusicalTimeSpan DefaultNoteTimeSpan,
    int CompositionLength,
    int CompositionContextSize = 8,
    int Tempo = 120)
{
    public const int MaxScaleStepChange = 5;

    public IDictionary<Instrument, InstrumentConfiguration> InstrumentConfigurationsByInstrument { get; } = InstrumentConfigurations.ToDictionary(
        instrumentConfiguration => instrumentConfiguration.Instrument
    );

    public List<Instrument> Instruments { get; } = InstrumentConfigurations.Select(static instrumentConfiguration => instrumentConfiguration.Instrument).ToList();

    public int BeatsPerMeasure => Meter.BeatsPerMeasure();

    /// <summary>
    ///     Determine if the given note is within the range of the given instrument for the composition.
    /// </summary>
    /// <param name="instrument">The instrument to check the note against.</param>
    /// <param name="note">The note to check against the instrument.</param>
    /// <returns>Whether the note is within the range of the instrument.</returns>
    public bool IsNoteInInstrumentRange(Instrument instrument, Note note) => InstrumentConfigurationsByInstrument[instrument].IsNoteWithinInstrumentRange(note);
}
