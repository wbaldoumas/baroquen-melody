using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Tests.TestData;

internal static class Configurations
{
    public static CompositionConfiguration GetCompositionConfiguration(
        int numberOfVoices = 4,
        int compositionLength = 100
    ) => new(
        GenerateVoiceConfigurations(numberOfVoices),
        PhrasingConfiguration.Default,
        AggregateCompositionRuleConfiguration.Default,
        AggregateOrnamentationConfiguration.Default,
        BaroquenScale.Parse("C Major"),
        Meter.FourFour,
        MusicalTimeSpan.Half,
        CompositionLength: compositionLength
    );

    private static HashSet<VoiceConfiguration> GenerateVoiceConfigurations(int numberOfVoices) => numberOfVoices switch
    {
        1 =>
        [
            new VoiceConfiguration(Voice.Soprano, Notes.C4, Notes.C6)
        ],
        2 =>
        [
            new VoiceConfiguration(Voice.Soprano, Notes.C4, Notes.C6),
            new VoiceConfiguration(Voice.Alto, Notes.G2, Notes.G4)
        ],
        3 =>
        [
            new VoiceConfiguration(Voice.Soprano, Notes.C4, Notes.C6),
            new VoiceConfiguration(Voice.Alto, Notes.G2, Notes.G4),
            new VoiceConfiguration(Voice.Tenor, Notes.C2, Notes.C3)
        ],
        4 =>
        [
            new VoiceConfiguration(Voice.Soprano, Notes.C4, Notes.C6),
            new VoiceConfiguration(Voice.Alto, Notes.G2, Notes.G4),
            new VoiceConfiguration(Voice.Tenor, Notes.C2, Notes.C3),
            new VoiceConfiguration(Voice.Bass, Notes.C1, Notes.C2)
        ],
        _ => throw new ArgumentException("Invalid number of voices.")
    };
}
