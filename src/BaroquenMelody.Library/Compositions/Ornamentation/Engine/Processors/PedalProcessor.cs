using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

internal sealed class PedalProcessor(
    IMusicalTimeSpanCalculator musicalTimeSpanCalculator,
    CompositionConfiguration configuration,
    int pedalInterval
) : IProcessor<OrnamentationItem>
{
    public const int Interval = 2;

    public const int RootPedalInterval = -3;

    public const int ThirdPedalInterval = -2;

    public const int FifthPedalInterval = -4;

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var nextNote = item.NextBeat![item.Voice];

        var notes = configuration.Scale.GetNotes();

        var currentNoteIndex = notes.IndexOf(currentNote.Raw);
        var nextNoteIndex = notes.IndexOf(nextNote.Raw);

        var isDescending = currentNoteIndex > nextNoteIndex;

        var ornamentationNotes = new[]
        {
            notes[currentNoteIndex + pedalInterval],
            notes[isDescending ? currentNoteIndex - 1 : currentNoteIndex + 1],
            notes[currentNoteIndex + pedalInterval]
        };

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.Pedal, configuration.Meter);

        var ornamentationDuration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.Pedal, configuration.Meter);

        foreach (var note in ornamentationNotes)
        {
            currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, note)
            {
                Duration = ornamentationDuration
            });
        }

        currentNote.OrnamentationType = OrnamentationType.Pedal;
    }
}
