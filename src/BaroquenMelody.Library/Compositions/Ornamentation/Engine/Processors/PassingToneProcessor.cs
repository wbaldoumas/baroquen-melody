using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

internal class PassingToneProcessor(IMusicalTimeSpanCalculator musicalTimeSpanCalculator, CompositionConfiguration configuration) : IProcessor<OrnamentationItem>
{
    public const int Interval = 2;

    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var nextNote = item.NextBeat![item.Voice];

        var notes = configuration.Scale.GetNotes().ToList();

        var currentNoteScaleIndex = notes.IndexOf(currentNote.Raw);
        var nextNoteScaleIndex = notes.IndexOf(nextNote.Raw);

        var passingToneScaleIndex = currentNoteScaleIndex > nextNoteScaleIndex ? currentNoteScaleIndex - 1 : currentNoteScaleIndex + 1;
        var passingTone = notes[passingToneScaleIndex];

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.PassingTone, configuration.Meter);

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, passingTone)
        {
            Duration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.PassingTone, configuration.Meter)
        });
    }
}
