using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using Melanchall.DryWetMidi.Interaction;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Compositions.Configurations;

/// <summary>
///     The composition configuration.
/// </summary>
/// <param name="VoiceConfigurations"> The voice configurations to be used in the composition. </param>
/// <param name="PhrasingConfiguration"> The phrasing configuration to be used in the composition. </param>
/// <param name="AggregateCompositionRuleConfiguration"> The configuration of the composition rules to use in the composition. </param>
/// <param name="Scale"> The scale to be used in the composition. </param>
/// <param name="CompositionLength"> The length of the composition in measures. </param>
/// <param name="Meter"> The meter to be used in the composition. </param>
/// <param name="DefaultNoteTimeSpan"> The default note time span to be used in the composition. </param>
/// <param name="CompositionContextSize"> The size of the context to be used in the composition. </param>
/// <param name="Tempo"> The tempo of the composition, in beats per minute. </param>
internal sealed record CompositionConfiguration(
    ISet<VoiceConfiguration> VoiceConfigurations,
    PhrasingConfiguration PhrasingConfiguration,
    AggregateCompositionRuleConfiguration AggregateCompositionRuleConfiguration,
    BaroquenScale Scale,
    Meter Meter,
    MusicalTimeSpan DefaultNoteTimeSpan,
    int CompositionLength,
    int CompositionContextSize = 8,
    int Tempo = 120)
{
    public IDictionary<Voice, VoiceConfiguration> VoiceConfigurationsByVoice { get; } = VoiceConfigurations.ToDictionary(
        voiceConfiguration => voiceConfiguration.Voice
    );

    public List<Voice> Voices { get; } = VoiceConfigurations.Select(static voiceConfiguration => voiceConfiguration.Voice).ToList();

    public int BeatsPerMeasure => Meter.BeatsPerMeasure();

    /// <summary>
    ///     Determine if the given note is within the range of the given voice for the composition.
    /// </summary>
    /// <param name="voice">The voice to check the note against.</param>
    /// <param name="note">The note to check against the voice.</param>
    /// <returns>Whether the note is within the range of the voice.</returns>
    public bool IsNoteInVoiceRange(Voice voice, Note note) => VoiceConfigurationsByVoice[voice].IsNoteWithinVoiceRange(note);
}
