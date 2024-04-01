using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;

internal class PassingToneProcessor(IMusicalTimeSpanCalculator musicalTimeSpanCalculator, CompositionConfiguration configuration) : IProcessor<OrnamentationItem>
{
    public void Process(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Voice];
        var nextNote = item.NextBeat?[item.Voice];

        var notes = configuration.Scale.GetNotes().ToList();

        var currentScaleIndex = notes.IndexOf(currentNote.Raw);
        var nextScaleIndex = notes.IndexOf(nextNote?.Raw ?? currentNote.Raw);

        var passingToneIndex = currentScaleIndex > nextScaleIndex ? currentScaleIndex - 1 : currentScaleIndex + 1;
        var passingToneNote = notes[passingToneIndex];

        currentNote.Duration = musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(OrnamentationType.PassingTone, configuration.Meter);

        currentNote.Ornamentations.Add(new BaroquenNote(currentNote.Voice, passingToneNote)
        {
            Duration = musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(OrnamentationType.PassingTone, configuration.Meter)
        });
    }
}
